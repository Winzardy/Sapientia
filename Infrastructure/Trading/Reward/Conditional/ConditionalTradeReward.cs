using System;
using System.Collections.Generic;
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
		public Condition<Blackboard> condition = new ObjectProviderBlackboardProxyEvaluator();

		[SerializeReference]
		public TradeReward reward;

		protected override bool CanReceive(Tradeboard board, out TradeReceiveError? error)
			=> reward.CanExecute(board, out error);

		protected override bool Receive(Tradeboard board)
			=> !condition.IsFulfilled(board) || reward.Execute(board);

		protected internal override IEnumerable<TradeReward> EnumerateActual(Tradeboard board)
		{
			if (!condition.IsFulfilled(board))
				yield break;

			foreach (var actualReward in reward.EnumerateActual(board))
				yield return actualReward;
		}
	}
}
