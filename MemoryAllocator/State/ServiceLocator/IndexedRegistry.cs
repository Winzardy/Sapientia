using System.Runtime.CompilerServices;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	/// <summary>
	/// Generic-хранилище payload'ов проиндексированное по <see cref="TypeIdOf{TBase, T}"/>.
	/// Размер массива = <see cref="TypeId{TBase}.Count"/>, payload type — параметр <typeparamref name="TPayload"/>.
	/// Не знает о конкретном <typeparamref name="TBase"/>-маркере — переиспользуется для ServiceRegistry, ComponentsManager и др.
	/// In-state хранилище (через <see cref="MemArray{T}"/> в allocator).
	/// </summary>
	public struct IndexedRegistry<TBase, TPayload>
		where TBase : IIndexedType
		where TPayload : unmanaged
	{
		public MemArray<TPayload> _payloads;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedRegistry(WorldState worldState)
		{
			_payloads = new MemArray<TPayload>(worldState, TypeId<TBase>.Count);
		}

		public readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _payloads.IsCreated;
		}

		public readonly int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _payloads.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref TPayload Get<T>(WorldState worldState) where T : unmanaged, TBase
		{
			return ref _payloads[worldState, TypeIdOf<TBase, T>.typeId];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref TPayload Get(WorldState worldState, TypeId<TBase> contextTypeId)
		{
			return ref _payloads[worldState, contextTypeId];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set<T>(WorldState worldState, in TPayload payload) where T : unmanaged, TBase
		{
			if (!_payloads.IsCreated)
				throw new System.InvalidOperationException("IndexedRegistry не инициализирован — забыли InitializeInStateRegistries после SetupNewWorldId?");
			_payloads[worldState, TypeIdOf<TBase, T>.typeId] = payload;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(WorldState worldState, TypeId<TBase> contextTypeId, in TPayload payload)
		{
			if (!_payloads.IsCreated)
				throw new System.InvalidOperationException("IndexedRegistry не инициализирован — забыли InitializeInStateRegistries после SetupNewWorldId?");
			_payloads[worldState, contextTypeId] = payload;
		}
	}
}
