using System;
using Sapientia;
using Sapientia.Collections;

namespace Submodules.Sapientia.Memory
{
	public enum TrackingType
	{
		Invalid,
		NoTracking,
		CountTracking,
		DeepTracking,
	}

	public unsafe struct MemoryTracker : IDisposable
	{
		public static readonly MemoryTracker Invalid = new MemoryTracker
		{
			_trackingType = TrackingType.Invalid,
		};

		// Ключ - указатель на начало выделенной памяти, значение - размер выделенной памяти
		private UnsafeDictionary<IntPtr, int> _allocations;
		// Список границ всех выделенных областей памяти (Нижние и верхние границы), отсортирован по возрастанию
		// Всегда кратно 2, при выделении памяти добавляет в список нижний и верхний предел выделенной памяти
		// Используется `long` вместо `IntPtr`, т.к. `IntPtr` не поддерживает сравнение и бинарный поиск (По факту это `IntPtr`)
		private UnsafeList<long> _memorySpaces;
		private int _allocationCount;

		private TrackingType _trackingType;

		public bool IsValid => _trackingType != TrackingType.Invalid;

		public int AllocationCount => _allocationCount;

		public MemoryTracker(TrackingType trackingType, int initialCapacity = 128)
		{
			E.ASSERT(trackingType != TrackingType.Invalid, "Нельзя создать трекер в режиме TrackingType.Invalid");

			this = default;
			if (trackingType == TrackingType.DeepTracking)
			{
				_allocations = new UnsafeDictionary<IntPtr, int>(MemoryManager.InnerMemoryId, initialCapacity);
				_memorySpaces = new UnsafeList<long>(MemoryManager.InnerMemoryId, initialCapacity);
			}
			_trackingType = trackingType;
		}

		public void StartDisposeTrackingType()
		{
			if (_trackingType == TrackingType.DeepTracking)
				_trackingType = TrackingType.CountTracking;
		}

		public void Track(void* ptr, int size)
		{
			if (_trackingType == TrackingType.DeepTracking)
			{
				InsertMemorySpace(ptr, size);
				_allocationCount++;
			}
			else if (_trackingType == TrackingType.CountTracking)
				_allocationCount++;
		}

		public void Untrack(void* ptr)
		{
			Untrack(ptr, out _);
		}

		public void Untrack(void* ptr, out int size)
		{
			size = 0;
			if (_trackingType == TrackingType.DeepTracking)
			{
				RemoveMemorySpace(ptr, out size);
				_allocationCount--;
			}
			else if (_trackingType == TrackingType.CountTracking)
				_allocationCount--;
		}

		private void RemoveMemorySpace(void* ptr, out int size)
		{
			var intPtr = (IntPtr)ptr;
			var removeResult = _allocations.Remove(intPtr, out size);
			E.ASSERT(removeResult);

			var low = intPtr;
			var hi = intPtr + size - 1;

			var isLowInBound = IsInBound(low, out var lowIndex);
			var isHiInBound = IsInBound(hi, out var hiIndex);
			E.ASSERT(isLowInBound && isHiInBound);
			E.ASSERT((lowIndex + 1) == hiIndex); // Индексы должны идти друг за другом

			_memorySpaces.RemoveAt(hiIndex);
			_memorySpaces.RemoveAt(lowIndex);
		}

		private void InsertMemorySpace(void* ptr, int size)
		{
			var intPtr = (IntPtr)ptr;
			var low = intPtr;
			var hi = intPtr + size - 1;

			var isLowInBound = IsInBound(low, out var lowIndex);
			var isHiInBound = IsInBound(hi, out var hiIndex);
			E.ASSERT(!isLowInBound && !isHiInBound);
			E.ASSERT(lowIndex == hiIndex); // Оба индекса должны указывать на одну и ту же область

			// Сначала добавляем верхний предел, т.к. при добавлении нижнего `hiIndex` станет не валидным
			_memorySpaces.Insert(hiIndex, hi.ToInt64());
			_memorySpaces.Insert(lowIndex, low.ToInt64());

			_allocations.Add(intPtr, size);
		}

		private bool IsInBound(byte* lowPtr, byte* hiPtr)
		{
			return IsInBound((IntPtr)lowPtr, out _) && IsInBound((IntPtr)(hiPtr - 1), out _);
		}

		/// <returns>`true` если нет коллизий, иначе `false` и индекс элемента с коллизией</returns>
		private bool IsInBound(IntPtr ptr, out int index)
		{
			index = _memorySpaces.BinarySearch(ptr.ToInt64());
			if (index >= 0)
				return true;
			index = ~index;

			return index % 2 == 1;
		}

		public UnsafeDictionary<IntPtr, int> GetAllocations()
		{
			return _allocations;
		}

		public void Dispose()
		{
			E.ASSERT(_allocationCount == 0, $"Попытка удалить трекер с не освобожденной памятью. [Количество аллокаций: {_allocationCount}]");

			_allocations.Dispose();
			_memorySpaces.Dispose();

			this = default;
		}
	}
}
