using System;
using Sapientia.Conditions;

#if CLIENT
using UnityEngine;
#endif

namespace Trading
{
	[Serializable]
	public partial class IfElseTradeCost : TradeCost
	{
		[SerializeReference]
		public Condition condition;

		[SerializeReference]
		public TradeCost a;

		[SerializeReference]
		public TradeCost b = new NotAvailableTradeCost();

		protected override bool CanPay(Tradeboard board, out TradePayError? error)
		{
			return condition.IsMet(board)
				? a.CanExecute(board, out error)
				: b.CanExecute(board, out error);
		}

		protected override bool Pay(Tradeboard board)
		{
			if (condition.IsMet(board))
				return a.Execute(board);

			return b.Execute(board);
		}
	}
}
