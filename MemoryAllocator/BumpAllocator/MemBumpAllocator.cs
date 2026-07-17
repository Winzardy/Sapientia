using Sapientia.Data;
using Sapientia.Memory;
using Submodules.Sapientia.Memory;

namespace Sapientia.MemoryAllocator
{
	/// <summary>
	/// Обёртка над <see cref="BumpHeader"/> с блоком из основного <see cref="Allocator"/> (через <see cref="MemPtr"/>).
	/// База блока переезжает при пересборке аллокатора (десериализация снапшота), поэтому хранится стабильный
	/// <see cref="MemPtr"/>, а указатель на заголовок резолвится из <see cref="WorldState"/> при каждом обращении.
	/// Re-resolve не нужен: <see cref="BumpHeader"/> position-independent, достаточно получить актуальный
	/// <see cref="SafePtr{T}"/> на заголовок. Для *persistent данных, которые едут в снапшот мира.
	/// Владеет блоком: <see cref="Create"/> выделяет, <see cref="Dispose"/> освобождает.
	/// </summary>
	public struct MemBumpAllocator
	{
		public readonly MemPtr handle;

		public bool IsValid => handle.IsValid();

		/// <param name="reservedSize">Размер под данные (без заголовка) — заголовок добавляется поверх.</param>
		public MemBumpAllocator(WorldState worldState, int reservedSize)
		{
			E.ASSERT(reservedSize > 0);

			var totalSize = reservedSize + BumpHeader.HeaderSize;
			handle = worldState.MemAlloc(totalSize, out var block);

			// Основной Allocator не гарантирует обнуление — чистим под детерминизм (паритет с ClearMemory у raw-пути).
			MemoryExt.MemClear(block, totalSize);
			BumpHeader.Create(block, totalSize);
		}

		public SafePtr<BumpHeader> GetPtr(WorldState worldState)
		{
			return worldState.GetSafePtr<BumpHeader>(handle);
		}

		public ref BumpHeader GetValue(WorldState worldState)
		{
			return ref worldState.GetSafePtr<BumpHeader>(handle).Value();
		}

		public void Dispose(WorldState worldState)
		{
			worldState.MemFree(handle);
			this = default;
		}
	}
}
