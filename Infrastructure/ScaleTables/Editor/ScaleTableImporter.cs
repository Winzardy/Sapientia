#if UNITY_EDITOR
using System.IO;
using System.Linq;
using Content.ScriptableObjects.Editor;
using Content.ScriptableObjects.ScaleTables;
using NReco.Csv;
using Sapientia.Deterministic;
using Sapientia.Extensions;
using Sapientia.Pooling;
using UnityEditor.AssetImporters;

namespace Sapientia.ScaleTables.Editor
{
	[ScriptedImporter(version: 1, ext: "slt")]
	public class ScaleTableImporter : ContentScriptedImporter<ScaleTableScriptableObject, ScaleTableConfig>
	{
		private const string DELIMITER = ",";

		protected override bool TryCreateValue(AssetImportContext ctx, out ScaleTableConfig value)
		{
			using (ListPool<ScaleTableRow>.Get(out var tableRows))
			using (ListPool<Fix64>.Get(out var valuesBuffer))
			{
				using (var streamRdr = new StreamReader(ctx.assetPath))
				{
					var lineIndex = 0;
					var valuesRowSize = 0;

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

						for (var i = 1; i < valuesRowSize; i++)
						{
							if (lineIndex == 0 && reader[i].IsNullOrWhiteSpace())
							{
								ctx.LogImportWarning(
									$"Scale row contains " +
									$"empty cell at index [ {i} ], " +
									$"turnicating to [ {i - 1} ]");

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

				value = new ScaleTableConfig
				{
					scaleRow = tableRows.First(),
					valueRows = tableRows.Skip(1).ToArray()
				};
				return true;
			}
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
