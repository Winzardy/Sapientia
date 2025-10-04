using System;
using Sapientia;

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
			items.Roll(randomizer, out var index);
			return items[index].reward.CanExecute(board, out error);
		}

		protected override bool Receive(Tradeboard board)
		{
			var randomizer = board.Get<IRandomizer<int>>();
			items.Roll(randomizer, out var index);
			return items[index].reward.Execute(board);
		}

		private int SortByPriority(TradeReward x, TradeReward y) => y.Priority.CompareTo(x.Priority);
	}

	[Serializable]
	public struct WeightedReward : IWeightable
	{
		public int Weight => weight;

		public int weight;

		[SerializeReference]
		public TradeReward reward;
	}
}
