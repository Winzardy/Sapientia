using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	/// <summary>
	/// Пара <see cref="ProxyPtr{T}"/> + <see cref="TypeId{IWorldService}"/> для регистрации
	/// state-парта / системы в <see cref="UnsafeIndexedRegistry{IWorldService, IndexedPtr}"/>.
	/// Используется в <see cref="WorldBuilder"/> при сборке мира и в <see cref="World.Initialize"/>.
	/// Имя — по фидбеку лида: "struct WorldService&lt;T&gt; для списка сейт партов и систем". Не путать с маркером <see cref="IWorldService"/>: тот — interface (I-префикс), этот — payload для регистрации.
	/// </summary>
	public readonly struct WorldService<TProxy> where TProxy : unmanaged, IProxy
	{
		public readonly ProxyPtr<TProxy> proxy;
		public readonly TypeId<IWorldService> typeId;

		public WorldService(ProxyPtr<TProxy> proxy, TypeId<IWorldService> typeId)
		{
			this.proxy = proxy;
			this.typeId = typeId;
		}
	}
}
