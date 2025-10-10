using System;
using System.Collections.Generic;
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
		public BlackboardCondition condition;

		[SerializeReference]
		[TradeAccess(TradeAccessType.ByParent)]
		public TradeCost a;

		[SerializeReference]
		[TradeAccess(TradeAccessType.ByParent)]
		public TradeCost b = new NotAvailableTradeCost();

		protected override bool CanPay(Tradeboard board, out TradePayError? error)
		{
			return condition.IsFulfilled(board)
				? a.CanExecute(board, out error)
				: b.CanExecute(board, out error);
		}

		protected override bool Pay(Tradeboard board)
		{
			if (condition.IsFulfilled(board))
				return a.Execute(board);

			return b.Execute(board);
		}

		public override IEnumerable<TradeCost> EnumerateActual(Tradeboard board)
		{
			if (condition.IsFulfilled(board))
			{
				foreach (var cost in a.EnumerateActual(board))
					yield return cost;
			}
			else
			{
				foreach (var cost in b.EnumerateActual(board))
					yield return cost;
			}
		}
	}
}
