using System;
using System.ComponentModel;
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
	public partial class TradeRewardProgression : TradeReward
	{
		private const string GROUP_CATALOG_ID = "TradeRewardProgression";
		private const string KEY_FORMAT = "TradeRewardProgression_{0}";
		private const string GUID_KEY_FORMAT = "{0}_{1}";

		[NonSerialized]
		private string _progressKeyCache;

		public ContentEntry<TradeRewardProgressionStage[]> stages;

		/// <summary>
		/// Условие при котором награда прогрессирует, если 'None', то прогрессирует всегда
		/// </summary>
		[SerializeReference]
		public Condition<Blackboard> condition = new ObjectProviderBlackboardProxyEvaluator();

		/// <summary>
		/// Если использовать группу, то прогрессия будет связана по выбранной группе,
		/// например награда из одной группы будет отмечаться по этой группе
		///
		/// <br/><br/>
		/// P.S так же работает в связке с ценами!
		/// </summary>
		[ContextLabel(GROUP_CATALOG_ID)]
		public Toggle<int> group;

		public TradeProgressionScheme autoReset;

		public void Reset(Tradeboard board)
		{
			var node = board.Get<ITradingNode>();
			node.ResetProgress(GetProgressKey(board.Id), in autoReset);
		}

		protected override bool CanReceive(Tradeboard board, out TradeReceiveError? error)
		{
			ref readonly var stage = ref GetCurrentStage(board);
			return stage.reward.CanExecute(board, out error);
		}

		protected override bool Receive(Tradeboard board)
		{
			ref readonly var stage = ref GetCurrentStage(board);
			var success = stage.reward
			   .Execute(board);

			if (success)
				TryIncrementStage(in stage, board);

			return success;
		}

		private ref readonly TradeRewardProgressionStage GetCurrentStage(Tradeboard board)
		{
			var node = board.Get<ITradingNode>();
			var progressPoint = node.GetCurrentProgress(GetProgressKey(board.Id), autoReset);
			var index = progressPoint >= stages.Value.Length ? ^1 : progressPoint;
			return ref stages.Value.GetValueByIndex(index);
		}

		private void TryIncrementStage(in TradeRewardProgressionStage stage, Tradeboard board)
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
			return _progressKeyCache ??= group ? KEY_FORMAT.Format(group) : GUID_KEY_FORMAT.Format(tradeId, stages.Guid);
		}
	}

	[Serializable]
	public struct TradeRewardProgressionStage
	{
		[SerializeReference]
		public TradeReward reward;

		/// <summary>
		/// Условие при котором на данном этапе прогрессирует
		/// </summary>
		public bool useOverrideCondition;

		/// <summary>
		/// Условие при котором на данном этапе прогрессирует
		/// </summary>
		[SerializeReference]
		[DefaultValue(default(ObjectProviderBlackboardProxyEvaluator))]
		public Condition<Blackboard> overrideCondition;
	}
}
