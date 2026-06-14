using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.TypeIndexer;

namespace Sapientia.LogicGraph
{
	public interface INode<out TLogicNode> : INode where TLogicNode : unmanaged, ILogicNode
	{
		TypeId<INode> INode.NodeTypeId => TypeIdOf<INode, TLogicNode>.typeId;
	}

	public interface INode
	{
		public TypeId<INode> NodeTypeId { get; }

		/// <summary>
		/// Размеры трёх регионов данных ноды (Static/Cache/Persistence, sizing-only). По умолчанию все нули —
		/// нода ничего не занимает. Out'ы ноды размещаются в голове слайса соответствующего региона и обязаны
		/// помещаться в объявленный размер.
		/// </summary>
		public DataSizes DataSizes => default;

		// Скорее всего инпуты/аутпуты не должны передаваться из ноды, возможно их нужно получать из блюпринта, хз
		public NodeInput[] GetInputs();
		public NodeOutput[] GetOutputs();
	}

	public class NodeInput<T> : NodeInput where T : unmanaged
	{
	}

	public class NodeInput
	{
	}

	/// <summary>
	/// Out, значение которого живёт в Persistence-регионе (мутабельный стейт ноды, переживает run'ы;
	/// другие ноды могут его читать/менять через указатель Map).
	/// </summary>
	public class NodeStateOutput<T> : NodeOutput<T> where T : unmanaged
	{
		public override bool IsPersistent => true;
	}

	public class NodeOutput<T> : NodeOutput where T : unmanaged
	{
		public override int DataSize => TSize<T>.size;
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
		/// True если это просто дефолтное значение и не является выходом никакой ноды (Только входом для инпута).
		/// Такие Out'ы — константы: живут в Static-регионе (read-only), дефолт бейкается при компиляции.
		/// </summary>
		public abstract bool IsPreCalculated { get; }

		/// <summary>True если значение живёт в Persistence-регионе (см. <see cref="NodeStateOutput{T}"/>); иначе — Cache.</summary>
		public virtual bool IsPersistent => false;

		/// <summary>Записывает дефолтное значение по адресу размещения (бейк в Static при компиляции).</summary>
		public abstract void SetValue(SafePtr ptr);
	}
}
