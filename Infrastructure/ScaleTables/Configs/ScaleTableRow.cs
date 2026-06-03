using Content;
using Sapientia.Collections;
using Sapientia.Deterministic;
using Sapientia.Extensions;

namespace Sapientia.ScaleTables
{
	[System.Serializable]
	public class ScaleTableRow
	{
		/// <summary>
		/// Full key for row, may represent path.
		/// </summary>
		public string key;
		/// <summary>
		/// Identifier, usually last part of key.
		/// Might be same as key.
		/// </summary>
		public string identifier;
		public Fix64[] values;

		public Fix64 this[int index]
		{
			get { return values[index]; }
		}

		public int Count { get { return values.IsNullOrEmpty() ? 0 : values.Length; } }
		public bool IsEmpty { get { return values.IsNullOrEmpty(); } }

		public Fix64 First { get { return values.First(); } }
		public Fix64 Last { get { return values.Last(); } }

		public ScaleTableRow()
		{
		}
		public ScaleTableRow(string key, string identifier, Fix64[] values)
		{
			this.key = key;
			this.identifier = identifier;
			this.values = values;
		}

		public override string ToString()
		{
			return
				$"Key: [ {key} ] " +
				$"Identifier: [ {identifier} ] " +
				$"\nValues: {values.GetCompositeString(vertical: false, numerate: false, separator: "|")}";
		}
	}
}
