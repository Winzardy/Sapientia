using System;
using System.Collections.Generic;
using Sapientia;
using Sapientia.Deterministic;
#if CLIENT
using Sirenix.OdinInspector;
using UnityEngine;
#endif

namespace Trading
{
	[Serializable]
	public partial class TradeRewardByChance : TradeReward
	{
		private static readonly Fix64 MAX_CHANCE = Fix64.One;
#if CLIENT
		[PropertyRange(0, 1)]
#endif
		public Fix64 chance;

		[SerializeReference]
		public TradeReward reward;

		protected override bool CanReceive(Tradeboard board, out TradeReceiveError? error)
			=> reward.CanExecute(board, out error);

		protected override bool Receive(Tradeboard board)
		{
			var randomizer = board.Get<IRandomizer<Fix64>>();
			var roll = randomizer.Next(0, MAX_CHANCE);

			if (roll <= chance)
				return reward.Execute(board);

			return true;
		}

		#region Enumerate

		public override IEnumerable<TradeReward> EnumerateActual(Tradeboard board)
		{
			var randomizer = board.Get<IRandomizer<Fix64>>();
			var roll = randomizer.Next(0, MAX_CHANCE);

			if (roll <= chance)
				foreach (var actualReward in reward.EnumerateActual(board))
					yield return actualReward;
		}

		#endregion
	}
}
