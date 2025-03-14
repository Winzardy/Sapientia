using System;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Core;
using Sapientia.ServiceManagement;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public static unsafe class AllocatorManager
	{
		private static Allocator* _currentAllocator = null;
		private static Allocator** _allocators = null;
		private static ushort _count = 0;
		private static ushort _capacity = 0;
		private static ushort _currentId = 0;

		public static Allocator* CurrentAllocatorPtr
		{
			[INLINE(256)]
			get => _currentAllocator;
		}

		public static ref Allocator CurrentAllocator
		{
			[INLINE(256)]
			get => ref *_currentAllocator;
		}

		public static AllocatorId CurrentAllocatorId
		{
			[INLINE(256)]
			get => _currentAllocator->allocatorId;
		}

		[INLINE(256)]
		public static AllocatorScope GetAllocatorScope(this ref AllocatorId allocatorId, out Allocator* allocator)
		{
			var scope = new AllocatorScope(_currentAllocator);
			allocatorId.SetCurrentAllocator();
			allocator = _currentAllocator;
			return scope;
		}

		[INLINE(256)]
		public static void SetCurrentAllocator(this ref AllocatorId allocatorId)
		{
			SetCurrentAllocator(allocatorId.GetAllocatorPtr());
		}

		[INLINE(256)]
		public static void SetCurrentAllocator(Allocator* allocator)
		{
			_currentAllocator = allocator;

			var context = allocator == null ? default : allocator->allocatorId;
			ServiceContext<AllocatorId>.SetContext(context);
		}

		public static Allocator* DeserializeAllocator(ref StreamBufferReader stream)
		{
			var allocator = Allocator.Deserialize(ref stream);

			ref var allocatorId = ref allocator->allocatorId;

			// На момент создания не должно существовать аллокатора с таким же Id
			if (allocatorId.IsValid())
				throw new System.Exception("Allocator with such Id already exists.");

			Prewarm(_count);

			allocatorId.index = _count++;

			_allocators[allocatorId.index] = allocator;
			_currentId = _currentId.Max(allocatorId.id + 1);

			return allocator;
		}

		public static Allocator* CreateAllocator(int initialSize = -1)
		{
			Prewarm(_count);

			var allocatorId = new AllocatorId(_count++, ++_currentId);

			var allocator = MemoryExt.MemAllocAndClear<Allocator>();
			_allocators[allocatorId.index] = allocator;

			allocator->Initialize(allocatorId, initialSize);
			return allocator;
		}

		private static void Prewarm(int index)
		{
			if (index + 1 >= _capacity)
			{
				if (_allocators != null)
					MemoryExt.MemFree(_allocators);
				var newCapacity = _capacity > 0 ? (ushort)(_capacity * 2) : (ushort)8;
				_allocators = (Allocator**)MemoryExt.MemAlloc(sizeof(Allocator*) * newCapacity);

				_capacity = newCapacity;
			}
		}

		public static void RemoveAllocator(this ref AllocatorId allocatorId)
		{
			if (!allocatorId.IsValid())
				throw new ArgumentException($"{nameof(AllocatorId)} with such Id [id: {allocatorId.id}] doesn't exist.");

			_allocators[allocatorId.index]->Dispose();
			if (_currentAllocator == _allocators[allocatorId.index])
				SetCurrentAllocator(null);

			_allocators[allocatorId.index] = _allocators[_count - 1];
			_count--;
		}

		[INLINE(256)]
		public static ref Allocator GetAllocator(AllocatorId allocatorId)
		{
			if (!allocatorId.IsValid())
				throw new ArgumentException($"{nameof(AllocatorId)} with such Id [id: {allocatorId.id}] doesn't exist.");
			return ref *_allocators[allocatorId.index];
		}

		[INLINE(256)]
		public static ref Allocator GetAllocator(this ref AllocatorId allocatorId)
		{
			if (!allocatorId.IsValid())
				throw new ArgumentException($"{nameof(AllocatorId)} with such Id [id: {allocatorId.id}] doesn't exist.");
			return ref *_allocators[allocatorId.index];
		}

		[INLINE(256)]
		public static Allocator* GetAllocatorPtr(this ref AllocatorId allocatorId)
		{
			if (!allocatorId.IsValid())
				throw new ArgumentException($"{nameof(AllocatorId)} with such Id [id: {allocatorId.id}] doesn't exist.");
			return _allocators[allocatorId.index];
		}

		[INLINE(256)]
		public static bool IsValid(this ref AllocatorId allocatorId)
		{
			if (allocatorId.index < _count && _allocators[allocatorId.index]->allocatorId.id == allocatorId.id)
				return true;

			for (ushort i = 0; i < _count; i++)
			{
				if (_allocators[i]->allocatorId.id != allocatorId.id)
					continue;
				allocatorId.index = i;
				return true;
			}
			return false;
		}

		public readonly ref struct AllocatorScope
		{
			private readonly Allocator* _previousAllocator;

			public AllocatorScope(Allocator* previousAllocator)
			{
				_previousAllocator = previousAllocator;
			}

			public void Dispose()
			{
				SetCurrentAllocator(_previousAllocator);
			}
		}
	}
}
