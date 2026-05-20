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
	public partial class TradeCostProgression : TradeCost, ITradeResettable, ITradeFinishHandler
#if CLIENT
		, ISerializationCallbackReceiver
#endif
	{
		public TradeCostProgressionStage[] stages;

		/// <summary>
		/// Награда выбирается циклически по значению прогресса.
		/// Например, если прогресс = 5, а цен всего 4,
		/// будет выбрана цена с индексом 1 (5 % 4).
		/// <br/>
		/// В отличие от обычного поведения, где при превышении диапазона
		/// выбирается последняя цена
		/// </summary>
		public bool cycle;

		public TradeProgressionSchemeSource schemeSource;
		public ContentEntry<TradeProgressionScheme> schemeEntry;
		public SerializableGuid Key { get => schemeEntry.Guid; }
		public ContentReference<TradeProgressionScheme> schemeReference;

		public void Reset(Tradeboard board)
		{
			var node = board.Get<ITradingNode>();
			node.ResetProgress(GetProgressKey(), schemeEntry);
		}

		public int GetCurrentStageIndex(Tradeboard board)
		{
			var node = board.Get<ITradingNode>();
			var progress = node.GetCurrentProgress(GetProgressKey(), schemeEntry);
			var length = stages.Length;
			return progress >= length
				? cycle ? progress % length : length - 1
				: progress;
		}

		protected override bool CanPay(Tradeboard board, out TradePayError? error)
		{
			ref readonly var stage = ref GetCurrentStage(board);
			return stage.cost.CanExecute(board, out error);
		}

		protected override bool Pay(Tradeboard board)
		{
			ref readonly var stage = ref GetCurrentStage(board);
			var success = stage.cost.Execute(board);

			if (success)
				TryIncrementStageAfterPay(in stage, board);

			return success;
		}

		private ref readonly TradeCostProgressionStage GetCurrentStage(Tradeboard board)
		{
			var node = board.Get<ITradingNode>();
			var progress = node.GetCurrentProgress(GetProgressKey(), schemeEntry);
			var length = stages.Length;
			var index = progress >= length
				? cycle
					? progress % length
					: ^1
				: progress;
			return ref stages.GetValueByIndex(index);
		}

		private void TryIncrementStageAfterPay(in TradeCostProgressionStage stage, Tradeboard board)
		{
			// Если награда под общим прогрессом ее инкремент отдельный в конце сделки
			if (schemeSource != TradeProgressionSchemeSource.Local)
				return;

			TryIncrementStage(schemeEntry, in stage, board);
		}

		public void OnTradeFinished(Tradeboard board)
		{
			if (schemeSource != TradeProgressionSchemeSource.Shared)
				return;

			var progressKey = GetProgressKey();

			// Если прогресс уже был, игнорируем остальные
			if (board.Contains<bool>(progressKey))
				return;

			TryIncrementStage(progressKey, schemeReference, board);
			board.Register(true, progressKey);
		}

		private void TryIncrementStage(TradeProgressionScheme targetScheme, in TradeCostProgressionStage stage, Tradeboard board)
		{
			var progressKey = GetProgressKey();
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

		private string GetProgressKey() => schemeSource == TradeProgressionSchemeSource.Shared
				? schemeReference.guid
				: schemeEntry.Guid;

#if CLIENT
		[FormerlySerializedAs("autoReset")]
		[HideInInspector]
		public TradeProgressionScheme scheme;

		[FormerlySerializedAs("stages")]
		[HideInInspector]
		public ContentEntry<TradeCostProgressionStage[]> legacyStages;

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
