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
	public sealed class Tradeboard : Blackboard
	{
		private const string RESTORE_KEY = "restoring";

		private string _id;

		/// <summary>
		/// Trade Id
		/// </summary>
		public string Id => _id;

		internal void SetId(string id) => _id = id;

		public bool IsRestoreState => _restoreSources.Any();

		private HashSet<string> _restoreSources;

		private BlackboardToken? _registerRestoreToken;

		public Tradeboard()
		{
		}

		public Tradeboard(Blackboard source) : base(source)
		{
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

		#region Fetching

#if CLIENT
		private bool _fetching;

		/// <summary>
		/// Режим при котором мы получаем квитанции (чеки), так же автоматически включается Dummy (фейк) режим
		/// </summary>
		public bool IsFetching => _fetching;

		/// <inheritdoc cref="IsFetching"/>
		public void SetFetching(bool value) => _fetching = value;

		/// <inheritdoc cref="IsFetching"/>
		public FetchingScope FetchingScope(bool value = true) => new(this, value);

#else
		public bool FetchMode => false;
#endif

		#endregion

		#region Dummy

		private bool _dummyMode;

		/// <summary>
		/// Режим симуляции покупки, чтобы вернуть стейт игры обратно (в нашем случае рандом)
		/// </summary>
		/// <remarks>
		/// Назвал Dummy вместо Fake потому что Fake конфликтует с Fetching
		/// </remarks>
		public bool DummyMode => _dummyMode;

		public event Action<bool> dummyModeChanged;

		public void SetDummyMode(bool value)
		{
			_dummyMode = value;
			dummyModeChanged?.Invoke(value);
		}

		public DummyModeScope DummyModeScope(bool value = true) => new(this, value);

		#endregion

		protected override void OnRelease()
		{
			StaticObjectPoolUtility.ReleaseAndSetNullSafe(ref _restoreSources);
			BlackboardToken.ReleaseAndSetNull(ref _registerRestoreToken);

			_dummyMode = false;
			_fetching = false;
			_id = null;
			_restoreSources?.Clear();
		}
	}

#if CLIENT
	public readonly struct FetchingScope : IDisposable
	{
		private readonly Tradeboard _tradeboard;

		public FetchingScope(Tradeboard tradeboard, bool value = true)
		{
			_tradeboard = tradeboard;
			_tradeboard.SetFetching(value);
			_tradeboard.SetDummyMode(value);
		}

		public void Dispose()
		{
			_tradeboard.SetFetching(false);
			_tradeboard.SetDummyMode(false);
		}
	}
#endif

	public readonly struct DummyModeScope : IDisposable
	{
		private readonly Tradeboard _tradeboard;

		public DummyModeScope(Tradeboard tradeboard, bool value = true)
		{
			_tradeboard = tradeboard;
			_tradeboard.SetDummyMode(value);
		}

		public void Dispose() => _tradeboard.SetDummyMode(false);
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
