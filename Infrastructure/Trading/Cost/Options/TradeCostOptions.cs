using System;
using System.Threading;
using System.Threading.Tasks;
using Sapientia;
using Sapientia.Collections;

namespace Trading
{
	[Serializable]
	public sealed partial class TradeCostOptions : TradeCost, IOptions<TradeCost>
	{
		internal int selectedIndex;

#if CLIENT
		[UnityEngine.SerializeReference]
#endif
		// ReSharper disable once UseArrayEmptyMethod
		public TradeCost[] options = new TradeCost[0];

		public TradeCost[] Items => options;
		public TradeCost[] Options => options;

		TradeCost[] IOptions<TradeCost>.Options => options;

		public ref readonly TradeCost this[int index] => ref options[index];

		public override bool CanPay(out TradePayError? error) => options[selectedIndex].CanPay(out error);

		protected override Task<bool> PayAsync(CancellationToken cancellationToken) => options[selectedIndex].ExecuteAsync(cancellationToken);

		public bool TrySelect(int index)
		{
			if (options.ContainsIndexSafe(index))
				return false;

			selectedIndex = index;
			return true;
		}
	}
}
