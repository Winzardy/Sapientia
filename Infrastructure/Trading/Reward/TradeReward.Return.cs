using System.Threading;
using System.Threading.Tasks;

namespace Trading
{
	public abstract partial class TradeReward
	{
		/// <inheritdoc cref="CanReturn"/>
		internal virtual bool CanExecuteReturn(Tradeboard board, out TradeRewardReturnError? error)
			=> CanReturn(board, out error);

		/// <inheritdoc cref="ReceiveAsync"/>
		internal virtual bool ExecuteReturn(Tradeboard board)
			=> Return(board);

		/// <summary>
		/// Можно ли забрать обратно что выдали?
		/// </summary>
		protected virtual bool CanReturn(Tradeboard board, out TradeRewardReturnError? error)
		{
			error = null;
			return false;
		}

		/// <summary>
		/// Забираем, то что выдали
		/// </summary>
		/// <returns>Успешность</returns>
		protected virtual bool Return(Tradeboard board) => true;
	}

	public readonly struct TradeRewardReturnError
	{
		public readonly string category;
		public readonly int code;
		public readonly object rawData;

		public TradeRewardReturnError(string category, int code, object rawData = null)
		{
			this.category = category;
			this.code = code;
			this.rawData = rawData;
		}

		public TradeRewardReturnError(string category, object rawData = null) : this(category, 0, rawData)
		{
		}
	}
}
