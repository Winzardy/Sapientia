using System.Threading;
using System.Threading.Tasks;

namespace Trading
{
	public abstract partial class TradeReward
	{
		/// <summary>
		/// Можно ли забрать обратно что выдали?
		/// </summary>
		public virtual bool CanReturn(Tradeboard board, out TradeRewardReturnError? error)
		{
			error = null;
			return false;
		}

		/// <summary>
		/// Забираем, то что выдали
		/// </summary>
		/// <returns>Успешность</returns>
		internal virtual Task<bool> ReturnAsync(Tradeboard board, CancellationToken cancellationToken = default) => Task.FromResult(true);
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
