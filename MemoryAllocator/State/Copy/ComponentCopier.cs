using System;
using System.Collections;
using Sapientia.Collections;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Диспатч копира по локальному индексу компонента (<see cref="TypeId{IComponent}"/>). Генератор запекает
	/// per-type thunk-и в массивы делегатов и метит биты копируемости/пропуска — тело диспатча не генерируется.
	/// Лог о необработанном компоненте уходит в код игры через зарегистрированный делегат: Sapientia не видит WLog.
	/// </summary>
	public sealed class ComponentCopier : IComponentCopier
	{
		public delegate void AppendDelegate(WorldState world, Entity entity, ref UnsafeList<Entity> frontier);

		public delegate void CopyDelegate(WorldState oldWS, WorldState newWS, Entity oldEntity, Entity newEntity, in EntityCopyMap map);

		private readonly AppendDelegate?[] _append;
		private readonly CopyDelegate?[] _copy;
		private readonly BitArray _copiable;
		private readonly BitArray _skipped;
		private Action<TypeId<IComponent>>? _reportUnhandled;

		public ComponentCopier()
		{
			// Count готов к BeforeSceneLoad (выставляется в IndexedTypesInitializer раньше, на BeforeSplashScreen).
			var count = TypeId<IComponent>.Count;
			_append = new AppendDelegate?[count];
			_copy = new CopyDelegate?[count];
			_copiable = new BitArray(count);
			_skipped = new BitArray(count);
		}

		/// <summary>
		/// Регистрирует thunk-и компонента по локальному индексу. У плоского компонента <paramref name="append"/>
		/// = null (дочерних сущностей нет).
		/// </summary>
		public void Register(int localId, AppendDelegate? append, CopyDelegate copy)
		{
			_append[localId] = append;
			_copy[localId] = copy;
			_copiable[localId] = true;
		}

		/// <summary>
		/// Метит компонент как намеренно не копируемый (<see cref="SkipCopyAttribute"/>).
		/// </summary>
		public void MarkSkipped(int localId)
		{
			_skipped[localId] = true;
		}

		/// <summary>
		/// Регистрирует мост в код игры для лога о необработанном компоненте (WLog в Sapientia не виден).
		/// </summary>
		public void SetUnhandledReporter(Action<TypeId<IComponent>> reporter)
		{
			_reportUnhandled = reporter;
		}

		public bool IsCopiable(TypeId<IComponent> typeId)
		{
			return _copiable[typeId];
		}

		public bool IsSkipped(TypeId<IComponent> typeId)
		{
			return _skipped[typeId];
		}

		public void AppendEntities(TypeId<IComponent> typeId, WorldState world, Entity entity, ref UnsafeList<Entity> frontier)
		{
			// У плоского компонента append = null: дочерних сущностей нет, складывать нечего.
			_append[typeId]?.Invoke(world, entity, ref frontier);
		}

		public void CopyComponent(TypeId<IComponent> typeId, WorldState oldWS, WorldState newWS, Entity oldEntity, Entity newEntity, in EntityCopyMap map)
		{
			// Зовётся только после IsCopiable == true, поэтому copy-делегат заведомо не null.
			_copy[typeId]!.Invoke(oldWS, newWS, oldEntity, newEntity, in map);
		}

		public void ReportUnhandled(TypeId<IComponent> typeId)
		{
			_reportUnhandled?.Invoke(typeId);
		}
	}
}
