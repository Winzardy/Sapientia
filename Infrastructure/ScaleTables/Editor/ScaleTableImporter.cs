#if UNITY_EDITOR
using Content.ScriptableObjects;
using Content.ScriptableObjects.ScaleTables;
using NReco.Csv;
using Sapientia.Deterministic;
using Sapientia.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Sapientia.ScaleTables.Editor
{
	[ScriptedImporter(version: 1, ext: "slt")]
	public class ScaleTableImporter : ScriptedImporter
	{
		public const string DELIMITER = ",";

		public override void OnImportAsset(AssetImportContext ctx)
		{
			var id = Path.GetFileNameWithoutExtension(ctx.assetPath);
			if (id.IsNullOrEmpty())
			{
				ctx.LogImportError($"Id for scale table is empty");
				return;
			}

			var tableRows = new List<ScaleTableRow>();

			using (var streamRdr = new StreamReader(ctx.assetPath))
			{
				int lineIndex = 0;
				int valuesRowSize = 0;

				var valuesBuffer = new List<Fix64>();
				var reader = new CsvReader(streamRdr, DELIMITER);

				while (reader.Read())
				{
					valuesBuffer.Clear();

					if (reader.IsEmptyLine())
					{
						ctx.LogImportError(
							"Invalid scale table format - " +
							$"empty line detected - [ {lineIndex} ]");

						lineIndex++;
						continue;
					}

					if (lineIndex == 0 && !IsValidScaleRow(reader))
					{
						ctx.LogImportError(
							"Invalid scale table format - " +
							"make sure scale row exists");

						lineIndex++;
						continue;
					}

					if (!IsValidRowKey(reader))
					{
						ctx.LogImportError(
							"Invalid scale table format - " +
							"make sure row has identifier");

						lineIndex++;
						continue;
					}

					if (lineIndex == 0)
					{
						valuesRowSize = reader.FieldsCount;
					}

					var rowKey = reader[0];
					var parsedValue = Fix64.Zero;

					for (int i = 1; i < valuesRowSize; i++)
					{
						if (lineIndex == 0 && reader[i].IsNullOrWhiteSpace())
						{
							ctx.LogImportError(
								$"Invalid scale table format - " +
								$"Scale row contains " +
								$"empty cell at index [ {i} ]");

							valuesRowSize = i;
							break;
						}

						// will parse all the added values
						// and fill remaining with the last parsed value
						// (in case row is only partially filled)
						if (i < reader.FieldsCount)
						{
							if (Fix64.TryParse(reader[i], out var parsed))
							{
								parsedValue = parsed;
							}
						}

						valuesBuffer.Add(parsedValue);
					}

					var row = new ScaleTableRow
					{
						key = rowKey,
						identifier = Path.GetFileName(rowKey),
						values = valuesBuffer.ToArray()
					};

					tableRows.Add(row);
					lineIndex++;
				}
			}

			var table = ScriptableObject.CreateInstance<ScaleTableScriptableObject>();
			table.ForceCreateEntry();

			var config = new ScaleTableConfig
			{
				scaleRow = tableRows.First(),
				valueRows = tableRows.Skip(1).ToArray()
			};

			(config as IExternallyIdentifiable).SetId(id);

			table.SetValue(config, false);

			ctx.AddObjectToAsset(nameof(ScaleTableScriptableObject), table);
			ctx.SetMainObject(table);
		}

		private bool IsValidScaleRow(CsvReader reader)
		{
			return
				!reader[0].IsNullOrEmpty() &&
				reader[0] == ScaleTableKey.SCALE &&
				reader.FieldsCount > 1;
		}

		private bool IsValidRowKey(CsvReader reader)
		{
			return reader.FieldsCount > 0 &&
				!reader[0].IsNullOrWhiteSpace();
		}				
	}
}
#endif

