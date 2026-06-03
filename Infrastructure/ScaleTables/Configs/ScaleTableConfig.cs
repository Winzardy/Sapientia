using Sapientia.Extensions;
using System;

namespace Sapientia.ScaleTables
{
	[Serializable]
	public partial class ScaleTableConfig : IdentifiableConfig
	{
		public ScaleTableRow scaleRow;
		public ScaleTableRow[] valueRows;

		public override string ToString()
		{
			return
				base.ToString() +
				$"\nScaling: {scaleRow} " +
				$"\nRows: {valueRows.GetCompositeString()} ";
		}
	}
}
