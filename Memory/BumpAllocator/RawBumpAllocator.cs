using System;
using Sapientia.Data;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.Memory
{
	/// <summary>
	/// Обёртка над <see cref="BumpHeader"/> с сырым нативным блоком из <see cref="MemoryExt"/>.
	/// База блока неподвижна — путь по умолчанию для standalone/server/тестов и сериализуемого бинаря
	/// (блок арены = сам бинарь). Владеет жизненным циклом блока: <see cref="Create"/> выделяет,
	/// <see cref="Dispose"/> освобождает. <see cref="BumpHeader"/> остаётся memory-agnostic.
	/// </summary>
	public struct RawBumpAllocator : IDisposable
	{
		public readonly SafePtr<BumpHeader> header;
		public readonly Id<MemoryManager> memoryId;

		public bool IsValid => header.IsValid;
		public ref BumpHeader Value => ref header.Value();

		/// <param name="reservedSize">Размер под данные (без заголовка) — заголовок добавляется поверх.</param>
		public RawBumpAllocator(int reservedSize) : this(default, reservedSize)
		{
		}

		/// <param name="memoryId">Id менеджера для аллокации памяти.</param>
		/// <param name="reservedSize">Размер под данные (без заголовка) — заголовок добавляется поверх.</param>
		public RawBumpAllocator(Id<MemoryManager> memoryId, int reservedSize)
		{
			E.ASSERT(reservedSize > 0);

			var totalSize = reservedSize + BumpHeader.HeaderSize;
			var block = memoryId.GetManager().MemAlloc(totalSize, ClearOptions.ClearMemory);

			this.memoryId = memoryId;
			this.header = BumpHeader.Create(block, totalSize);
		}

		/// <param name="memoryId">Id менеджера для аллокации памяти.</param>
		/// <param name="header">Данные.</param>
		private RawBumpAllocator(Id<MemoryManager> memoryId, SafePtr<BumpHeader> header)
		{
			this.memoryId = memoryId;
			this.header = header;
		}

		public void Serialize(ref StreamBufferWriter stream)
		{
			header.Value().Serialize(ref stream);
		}

		public static RawBumpAllocator Deserialize(ref StreamBufferReader stream)
		{
			return Deserialize(default, ref stream);
		}

		public static RawBumpAllocator Deserialize(Id<MemoryManager> memoryId, ref StreamBufferReader stream)
		{
			// В стрим записан полный размер блока (включая заголовок) — выделяем ровно столько.
			var totalSize = stream.Read<int>();
			var block = memoryId.GetManager().MemAlloc(totalSize, ClearOptions.ClearMemory);
			stream.Read(block, totalSize);

			return new RawBumpAllocator(memoryId, block);
		}

		public void Dispose()
		{
			if (!IsValid)
				return;
			memoryId.GetManager().MemFree((SafePtr)header);
			this = default;
		}
	}
}
