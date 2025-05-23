using System.Threading;
using System.Threading.Tasks;

namespace Trading
{
	public abstract partial class TradeCost
	{
		/// <returns>Можно ли вернуть, что заплатили?</returns>
		public virtual bool CanReturn(out TradeCostReturnError? error)
		{
			error = null;
			return false;
		}

		/// <summary>
		/// Возвращаем, что заплатили
		/// </summary>
		/// <returns>Получилось ли?</returns>
		internal virtual Task<bool> ReturnAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
	}

	public readonly struct TradeCostReturnError
	{
		public readonly string category;
		public readonly int code;
		public readonly object rawData;

		public TradeCostReturnError(string category, int code, object rawData = null)
		{
			this.category = category;
			this.code = code;
			this.rawData = rawData;
		}

		public TradeCostReturnError(string category, object rawData = null) : this(category, 0, rawData)
		{
		}
	}
}
