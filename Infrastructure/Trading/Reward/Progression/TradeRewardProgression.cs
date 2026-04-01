using System;
using Content;
using Sapientia;
using Sapientia.Collections;
using Sapientia.Conditions;
using Sapientia.Extensions;
using UnityEngine;
#if CLIENT
using UnityEngine.Serialization;
#endif

namespace Trading
{
	[Serializable]
	public partial class TradeRewardProgression : TradeReward, ITradeFinishHandler
	{
		[NonSerialized]
		private string _progressKeyCache;

		public ContentEntry<TradeRewardProgressionStage[]> stages;

		/// <summary>
		/// Награда выбирается циклически по значению прогресса.
		/// Например, если прогресс = 5, а наград всего 4,
		/// будет выбрана награда с индексом 1 (5 % 4).
		/// <br/>
		/// В отличие от обычного поведения, где при превышении диапазона
		/// выбирается последняя награда
		/// </summary>
		public bool cycle;

		public TradeProgressionSchemeSource schemeSource;

#if CLIENT
		[FormerlySerializedAs("autoReset")]
#endif
		public TradeProgressionScheme scheme;

		public ContentReference<TradeProgressionScheme> schemeReference;

		public void Reset(Tradeboard board)
		{
			var node = board.Get<ITradingNode>();
			node.ResetProgress(GetProgressKey(board.Id), in scheme);
		}

		protected override bool CanReceive(Tradeboard board, out TradeReceiveError? error)
		{
			ref readonly var stage = ref GetCurrentStage(board);
			return stage.reward.CanExecute(board, out error);
		}

		protected override bool Receive(Tradeboard board)
		{
			ref readonly var stage = ref GetCurrentStage(board);
			var success = stage.reward.Execute(board);

			if (success)
				TryIncrementStageAfterReceive(in stage, board);

			return success;
		}

		public ref readonly TradeRewardProgressionStage GetCurrentStage(Tradeboard board)
		{
			var node = board.Get<ITradingNode>();
			return ref GetCurrentStage(node, board.Id);
		}

		public ref readonly TradeRewardProgressionStage GetCurrentStage(ITradingNode node, string tradeId)
		{
			var index = GetCurrentStageIndex(node, tradeId);
			return ref stages.Value.GetValueByIndex(index);
		}

		public Index GetCurrentStageIndex(Tradeboard board)
		{
			var node = board.Get<ITradingNode>();
			return GetCurrentStageIndex(node, board.Id);
		}

		public Index GetCurrentStageIndex(ITradingNode node, string tradeId)
		{
			var key = GetProgressKey(tradeId);
			var progress = node.GetCurrentProgress(key, scheme);
			var length = stages.Value.Length;
			return progress >= length ? cycle ? progress % length : ^1 : progress;
		}

		private void TryIncrementStageAfterReceive(in TradeRewardProgressionStage stage, Tradeboard board)
		{
			// Если награда под общим прогрессом ее инкремент отдельный в конце сделки
			if (schemeSource != TradeProgressionSchemeSource.Local)
				return;

			TryIncrementStage(scheme, in stage, board);
		}

		public void OnTradeFinished(Tradeboard board)
		{
			if (schemeSource != TradeProgressionSchemeSource.Shared)
				return;

			var progressKey = GetProgressKey(board.Id);

			// Если прогресс уже был, игнорируем остальные
			if (board.Contains<bool>(progressKey))
				return;

			TryIncrementStage(progressKey, schemeReference, board);
			board.Register(true, progressKey);
		}

		private void TryIncrementStage(TradeProgressionScheme targetScheme, in TradeRewardProgressionStage stage, Tradeboard board)
		{
			var progressKey = GetProgressKey(board.Id);
			var skipCondition = stage.useOverrideCondition;
			if (!stage.overrideCondition.IsFulfilled(board))
				return;

			TryIncrementStage(progressKey, targetScheme, board, skipCondition);
		}

		private void TryIncrementStage(string key, TradeProgressionScheme targetScheme, Tradeboard board)
		{
			TryIncrementStage(key, targetScheme, board, false);
		}

		private void TryIncrementStage(string key, TradeProgressionScheme targetScheme, Tradeboard board, bool skipCondition)
		{
			if (!skipCondition && !targetScheme.condition.IsFulfilled(board))
			{
				return;
			}

			var node = board.Get<ITradingNode>();
			node.IncrementProgress(key, targetScheme);
		}

		private string GetProgressKey(string tradeId)
		{
			return _progressKeyCache ??= schemeSource == TradeProgressionSchemeSource.Shared
				? TradeProgressionScheme.BOARD_PROGRESS_KEY_FORMAT.Format(schemeReference.guid)
				: TradeProgressionScheme.PROGRESS_GUID_KEY_FORMAT.Format(tradeId, stages.Guid);
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
		public Condition<Blackboard> overrideCondition;
	}
}
