using System;
using System.Collections.Generic;
using Content;
using Sapientia;
using Sapientia.Collections;
using Sapientia.Pooling;

namespace Trading
{
	/// <summary>
	/// Контекст сделки, сюда попадают все объекты участвующие в сделке
	/// </summary>
	/// <remarks>
	/// Наследуется от <see cref="Blackboard"/>, поэтому может участвовать во вложенных взаимодействиях,
	/// например для передачи в RewardBox или еще каких системах
	/// </remarks>
	/// <seealso cref="Blackboard"/>
	public sealed partial class Tradeboard : Blackboard
	{
		private const string RESTORE_KEY = "restoring";

		private string _id;

		private HashSet<string> _restoreSources;
		private BlackboardToken? _registerRestoreToken;

		/// <summary>
		/// Идентификатор сделки (tradeId)
		/// </summary>
		public string Id => _id;

		public bool IsRestoreState => _restoreSources.Any();

		public Tradeboard()
		{
		}

		public Tradeboard(Blackboard source) : base(source)
		{
		}

		protected override void OnRelease()
		{
			StaticObjectPoolUtility.ReleaseAndSetNullSafe(ref _restoreSources);
			BlackboardToken.ReleaseAndSetNull(ref _registerRestoreToken);

			_id = null;
			_restoreSources?.Clear();

#if CLIENT
			OnReleaseFetchMode();
#endif
			OnReleaseResultHandle();
		}

		internal void SetId(string id)
		{
			_id = id;
		}

		public void AddRestoreSource(string source)
		{
			_restoreSources ??= HashSetPool<string>.Get();
			if (_restoreSources.Add(source))
			{
				if (!_registerRestoreToken.HasValue)
					_registerRestoreToken = Register(true, RESTORE_KEY);
			}
		}

		public void RemoveRestoreSource(string source)
		{
			_restoreSources?.Remove(source);

			if (_restoreSources != null && _restoreSources.IsEmpty())
				BlackboardToken.ReleaseAndSetNull(ref _registerRestoreToken);
		}

		protected override Exception GetArgumentException(object msg) => TradingDebug.logger?.Exception(msg) ??
			base.GetArgumentException(msg);
	}

	public static class TradeboardUtility
	{
		public static string GetTradeId(this in ContentReference<TradeCost> reference) => reference.guid;
		public static string GetTradeId(this in ContentReference<TradeReward> reference) => reference.guid;
		public static string GetTradeId(this ContentEntry<TradeCost> reference) => reference.Guid;

		public static void Bind(this Tradeboard board, in ContentReference<TradeCost> reference)
		{
			Bind(board, reference.GetTradeId());
		}

		public static void Bind(this Tradeboard board, in ContentReference<TradeReward> reference)
		{
			Bind(board, reference.GetTradeId());
		}

		public static void Bind(this Tradeboard board, in ContentEntry<TradeCost> reference)
		{
			Bind(board, reference.GetTradeId());
		}

		public static void Bind(this Tradeboard board, in TradeConfig config)
		{
			Bind(board, config.Id);
		}

		public static void Bind(this Tradeboard board, string tradeId)
		{
			board.SetId(tradeId);
		}
	}
}
