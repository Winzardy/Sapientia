using System;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.TypeIndexer;

namespace Sapientia.LogicGraph
{
	public interface INode<out TLogicNode> : INode where TLogicNode : unmanaged, ILogicNode
	{
		TypeId<ILogicNode> INode.NodeTypeId => TypeIdOf<ILogicNode, TLogicNode>.typeId;

		/// <summary>Бэкенд ноды выводится из <b>capability logic-тела</b> (<see cref="ILogicNode.RuntimeType"/>), а не
		/// объявляется отдельно: забейканный <see cref="NodeHeader.runtimeType"/> ⇒ совпадает с тем, по чему реестр
		/// решал Burst-skip (M6-D). Boxing default-инстанса — только на компиляции (раз/нода), не на горячем пути.</summary>
		RuntimeType INode.RuntimeType => ((ILogicNode)default(TLogicNode)).RuntimeType;
	}

	public interface INode
	{
		/// <summary>Индекс метода обработки ноды — плотный ordinal logic-тела в контексте <see cref="ILogicNode"/>
		/// (<c>TypeIdOf&lt;ILogicNode, TLogicNode&gt;</c>). Адресует function-table диспатча (M6-C); бейкается в
		/// <see cref="NodeHeader.typeId"/>.</summary>
		public TypeId<ILogicNode> NodeTypeId { get; }

		/// <summary>
		/// Размеры трёх регионов данных ноды (Static/Cache/InstancePersistence, sizing-only). По умолчанию все нули —
		/// нода ничего не занимает. Out'ы ноды размещаются в голове слайса соответствующего региона и обязаны
		/// помещаться в объявленный размер.
		/// </summary>
		public DataSizes DataSizes => default;

		/// <summary>Бэкенд исполнения ноды (форк 8): по умолчанию Burst-unmanaged. Пишется в
		/// <c>NodeHeader.runtimeType</c> на компиляции; бакетинг батчей по нему / чередование Burst↔Managed — M7.</summary>
		public RuntimeType RuntimeType => RuntimeType.Unmanaged;

		// Скорее всего инпуты/аутпуты не должны передаваться из ноды, возможно их нужно получать из блюпринта, хз
		public NodeInput[] GetInputs();
		public NodeOutput[] GetOutputs();

		/// <summary>
		/// Типы ambient-контекста (<c>TypeId&lt;INodeContext&gt;</c>), которые нода достаёт при исполнении. По умолчанию
		/// пусто. Компилятор собирает union по всем нодам (дедуп) в <c>CompiledBlueprintHeader.contextTypes</c> (4E);
		/// на скомпилированной ноде список не хранится. Резолв «тип → указатель» — через <c>ExecutionScope</c> (4F).
		/// </summary>
		public TypeId<INodeContext>[] GetContextTypes() => Array.Empty<TypeId<INodeContext>>();
	}

	public class NodeInput<T> : NodeInput where T : unmanaged
	{
	}

	public class NodeInput
	{
	}

	/// <summary>
	/// Out, значение которого живёт в InstancePersistence-регионе (мутабельный стейт ноды, переживает run'ы;
	/// другие ноды могут его читать/менять через указатель Map).
	/// </summary>
	public class NodeStateOutput<T> : NodeOutput<T> where T : unmanaged
	{
		public override bool IsPersistent => true;
	}

	public class NodeOutput<T> : NodeOutput where T : unmanaged
	{
		public override int DataSize => TSize<T>.size;
		/// <summary>Размер одной ячейки метаданных <see cref="CacheLink"/> (тег + офсеты, без значения; значение —
		/// отдельным блоком, размер = <see cref="DataSize"/>). Слот Cache-Out'а в массиве метаданных.</summary>
		public override int CacheCellSize => TSize<CacheLink>.size;
		public override bool IsPreCalculated => false;
		public virtual T DefaultValue => default;

		public override void SetValue(SafePtr ptr)
		{
			ptr.Value<T>() = DefaultValue;
		}
	}

	public abstract class NodeOutput
	{
		public abstract int DataSize { get; }

		/// <summary>
		/// Размер слота Out'а в <b>Cache</b>-регионе: для Cache-Out'ов это размер ячейки <see cref="CacheLink"/>
		/// (тег+payload, см. <see cref="NodeOutput{T}"/>), а не сырого значения. Static/InstancePersistence используют
		/// <see cref="DataSize"/>. По умолчанию = <see cref="DataSize"/>.
		/// </summary>
		public virtual int CacheCellSize => DataSize;

		/// <summary>
		/// True если это просто дефолтное значение и не является выходом никакой ноды (Только входом для инпута).
		/// Такие Out'ы — константы: живут в Static-регионе (read-only), дефолт бейкается при компиляции.
		/// </summary>
		public abstract bool IsPreCalculated { get; }

		/// <summary>True если значение живёт в InstancePersistence-регионе (см. <see cref="NodeStateOutput{T}"/>); иначе — Cache.</summary>
		public virtual bool IsPersistent => false;

		/// <summary>Записывает дефолтное значение по адресу размещения (бейк в Static при компиляции).</summary>
		public abstract void SetValue(SafePtr ptr);
	}
}
