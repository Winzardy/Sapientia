using System;

namespace Trading
{
	[Serializable]
	public abstract partial class TradeReward
	{
		/// <summary>
		/// Участвует в сортировке в <see cref="TradeRewardCollection"/>
		/// </summary>
		protected internal virtual int Priority => TradeRewardPriority.NORMAL;

		internal bool CanExecute(Tradeboard board, out TradeReceiveError? error) => CanReceive(board, out error);
		internal bool Execute(Tradeboard board) => Receive(board);

		/// <summary>
		/// Доступно ли для получения? пример: нет места в инвентаре
		/// </summary>
		protected virtual bool CanReceive(Tradeboard board, out TradeReceiveError? error)
		{
			error = null;
			return true;
		}

		/// <summary>
		/// Выдать
		/// </summary>
		protected abstract bool Receive(Tradeboard board);
	}

	public readonly struct TradeReceiveError
	{
		public readonly string category;
		public readonly int code;
		public readonly object rawData;

		public TradeReceiveError(string category, int code, object rawData = null)
		{
			this.category = category;
			this.code = code;
			this.rawData = rawData;
		}

		public TradeReceiveError(string category, object rawData = null) : this(category, 0, rawData)
		{
		}

		public static TradeReceiveError NotImplemented = new("NotImplemented");
		public static TradeReceiveError? NotError => null;

		public override string ToString() => $"TradeReceiveError: category: {category}, code: {code}, rawData: {rawData}";
	}

	public static class TradeRewardPriority
	{
		public const int VERY_HIGH = 1000;
		public const int HIGH = 100;
		public const int NORMAL = 0;
		public const int LOW = -100;
		public const int VERY_LOW = -1000;
	}
}
