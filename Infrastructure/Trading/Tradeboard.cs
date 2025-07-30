using System;
using Content;
using Sapientia;

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
		/// <summary>
		/// Trade Id
		/// </summary>
		public string Id { get; private set; }

		internal void SetId(string id) => Id = id;

		public Tradeboard()
		{
		}

		public Tradeboard(Blackboard source) : base(source)
		{
		}

		protected override Exception GetArgumentException(object msg) => TradingDebug.logger?.Exception(msg) ??
			base.GetArgumentException(msg);
	}

	public static class TradeboardUtility
	{
		public static void Bind(this Tradeboard board, in ContentReference<TradeCost> reference)
		{
			board.SetId(reference.guid);
		}

		public static void Bind(this Tradeboard board, in TradeEntry entry)
		{
			board.SetId(entry.Id);
		}
	}

	public interface IDateTimeProvider
	{
		public DateTime Now { get; }
	}
}
