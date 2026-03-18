using System;
using Sapientia;
using Sapientia.Evaluators;
using Sapientia.Pooling;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Trading
{
	[Serializable]
	public partial class WeightedTradeRewardCollection : TradeReward
	{
		public const string ERROR_CATEGORY = "Collection";

		// ReSharper disable once UseArrayEmptyMethod
		public TradeWeightedRewardOption[] items = new TradeWeightedRewardOption[0];
		public EvaluatedValue<Blackboard, int> count = 1;
#if CLIENT
		[ShowIf(nameof(CanShowRollMode))]
#endif
		public WeightRollManyMode rollMode;

		protected override bool CanReceive(Tradeboard board, out TradeReceiveError? error)
		{
			var randomizer = board.Get<IRandomizer<int>>();

			var evaluatedCount = count.Evaluate(board);
			foreach (var index in items.RollMany<TradeWeightedRewardOption, Blackboard>(board, rollMode, evaluatedCount, randomizer))
			{
				if (!items[index].reward.CanExecute(board, out error))
					return false;
			}

			error = null;
			return true;
		}

		protected override bool Receive(Tradeboard board)
		{
			var randomizer = board.Get<IRandomizer<int>>();
			var evaluatedCount = count.Evaluate(board);
			foreach (var index in items.RollMany<TradeWeightedRewardOption, Blackboard>(board, rollMode, evaluatedCount, randomizer))
			{
				if (!items[index].reward.Execute(board))
					return false;
			}

			return true;
		}

		private int SortByPriority(TradeReward x, TradeReward y) => y.Priority.CompareTo(x.Priority);
	}

	[Serializable]
	public class TradeWeightedRewardOption : IWeightable<Blackboard>
	{
		public EvaluatedValue<Blackboard, int> weight = 1;

		[SerializeReference]
		public TradeReward reward;

		int IWeightable.Weight { get => weight.IsConstant ? weight.value : -1; }
		public EvaluatedValue<Blackboard, int> EvaluatedWeight { get => weight; }
	}
}
