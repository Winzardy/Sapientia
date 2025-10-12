using System;
using System.Collections.Generic;
using Content;
using Sapientia;
using Sapientia.Collections;
using Sapientia.Conditions;
using Sapientia.Extensions;

#if CLIENT
using UnityEngine;
#endif

namespace Trading
{
	[Serializable]
	public partial class TradeCostProgression : TradeCost, IResettableCost
	{
		private const string GROUP_CATALOG_ID = "TradeCostProgression";
		private const string KEY_FORMAT = "TradeCostProgression_{0}";
		private const string GUID_KEY_FORMAT = "{0}_{1}";

		[NonSerialized]
		private string _progressKeyCache;

		public ContentEntry<TradeCostProgressionStage[]> stages;

		/// <summary>
		/// Условие при котором цена прогрессирует, если 'None', то прогрессирует всегда
		/// </summary>
		[SerializeReference]
		public Condition<Blackboard> condition = new ObjectProviderBlackboardProxyEvaluator();

		/// <summary>
		/// Если использовать группу, то прогрессия будет связана по выбранной группе,
		/// например цена из одной группы будет отмечаться по этой группе
		/// </summary>
		[ContextLabel(GROUP_CATALOG_ID)]
		public Toggle<int> group;

		public TradeProgressionScheme autoReset;

		public void Reset(Tradeboard board)
		{
			var node = board.Get<ITradingNode>();
			node.ResetProgress(GetProgressKey(board.Id), in autoReset);
		}

		protected override bool CanPay(Tradeboard board, out TradePayError? error)
		{
			ref readonly var stage = ref GetCurrentStage(board);
			return stage.cost.CanExecute(board, out error);
		}

		protected override bool Pay(Tradeboard board)
		{
			ref readonly var stage = ref GetCurrentStage(board);
			var success = stage.cost
			   .Execute(board);

			if (success)
				TryIncrementStage(in stage, board);

			return success;
		}

		private ref readonly TradeCostProgressionStage GetCurrentStage(Tradeboard board)
		{
			var node = board.Get<ITradingNode>();
			var progress = node.GetCurrentProgress(GetProgressKey(board.Id), autoReset);
			var index = progress >= stages.Value.Length ? ^1 : progress;
			return ref stages.Value.GetValueByIndex(index);
		}

		private void TryIncrementStage(in TradeCostProgressionStage stage, Tradeboard board)
		{
			var node = board.Get<ITradingNode>();

			if (stage is {useOverrideCondition: true, overrideCondition: not null})
			{
				if (!stage.overrideCondition.IsFulfilled(board))
					return;
			}
			else if (condition != null && !condition.IsFulfilled(board))
			{
				return;
			}

			var progressKey = GetProgressKey(board.Id);
			node.IncrementProgress(progressKey, autoReset);
		}

		private string GetProgressKey(string tradeId)
		{
			return _progressKeyCache ??=
				group ? KEY_FORMAT.Format(group) : GUID_KEY_FORMAT.Format(tradeId, stages.Guid);
		}

		public override IEnumerable<TradeCost> EnumerateActual(Tradeboard board)
		{
			var stage = GetCurrentStage(board);
			foreach (var cost in stage.cost.EnumerateActual(board))
				yield return cost;
		}
	}

	[Serializable]
	public struct TradeCostProgressionStage
	{
		[SerializeReference]
		[TradeAccess(TradeAccessType.ByParent)]
		public TradeCost cost;

		/// <summary>
		/// Условие при котором на данном этапе прогрессирует
		/// </summary>
		public bool useOverrideCondition;

		/// <summary>
		/// Условие при котором на данном этапе прогрессирует
		/// </summary>
		[SerializeReference]
		public Condition<Blackboard> overrideCondition;
	}
}
