using System;
using Sapientia;
using Sapientia.Evaluators;

#if CLIENT
using UnityEngine;
#endif

namespace Trading
{
	[Serializable]
	public partial class WeightedTradeRewardCollection : TradeReward
	{
		public const string ERROR_CATEGORY = "Collection";

		// ReSharper disable once UseArrayEmptyMethod
		public WeightedReward[] items = new WeightedReward[0];

		public WeightedReward[] Items => items;

		protected override bool CanReceive(Tradeboard board, out TradeReceiveError? error)
		{
			var randomizer = board.Get<IRandomizer<int>>();
			if (items.Roll<WeightedReward, Blackboard>(board, randomizer, out var index))
				return items[index].reward.CanExecute(board, out error);

			error = null;
			return true;
		}

		protected override bool Receive(Tradeboard board)
		{
			var randomizer = board.Get<IRandomizer<int>>();
			if (items.Roll<WeightedReward, Blackboard>(board, randomizer, out var index))
				return items[index].reward.Execute(board);
			return true;
		}

		private int SortByPriority(TradeReward x, TradeReward y) => y.Priority.CompareTo(x.Priority);
	}

	[Serializable]
	public class WeightedReward : IWeightableWithEvaluator<Blackboard>
	{
		[SerializeReference]
		public Evaluator<Blackboard, int> weight = 0;

		[SerializeReference]
		public TradeReward reward;

		public Evaluator<Blackboard, int> Weight => weight;
	}
}
