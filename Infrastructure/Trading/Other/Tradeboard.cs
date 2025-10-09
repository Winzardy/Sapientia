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
		private bool _fetchMode;
		private int _fetchRequest;

		/// <summary>
		/// Режим при котором мы получаем квитанции (чеки), так же автоматически включается Dummy (фейк) режим
		/// </summary>
		public bool IsFetchMode => _fetchMode;

		/// <inheritdoc cref="IsFetchMode"/>
		public void SetFetchMode(bool value)
		{
			if (value)
				_fetchRequest++;
			else
				_fetchRequest--;

			_fetchMode = _fetchRequest != 0;
		}

		/// <inheritdoc cref="IsFetchMode"/>
		public FetchModeScope FetchModeScope(bool value = true) => new(this, value);

#endif

		#endregion

		#region Dummy

		private bool _dummyMode;
		private int _dummyRequest;

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
			var l = _dummyMode;

			if (value)
				_dummyRequest++;
			else
				_dummyRequest--;

			_dummyMode = _dummyRequest != 0;
			if (l != _dummyMode)
				dummyModeChanged?.Invoke(value);
		}

		public DummyModeScope DummyModeScope(bool value = true) => new(this, value);

		#endregion

		protected override void OnRelease()
		{
			StaticObjectPoolUtility.ReleaseAndSetNullSafe(ref _restoreSources);
			BlackboardToken.ReleaseAndSetNull(ref _registerRestoreToken);

			_dummyMode = false;
			_dummyRequest = 0;

			_id = null;
			_restoreSources?.Clear();

#if CLIENT
			_fetchMode = false;
			_fetchRequest = 0;
#endif
		}
	}

#if CLIENT
	public readonly struct FetchModeScope : IDisposable
	{
		private readonly Tradeboard _tradeboard;
		private readonly bool _value;

		public FetchModeScope(Tradeboard tradeboard, bool value = true)
		{
			_value = value;
			_tradeboard = tradeboard;

			if (!value)
				return;

			_tradeboard.SetFetchMode(value);
			_tradeboard.SetDummyMode(value);
		}

		public void Dispose()
		{
			if (!_value)
				return;

			_tradeboard.SetFetchMode(false);
			_tradeboard.SetDummyMode(false);
		}
	}
#endif

	public readonly struct DummyModeScope : IDisposable
	{
		private readonly Tradeboard _tradeboard;
		private readonly bool _value;

		public DummyModeScope(Tradeboard tradeboard, bool value = true)
		{
			_value = value;
			_tradeboard = tradeboard;

			if (!value)
				_tradeboard.SetDummyMode(value);
		}

		public void Dispose()
		{
			if (!_value)
				return;

			_tradeboard.SetDummyMode(false);
		}
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
