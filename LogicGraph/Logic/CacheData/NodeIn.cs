namespace Sapientia.LogicGraph
{
	/// <summary>Типизированный вход ноды: читает значение из ячейки кеша своего источника через <see cref="InstanceCache"/>.</summary>
	public struct NodeIn<T> where T : unmanaged
	{
		public CacheHandler<T> input;

		/// <summary>Читает входное значение (следуя link'ам). <c>true</c> — посчитано.</summary>
		public bool Read(ref InstanceCache cache, out T value)
		{
			return cache.Read(input, out value);
		}

		/// <summary>Посчитан ли источник входа (гейт мемоизации, M8).</summary>
		public bool IsCalculated(ref InstanceCache cache)
		{
			return cache.IsCalculated(input);
		}
	}
}
