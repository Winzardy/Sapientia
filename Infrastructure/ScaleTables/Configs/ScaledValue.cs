using Content;
using Newtonsoft.Json;
using Sapientia.Collections;
using Sapientia.Deterministic;
using Sapientia.Extensions;

namespace Sapientia.ScaleTables
{
	[System.Serializable]
	public struct ScaledValue
	{
		public string scaleTableId;

		public int rowIndex;
		public bool interpolate;

		/// <summary>
		/// Identifier, usually last part of key.
		/// Might be same as key.
		/// <br></br>
		/// <i>Example: Agility</i>
		/// </summary>
		[JsonIgnore]
		public string Identifier { get { return TryGetRow(out var row) ? row.identifier : null; } }

		/// <summary>
		/// Full key for row, may represent path.
		/// <br></br>
		/// <i>Example: Stats/Bob/Agility</i>
		/// </summary>
		[JsonIgnore]
		public string Key { get { return TryGetRow(out var row) ? row.key : null; } }

		[JsonIgnore]
		public bool IsEmpty { get { return scaleTableId.IsNullOrEmpty(); } }

		public Fix64 this[int scale]
		{
			get { return this[(Fix64)scale]; }
		}

		public Fix64 this[Fix64 scale]
		{
			get
			{
				return
					TryGetValue(scale, out var endValue) ?
					endValue :
					Fix64.Zero;
			}
		}

		public int GetInt(int scale)
		{
			return (int)this[scale];
		}

		public bool TryGetValue(Fix64 scale, out Fix64 value)
		{
			if (!TryGetTable(out var table))
			{
				value = default;
				return false;
			}

			if (!TryGetRow(table, out var row))
			{
				value = default;
				return false;
			}

			value = GetScaledValue(scale, table.scaleRow, row);
			return true;
		}

		public bool TryGetValue(int x, out Fix64 value)
		{
			return TryGetValue((Fix64)x, out value);
		}

		public bool TryGetRow(out ScaleTableRow row)
		{
			if (!TryGetTable(out var table))
			{
				row = default;
				return false;
			}

			return TryGetRow(table, out row);
		}

		public bool TryGetRow(ScaleTableConfig table, out ScaleTableRow row)
		{
			if (!table.valueRows.WithinBounds(rowIndex))
			{
				row = default;
				return false;
			}

			row = table.valueRows[rowIndex];
			return true;
		}

		public bool TryGetTable(out ScaleTableConfig table)
		{
			if (scaleTableId.IsNullOrEmpty())
			{
				table = null;
				return false;
			}

			return ContentManager.TryGet(scaleTableId, out table);
		}

		private Fix64 GetScaledValue(Fix64 scale, in ScaleTableRow scalesRow, in ScaleTableRow valuesRow)
		{
			return GetScaledValue(scale, scalesRow, valuesRow, interpolate);
		}

		public static Fix64 GetScaledValue(Fix64 scale, in ScaleTableRow scalesRow, in ScaleTableRow valuesRow, bool interpolate)
		{
			if (scale <= 0)
			{
				return
					scalesRow.First == Fix64.Zero ?
					valuesRow.First :
					Fix64.Zero;
			}

			if (scale >= scalesRow.Last)
			{
				return valuesRow.Last;
			}

			for (int i = scalesRow.Count; i-- > 0;)
			{
				var previousScaleValue = scalesRow[i];

				if (scale == previousScaleValue)
					return valuesRow[i];

				if (scale > previousScaleValue)
				{
					if (interpolate)
					{
						var nextScaleValue = scalesRow[i + 1];

						var lowerBound = valuesRow[i];
						var upperBound = valuesRow[i + 1];

						var f = (scale - previousScaleValue) / (nextScaleValue - previousScaleValue);
						return Fix64.LerpClamped(lowerBound, upperBound, f);
					}
					else
					{
						return valuesRow[i];
					}
				}
			}

			return Fix64.Zero;
		}

		public override string ToString()
		{
			return
				$"Scale Table: [ {scaleTableId} ] " +
				$"Row Index: [ {rowIndex} ]";
		}
	}
}
