using System.Collections.Generic;
using Sapientia.Pooling;

namespace Trading
{
	public abstract partial class TradeReward
	{
		public virtual IEnumerator<TradeReward> GetEnumerator()
		{
			yield return this;
		}

		/// <returns>Перебирает внутренние элементы и возвращает массив</returns>
		public TradeReward[] ToArray()
		{
			using var _ = ListPool<TradeReward>.Get(out var list);
			foreach (var reward in this)
				list.Add(reward);
			return list.ToArray();
		}
	}
}
