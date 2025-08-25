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

		/// <summary>
		/// Trade Id
		/// </summary>
		public string Id { get; private set; }

		internal void SetId(string id) => Id = id;

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
	}

	public static class TradeboardUtility
	{
		public static void Bind(this Tradeboard board, in ContentReference<TradeCost> reference)
		{
			Bind(board, reference.guid);
		}

		public static void Bind(this Tradeboard board, in TradeEntry entry)
		{
			Bind(board, entry.Id);
		}

		public static void Bind(this Tradeboard board, string tradeId)
		{
			board.SetId(tradeId);
		}
	}

	public interface IDateTimeProvider
	{
		public DateTime Now { get; }
	}
}
