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

		protected override void OnRelease()
		{
			StaticObjectPoolUtility.ReleaseAndSetNullSafe(ref _restoreSources);
			BlackboardToken.ReleaseAndSetNull(ref _registerRestoreToken);
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

#if CLIENT
		private bool _fetchMode;

		/// <summary>
		/// Режим при котором мы получаем квитанции (чеки)
		/// </summary>
		public bool FetchMode => _fetchMode;

		/// <inheritdoc cref="FetchMode"/>
		public void SetFetchMode(bool value) => _fetchMode = value;

		/// <inheritdoc cref="FetchMode"/>
		public FetchModeScope FetchModeScope(bool value = true) => new(this, value);
#else
		public bool FetchMode => false;
#endif

		private bool _fakeMode;
		public bool FakeMode => _fakeMode;

		private Action _callback;

		public event Action<bool> fakeModeChanged;

		public void SetFakeMode(bool value)
		{
			_fakeMode = value;
			fakeModeChanged?.Invoke(value);
		}

		public FakeModeScope FakeModeScope(bool value)
		{
			return new(this, value);
		}
	}

#if CLIENT
	public readonly struct FetchModeScope : IDisposable
	{
		private readonly Tradeboard _tradeboard;

		public FetchModeScope(Tradeboard tradeboard, bool value = true)
		{
			_tradeboard = tradeboard;
			_tradeboard.SetFetchMode(value);
		}

		public void Dispose()
		{
			_tradeboard.SetFetchMode(false);
		}
	}
#endif

	public readonly struct FakeModeScope : IDisposable
	{
		private readonly Tradeboard _tradeboard;

		public FakeModeScope(Tradeboard tradeboard, bool value = true)
		{
			_tradeboard = tradeboard;
			_tradeboard.SetFakeMode(value);
		}

		public void Dispose() => _tradeboard.SetFakeMode(false);
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
