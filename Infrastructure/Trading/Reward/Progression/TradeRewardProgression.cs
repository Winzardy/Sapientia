using System;
using Content;
using Sapientia;
using Sapientia.Collections;
using Sapientia.Conditions;
using UnityEngine;
#if CLIENT
using UnityEngine.Serialization;
#endif

namespace Trading
{
	[Serializable]
	public partial class TradeRewardProgression : TradeReward, ITradeFinishHandler, ITradeResettable
#if CLIENT
		, ISerializationCallbackReceiver
#endif
	{
		public TradeRewardProgressionStage[] stages;

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
		public ContentEntry<TradeProgressionScheme> schemeEntry;
		public SerializableGuid Key { get => schemeEntry.Guid; }
		public ContentReference<TradeProgressionScheme> schemeReference;

		public void Reset(Tradeboard board)
		{
			var node = board.Get<ITradingNode>();
			var progressKey = GetProgressKey(out var scheme);
			node.ResetProgress(progressKey, scheme);
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
			return ref GetCurrentStage(node);
		}

		public ref readonly TradeRewardProgressionStage GetCurrentStage(ITradingNode node)
		{
			var index = GetCurrentStageIndex(node);
			return ref stages.GetValueByIndex(index);
		}

		public Index GetCurrentStageIndex(Tradeboard board)
		{
			var node = board.Get<ITradingNode>();
			return GetCurrentStageIndex(node);
		}

		public Index GetCurrentStageIndex(ITradingNode node)
		{
			var key = GetProgressKey(out var scheme);
			var progress = node.GetCurrentProgress(key, scheme);
			var length = stages.Length;
			return progress >= length ? cycle ? progress % length : ^1 : progress;
		}

		private void TryIncrementStageAfterReceive(in TradeRewardProgressionStage stage, Tradeboard board)
		{
			// Если награда под общим прогрессом ее инкремент отдельный в конце сделки
			if (schemeSource != TradeProgressionSchemeSource.Local)
				return;

			TryIncrementStage(in stage, board);
		}

		public void OnTradeFinished(Tradeboard board)
		{
			if (schemeSource != TradeProgressionSchemeSource.Shared)
				return;

			var progressKey = GetProgressKey(out var scheme);

			// Если прогресс уже был, игнорируем остальные
			if (board.Contains<bool>(progressKey))
				return;

			TryIncrementStage(progressKey, scheme, board);
			board.Register(true, progressKey);
		}

		private void TryIncrementStage(in TradeRewardProgressionStage stage, Tradeboard board)
		{
			var progressKey = GetProgressKey(out var scheme);
			var skipCondition = stage.useOverrideCondition;
			if (!stage.overrideCondition.IsFulfilled(board))
				return;

			TryIncrementStage(progressKey, scheme, board, skipCondition);
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

		private string GetProgressKey(out TradeProgressionScheme scheme)
		{
			if (schemeSource == TradeProgressionSchemeSource.Shared)
			{
				scheme = schemeReference.Read();
				return schemeReference.guid;
			}
			else
			{
				scheme = schemeEntry;
				return schemeEntry.Guid;
			}
		}

#if CLIENT
		[FormerlySerializedAs("autoReset")]
		[HideInInspector]
		public TradeProgressionScheme scheme;

		[FormerlySerializedAs("stages")]
		[HideInInspector]
		public ContentEntry<TradeRewardProgressionStage[]> legacyStages;

		[HideInInspector]
		public bool migrated;

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (migrated)
				return;
			if (!legacyStages.IsValid() && scheme == null)
				return;
			stages = legacyStages.Value;
			schemeEntry.SetValue(in scheme);
			if (legacyStages.IsValid())
			{
				schemeEntry.SetGuid(legacyStages.Guid);
				legacyStages.RegenerateGuid();
			}

			migrated = true;
		}
#endif
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
