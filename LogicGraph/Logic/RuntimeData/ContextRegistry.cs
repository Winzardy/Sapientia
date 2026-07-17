using System;
using Sapientia.Collections;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.TypeIndexer;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Ambient-context-реестр scope'а для категории контекста <typeparamref name="TContext"/> (напр.
	/// <see cref="INodeContext"/>): «конкретный тип контекста → указатель», индексируется по локальному id типа в
	/// категории (<c>(int)TypeId&lt;TContext&gt;</c>). Ноды при исполнении (M7) достают нужный контекст по типу
	/// (<see cref="GetContext{T}"/>); какие типы нужны блюпринту — забейкано компилятором в
	/// <see cref="CompiledBlueprintHeader.contextTypes"/> (4E).
	/// </summary>
	/// <remarks>
	/// Реестр <b>владеет</b> памятью контекстов: блок под контекст выделяется <b>при установке</b>
	/// (<see cref="SetContext{T}"/>, размер — <see cref="TSize{T}"/>), <see cref="_slots"/> держит на него
	/// <see cref="SafePtr"/>. Блоки — off-allocator raw (<see cref="MemoryManager"/>), позиции стабильны ⇒ указатель
	/// не протухает; освобождение — в <see cref="Dispose"/>. Число слотов — <c>TypeId&lt;TContext&gt;.Count</c> (все типы
	/// категории), считается <b>внутри</b> (не передаётся). Позиционно-независим (<see cref="UnsafeArray{T}"/> хранит
	/// сырой указатель) — безопасно копируется, в снапшот мира не идёт (transient ambient-state).
	/// <para><b>Индекс:</b> <c>TypeId&lt;TContext&gt;</c> <b>неявно приводится к <c>int</c></b> (локальный id типа в
	/// категории — <c>implicit operator int</c>), поэтому используется как индекс массива <b>напрямую</b>; явный
	/// <c>(int)</c>-каст не нужен.</para>
	/// <para><b>Тесты:</b> в EditMode-окружении <c>IndexedTypes</c> не инициализирован ⇒ <c>TypeId&lt;TContext&gt;.Count</c>
	/// = 0 ⇒ реестр пуст; функциональные тесты round-trip требуют зарегистрированных типов контекста (отложено).</para>
	/// </remarks>
	public struct ContextRegistry<TContext> : IDisposable where TContext : IIndexedType
	{
		private Id<MemoryManager> _memoryId;
		private UnsafeArray<SafePtr> _slots; // индекс = (int)TypeId<TContext>; SafePtr на реестр-владеемый блок

		public readonly bool IsCreated => _slots.IsCreated;
		public readonly int Capacity => _slots.Length;

		/// <summary>
		/// Создаёт реестр на <c>TypeId&lt;TContext&gt;.Count</c> слотов (все типы категории). Слоты обнулены (контексты не
		/// заданы). Если типов категории нет (<c>Count</c> &lt;= 0, в т.ч. неинициализированный <c>IndexedTypes</c>) → <c>default</c>.
		/// </summary>
		public static ContextRegistry<TContext> Create(Id<MemoryManager> memoryId)
		{
			var count = TypeId<TContext>.Count;
			if (count <= 0)
				return default;

			return new ContextRegistry<TContext>
			{
				_memoryId = memoryId,
				_slots = new UnsafeArray<SafePtr>(memoryId, count),
			};
		}

		/// <summary>Кладёт контекст <typeparamref name="T"/> по значению: при первой установке реестр выделяет <see cref="TSize{T}"/> и копирует; повтор — перезапись в тот же блок.</summary>
		public void SetContext<T>(in T value) where T : unmanaged, TContext
		{
			var id = TypeIdOf<TContext, T>.typeId;
			E.ASSERT(id >= 0 && id < _slots.Length, "[ContextRegistry] SetContext: тип контекста не зарегистрирован / id вне границ.");

			ref var slot = ref _slots[id];
			if (!slot.IsValid)
				slot = _memoryId.GetManager().MemAlloc(TSize<T>.size, ClearOptions.ClearMemory);
			slot.Value<T>() = value;
		}

		/// <summary>
		/// <c>readonly ref</c> на контекст <typeparamref name="T"/> (read-only — менять ambient-контекст через реестр
		/// нельзя, только <see cref="SetContext{T}"/>). Контекст обязан быть задан (см. <see cref="HasContext{T}"/>) —
		/// иначе DEBUG-assert. Резолв нодой в run'е — M7.
		/// </summary>
		public readonly ref readonly T GetContext<T>() where T : unmanaged, TContext
		{
			var id = TypeIdOf<TContext, T>.typeId;
			E.ASSERT(id >= 0 && id < _slots.Length, "[ContextRegistry] GetContext: тип контекста не зарегистрирован / id вне границ.");

			var block = _slots[id];
			E.ASSERT(block.IsValid, "[ContextRegistry] GetContext: контекст не задан (вызови SetContext / проверь HasContext).");
			return ref block.Value<T>();
		}

		/// <summary>Задан ли контекст <typeparamref name="T"/> (тип в границах реестра и указатель валиден).</summary>
		public readonly bool HasContext<T>() where T : TContext
		{
			var id = TypeIdOf<TContext, T>.typeId;
			return id >= 0 && id < _slots.Length && _slots[id].IsValid;
		}

		/// <summary>Освобождает блоки всех заданных контекстов и сам массив слотов. Идемпотентно.</summary>
		public void Dispose()
		{
			if (_slots.IsCreated)
			{
				ref var manager = ref _memoryId.GetManager();
				for (var i = 0; i < _slots.Length; i++)
				{
					var block = _slots[i];
					if (block.IsValid)
						manager.MemFree(block);
				}
			}

			_slots.Dispose();
		}
	}
}
