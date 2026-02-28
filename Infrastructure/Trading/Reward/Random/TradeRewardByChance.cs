using System;
using System.Collections.Generic;
using Sapientia;
using Sapientia.Deterministic;
using Sapientia.Evaluators;
#if CLIENT
using Fusumity.Attributes;
using UnityEngine;
#endif

namespace Trading
{
	[Serializable]
	public partial class TradeRewardByChance : TradeReward
	{
		private static readonly Fix64 MAX_CHANCE = Fix64.One;

#if CLIENT
		[Tooltip("Вероятность от 0 до 1")]
		[PropertyRangeParent(0, 1)]
#endif
		public EvaluatedValue<Blackboard, Fix64> chance;

		[SerializeReference]
		public TradeReward reward;

		protected override bool CanReceive(Tradeboard board, out TradeReceiveError? error)
			=> reward.CanExecute(board, out error);

		protected override bool Receive(Tradeboard board)
		{
			var randomizer = board.Get<IRandomizer<Fix64>>();
			var roll = randomizer.Next(0, MAX_CHANCE);

			if (roll <= chance.Evaluate(board))
				return reward.Execute(board);

			return true;
		}

		#region Enumerate

		protected internal override IEnumerable<TradeReward> OnEnumerateActual(Tradeboard board)
		{
			var randomizer = board.Get<IRandomizer<Fix64>>();
			var roll = randomizer.Next(0, MAX_CHANCE);

			if (roll <= chance.Evaluate(board))
				foreach (var actualReward in reward.OnEnumerateActual(board))
					yield return actualReward;
		}

		#endregion
	}
}
