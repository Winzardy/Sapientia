using System;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Memory;

namespace Sapientia.Memory
{
	/// <summary>
	/// Аллокатор памяти, который только выделяет память, но не освобождает её.
	/// Он фиксированного размера и никогда не расширяется.
	/// </summary>
	public struct ArenaAllocator : IDisposable
	{
		private int _reservedSize;
		private SafePtr _memory;
		private PtrOffset _rover;

		public SafePtr<ArenaAllocator> AllocatorPtr => _memory;

		public static SafePtr<ArenaAllocator> Create(int reservedSize)
		{
			var result = new ArenaAllocator();

			E.ASSERT(reservedSize > 0);
			result._reservedSize = reservedSize + TSize<ArenaAllocator>.size;
			result._memory = MemoryExt.MemAlloc(reservedSize, ClearOptions.ClearMemory);
			result._rover = default;
			result.MemAlloc<ArenaAllocator>(out _) = result;

			return result.AllocatorPtr;
		}

		public PtrOffset MemAlloc(int size)
		{
			E.ASSERT(size > 0);

			var result = _rover;
			_rover += size;

			return result;
		}

		public PtrOffset MemAlloc(int size, int count)
		{
			return MemAlloc(size * count);
		}

		public PtrOffset<T> MemAlloc<T>()
			where T : unmanaged
		{
			var size = TSize<T>.size;
			return MemAlloc(size);
		}

		public ref T MemAlloc<T>(out PtrOffset<T> offset)
			where T : unmanaged
		{
			var size = TSize<T>.size;
			offset = MemAlloc(size);
			return ref GetRef(offset);
		}

		public PtrOffset<T> MemAlloc<T>(int count)
			where T : unmanaged
		{
			var size = TSize<T>.size;
			return MemAlloc(size * count);
		}

		public SafePtr GetPtr(PtrOffset offset)
		{
			return _memory + offset;
		}

		public SafePtr<T> GetPtr<T>(PtrOffset<T> offset)
			where T : unmanaged
		{
			return _memory + offset;
		}

		public ref T GetRef<T>(PtrOffset<T> offset)
			where T : unmanaged
		{
			return ref (_memory + offset).Value();
		}

		public ref T GetRef<T>(PtrOffset offset)
			where T : unmanaged
		{
			return ref (_memory + offset).Value<T>();
		}

		public void CreateRelativeOffset(ref PtrOffset<ArenaAllocator> relativeOffset)
		{
			relativeOffset = (AllocatorPtr - (SafePtr)relativeOffset.AsSafePtr());
		}

		public void Serialize(ref StreamBufferWriter stream)
		{
			stream.Write(_reservedSize);
			stream.Write(_memory, _reservedSize);
			// Мы знаем, что состояние аллокатора сохранено в начале памяти, поэтому ничего больше сериализовать не нужно
		}

		public static SafePtr<ArenaAllocator> Deserialize(ref StreamBufferReader stream)
		{
			var allocator = new ArenaAllocator();
			allocator._reservedSize = stream.Read<int>();

			stream.Read(ref allocator._memory, allocator._reservedSize);
			allocator.AllocatorPtr.Value()._memory = allocator._memory;

			// Мы знаем, что состояние аллокатора сохранено в начале памяти, поэтому ничего больше десериализовать не нужно
			return allocator.AllocatorPtr;
		}

		public void Reset()
		{
			var allocatorSize = TSize<ArenaAllocator>.size;
			_rover = new PtrOffset(allocatorSize);

			var sizeToReset = _reservedSize - allocatorSize;
			MemoryExt.MemClear(_memory + _rover, sizeToReset);
		}

		public void Dispose()
		{
			MemoryExt.MemFree(_memory);
		}
	}

	/// <summary>
	/// Предполагаем, что аргумент <see cref="offset"/> находится внутри аллокатора.
	/// </summary>
	public static class MonotonicAllocatorExt
	{
		public static ref ArenaAllocator GetRelativeAllocator(this ref PtrOffset<ArenaAllocator> relativeOffset)
		{
			return ref ((SafePtr)relativeOffset.AsSafePtr() + relativeOffset).Value();
		}

		public static PtrOffset MemAlloc(this ref PtrOffset<ArenaAllocator> relativeOffset, int size)
		{
			return relativeOffset.GetRelativeAllocator().MemAlloc(size);
		}

		public static PtrOffset MemAlloc(this ref PtrOffset<ArenaAllocator> relativeOffset, int size, int count)
		{
			return relativeOffset.GetRelativeAllocator().MemAlloc(size, count);
		}

		public static PtrOffset<T> MemAlloc<T>(this ref PtrOffset<ArenaAllocator> relativeOffset)
			where T : unmanaged
		{
			return relativeOffset.GetRelativeAllocator().MemAlloc<T>();
		}

		public static ref T MemAlloc<T>(this ref PtrOffset<ArenaAllocator> relativeOffset, out PtrOffset<T> offset)
			where T : unmanaged
		{
			return ref relativeOffset.GetRelativeAllocator().MemAlloc(out offset);
		}

		public static PtrOffset<T> MemAlloc<T>(this ref PtrOffset<ArenaAllocator> relativeOffset, int count)
			where T : unmanaged
		{
			return relativeOffset.GetRelativeAllocator().MemAlloc<T>(count);
		}

		public static SafePtr GetPtr(this ref PtrOffset<ArenaAllocator> relativeOffset, PtrOffset offset)
		{
			return relativeOffset.GetRelativeAllocator().GetPtr(offset);
		}

		public static SafePtr<T> GetPtr<T>(this ref PtrOffset<ArenaAllocator> relativeOffset, PtrOffset<T> offset)
			where T : unmanaged
		{
			return relativeOffset.GetRelativeAllocator().GetPtr<T>(offset);
		}

		public static ref T GetRef<T>(this ref PtrOffset<ArenaAllocator> relativeOffset, PtrOffset<T> offset)
			where T : unmanaged
		{
			return ref relativeOffset.GetRelativeAllocator().GetRef<T>(offset);
		}

		public static ref T GetRef<T>(this ref PtrOffset<ArenaAllocator> relativeOffset, PtrOffset offset)
			where T : unmanaged
		{
			return ref relativeOffset.GetRelativeAllocator().GetRef<T>(offset);
		}
	}
}
