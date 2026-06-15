using Sapientia.Collections;
using Sapientia.Data;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Per-instance <b>InstancePersistence</b>: блок постоянного per-instance стейта поверх <see cref="UnsafeArray{T}"/>
	/// байт (off-allocator). <b>Часть состояния домена</b> (будущий слой State его сериализует); между run'ами
	/// <b>не обнуляется</b>. Фиксированного размера, не растёт — если ноде нужна динамическая память, она берёт её
	/// через контекст (напр. достаёт <c>WorldState</c> и заводит <c>MemArray</c> — 4F-2). Позиционно-независим
	/// (массив хранит сырой указатель на неподвижный блок), поэтому безопасно лежит в массиве/копируется.
	/// </summary>
	public struct InstancePersistence
	{
		private UnsafeArray<byte> _block;  // блок Persistence (off-allocator; владеет памятью)

		public readonly bool IsValid => _block.IsCreated;
		public readonly int Size => _block.Length;

		/// <summary>
		/// Создаёт InstancePersistence: выделяет обнулённый блок размера <paramref name="size"/> (== <c>compiled.GetBlockSize(Persistence)</c>).
		/// Пустой (<paramref name="size"/> &lt;= 0) → <c>default</c> (у блюпринта нет Persistence).
		/// </summary>
		public static InstancePersistence Create(Id<MemoryManager> memoryId, int size)
		{
			if (size <= 0)
				return default;

			return new InstancePersistence { _block = new UnsafeArray<byte>(memoryId, size) };
		}

		/// <summary>Указатель на блок Persistence. <c>default</c> (невалидный) — у блюпринта нет Persistence.</summary>
		public readonly SafePtr GetPtr()
		{
			return (SafePtr)_block.ptr;
		}

		/// <summary>Освобождает блок Persistence. Идемпотентно (no-op для пустого/уже освобождённого).</summary>
		public void Dispose()
		{
			_block.Dispose();
		}
	}
}
