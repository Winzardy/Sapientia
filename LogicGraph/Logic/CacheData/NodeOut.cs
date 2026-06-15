namespace Sapientia.LogicGraph
{
	/// <summary>Типизированный выход ноды: пишет мемоизированное значение в свою ячейку кеша через <see cref="CacheHeader"/>.</summary>
	public struct NodeOut<T> where T : unmanaged
	{
		public CacheHandler<T> output;

		/// <summary>Записывает результат (state=Value).</summary>
		public void Write(ref CacheHeader cache, in T value)
		{
			cache.Write(output, value);
		}

		/// <summary>Посчитан ли уже этот выход (гейт мемоизации, M8).</summary>
		public bool IsCalculated(ref CacheHeader cache)
		{
			return cache.IsCalculated(output);
		}
	}
}
