using Sapientia;
using Sapientia.Collections;

namespace Trading
{
	public sealed partial class TradeCostOptions : TradeCost, IOptions<TradeCost>
	{
		internal int selectedIndex;

		// ReSharper disable once UseArrayEmptyMethod
#if CLIENT
		[UnityEngine.SerializeReference]
#endif
		public TradeCost[] options = new TradeCost[0];

		public TradeCost[] Items => options;
		public TradeCost[] Options => options;

		TradeCost[] IOptions<TradeCost>.Options => options;

		public ref readonly TradeCost this[int index] => ref options[index];

		protected override bool CanPay(Tradeboard board, out TradePayError? error) => options[selectedIndex]
		   .CanExecute(board, out error);

		protected override bool Pay(Tradeboard board) => options[selectedIndex]
		   .Execute(board);

		public bool TrySelect(int index)
		{
			if (options.ContainsIndexSafe(index))
				return false;

			selectedIndex = index;
			return true;
		}
	}
}
