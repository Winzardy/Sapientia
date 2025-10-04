using System;
using Sapientia;
using Sapientia.Conditions;

#if CLIENT
using UnityEngine;
#endif

namespace Trading
{
	[Serializable]
	public partial class ConditionalTradeReward : TradeReward
	{
		[SerializeReference]
		public Condition condition;

		[SerializeReference]
		public TradeReward reward;

		protected override bool CanReceive(Tradeboard board, out TradeReceiveError? error)
			=> reward.CanExecute(board, out error);

		protected override bool Receive(Tradeboard board)
			=> !condition.IsMet(board) || reward.Execute(board);
	}
}
