using System;
using System.Collections.Generic;
using Sapientia;
using Sapientia.Conditions;
using Trading.Result;
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

		#region Enumerate

		public override IEnumerable<TradeReward> EnumerateActual(Tradeboard board)
		{
			if (!condition.IsFulfilled(board))
				yield break;

			foreach (var actualReward in reward.EnumerateActual(board))
				yield return actualReward;
		}

		public override IEnumerable<ITradeRewardResultHandle> EnumerateActualResult(Tradeboard board)
		{
			if (!condition.IsFulfilled(board))
				yield break;

			foreach (var result in reward.EnumerateActualResult(board))
				yield return result;
		}

		#endregion
	}
}
