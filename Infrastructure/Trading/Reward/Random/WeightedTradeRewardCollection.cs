using System;
using Sapientia;
using Sapientia.Evaluators;
using Sapientia.Pooling;
using UnityEngine;

namespace Trading
{
	[Serializable]
	public partial class WeightedTradeRewardCollection : TradeReward
	{
		public enum RollMode
		{
			Default,

			[Tooltip("Выдаёт награды без повторений, если запрошенное количество меньше или равно размеру списка")]
			Sequence
		}

		public const string ERROR_CATEGORY = "Collection";

		// ReSharper disable once UseArrayEmptyMethod
		public WeightedReward[] items = new WeightedReward[0];

		public WeightedReward[] Items => items;

		public EvaluatedValue<Blackboard, int> count = 1;

#if CLIENT
		[Sirenix.OdinInspector.ShowIf(nameof(CanShowRollMode))]
#endif
		public RollMode rollMode;

		protected override bool CanReceive(Tradeboard board, out TradeReceiveError? error)
		{
			var randomizer = board.Get<IRandomizer<int>>();

			var evaluatedCount = count.Evaluate(board);
			switch (rollMode)
			{
				case RollMode.Default:

					for (int i = 0; i < evaluatedCount; i++)
					{
						if (items.Roll<WeightedReward, Blackboard>(board, randomizer, out var index))
							if (!items[index].reward.CanExecute(board, out error))
								return false;
					}

					break;
				case RollMode.Sequence:

					using (ListPool<WeightedReward>.Get(out var list))
					{
						list.AddRange(items);

						for (int i = 0; i < evaluatedCount; i++)
						{
							if (list.Roll<WeightedReward, Blackboard>(board, randomizer, out var index))
							{
								var item = list[index];
								list.RemoveAt(index);
								if (!item.reward.CanExecute(board, out error))
									return false;
							}

							if (list.Count == 0)
								list.AddRange(items);
						}
					}

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			error = null;
			return true;
		}

		protected override bool Receive(Tradeboard board)
		{
			var randomizer = board.Get<IRandomizer<int>>();
			// if (items.Roll<WeightedReward, Blackboard>(board, randomizer, out var index))
			// {
			// 	return items[index].reward.Execute(board);
			// }

			var evaluatedCount = count.Evaluate(board);
			switch (rollMode)
			{
				case RollMode.Default:

					for (int i = 0; i < evaluatedCount; i++)
					{
						if (items.Roll<WeightedReward, Blackboard>(board, randomizer, out var index))
							if (!items[index].reward.Execute(board))
								return false;
					}

					break;
				case RollMode.Sequence:

					using (ListPool<WeightedReward>.Get(out var list))
					{
						list.AddRange(items);

						for (int i = 0; i < evaluatedCount; i++)
						{
							if (list.Roll<WeightedReward, Blackboard>(board, randomizer, out var index))
							{
								var item = list[index];
								list.RemoveAt(index);
								if (!item.reward.Execute(board))
									return false;
							}

							if (list.Count == 0)
								list.AddRange(items);
						}
					}

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return true;
		}

		private int SortByPriority(TradeReward x, TradeReward y) => y.Priority.CompareTo(x.Priority);
	}

	[Serializable]
	public class WeightedReward : IWeightableWithEvaluator<Blackboard>
	{
		public EvaluatedValue<Blackboard, int> weight = 1;

		[SerializeReference]
		public TradeReward reward;

		public EvaluatedValue<Blackboard, int> Weight => weight;
	}
}
