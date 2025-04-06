using System;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Core;
using Sapientia.ServiceManagement;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public static unsafe class AllocatorManager
	{
		private static SafePtr<Allocator> _currentAllocator = default;

		private static SafePtr<SafePtr<Allocator>> _allocators = default;
		private static int _count = 0;
		private static int _capacity = 0;
		private static int _currentId = 0;

		public static SafePtr<Allocator> CurrentAllocatorPtr
		{
			[INLINE(256)]
			get => _currentAllocator;
		}

		public static ref Allocator CurrentAllocator
		{
			[INLINE(256)]
			get => ref _currentAllocator.Value();
		}

		public static AllocatorId CurrentAllocatorId
		{
			[INLINE(256)]
			get => _currentAllocator.Value().allocatorId;
		}

		[INLINE(256)]
		public static AllocatorScope GetAllocatorScope(this ref AllocatorId allocatorId, out SafePtr<Allocator> allocator)
		{
			var scope = new AllocatorScope(_currentAllocator == default ? default : CurrentAllocatorId);
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
		public static void SetCurrentAllocator(AllocatorId allocatorId)
		{
			if (!allocatorId.IsValid())
				return;
			SetCurrentAllocator(allocatorId.GetAllocatorPtr());
		}

		[INLINE(256)]
		public static void SetCurrentAllocator(SafePtr<Allocator> allocator)
		{
			_currentAllocator = allocator;

			var context = allocator == default ? default : allocator.Value().allocatorId;
			ServiceContext<AllocatorId>.SetContext(context);
		}

		public static SafePtr<Allocator> DeserializeAllocator(ref StreamBufferReader stream)
		{
			var allocator = Allocator.Deserialize(ref stream);

			ref var allocatorId = ref allocator.Value().allocatorId;

			// На момент создания не должно существовать аллокатора с таким же Id
			if (allocatorId.IsValid())
				throw new System.Exception("Allocator with such Id already exists.");

			Prewarm(_count);

			allocatorId.index = (ushort)_count++;

			_allocators[allocatorId.index] = allocator;
			_currentId = _currentId.Max(allocatorId.id + 1);

			return allocator;
		}

		public static SafePtr<Allocator> CreateAllocator(int initialSize = -1)
		{
			Prewarm(_count);

			var allocatorId = new AllocatorId((ushort)_count++, (ushort)++_currentId);
			ref var allocator = ref _allocators[allocatorId.index];

			if (allocator.IsValid)
			{
				allocator.Value().Reset(allocatorId);
			}
			else
			{
				allocator = MemoryExt.MemAllocAndClear<Allocator>();
				allocator.Value().Initialize(allocatorId, initialSize);
			}

			return allocator;
		}

		private static void Prewarm(int index)
		{
			MemoryExt.ResizeArray(ref _allocators, ref _capacity, index + 1, true, true);
		}

		public static void RemoveAllocator(this ref AllocatorId allocatorId)
		{
			if (!allocatorId.IsValid())
				throw new ArgumentException($"{nameof(AllocatorId)} with such Id [id: {allocatorId.id}] doesn't exist.");

			if (_currentAllocator == _allocators[allocatorId.index])
				SetCurrentAllocator(default(SafePtr<Allocator>));

			if (_count > 1)
			{
				ref var a = ref _allocators[allocatorId.index];
				ref var b = ref _allocators[_count - 1];

				(a, b) = (b, a);
			}
			_count--;
		}

		[INLINE(256)]
		public static ref Allocator GetAllocator(AllocatorId allocatorId)
		{
			if (!allocatorId.IsValid())
				throw new ArgumentException($"{nameof(AllocatorId)} with such Id [id: {allocatorId.id}] doesn't exist.");
			return ref _allocators[allocatorId.index].Value();
		}

		[INLINE(256)]
		public static ref Allocator GetAllocator(this ref AllocatorId allocatorId)
		{
			if (!allocatorId.IsValid())
				throw new ArgumentException($"{nameof(AllocatorId)} with such Id [id: {allocatorId.id}] doesn't exist.");
			return ref _allocators[allocatorId.index].Value();
		}

		[INLINE(256)]
		public static SafePtr<Allocator> GetAllocatorPtr(this ref AllocatorId allocatorId)
		{
			if (!allocatorId.IsValid())
				throw new ArgumentException($"{nameof(AllocatorId)} with such Id [id: {allocatorId.id}] doesn't exist.");
			return _allocators[allocatorId.index];
		}

		[INLINE(256)]
		public static bool IsValid(this ref AllocatorId allocatorId)
		{
			if (allocatorId.index < _count && _allocators[allocatorId.index].Value().allocatorId.id == allocatorId.id)
				return true;

			for (ushort i = 0; i < _count; i++)
			{
				if (_allocators[i].Value().allocatorId.id != allocatorId.id)
					continue;
				allocatorId.index = i;
				return true;
			}
			return false;
		}

		public readonly ref struct AllocatorScope
		{
			private readonly AllocatorId _previousAllocator;

			public AllocatorScope(AllocatorId previousAllocator)
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
