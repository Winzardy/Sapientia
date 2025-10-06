using System;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.LogicGraph.Data;
using Sapientia.LogicGraph.Logic;
using Sapientia.Memory;

namespace Sapientia.LogicGraph
{
	public interface INode<out TLogicNode> : INode where TLogicNode : unmanaged, ILogicNode
	{
		NodeTypeId INode.NodeTypeId => NodeTypeId.Create<TLogicNode>();

		int INode.BodySize => TSize<TLogicNode>.size;

		TLogicNode CreateBody() => default;

		void INode.SetBody(SafePtr bodyPtr)
		{
			bodyPtr.Value<TLogicNode>() = CreateBody();
		}

		// Скорее всего инпуты/аутпуты не должны передаваться из ноды, возможно их нужно получать из блюпринта, хз
		NodeInput[] INode.GetInputs()
		{
			throw new NotImplementedException();
		}
		NodeOutput[] INode.GetOutputs()
		{
			throw new NotImplementedException();
		}
		NodeBody[] INode.GetBodies()
		{
			throw new NotImplementedException();
		}
		NodeState[] INode.GetStates()
		{
			throw new NotImplementedException();
		}
	}

	public interface INode
	{
		public NodeTypeId NodeTypeId { get; }

		public NodeInput[] GetInputs();
		public NodeOutput[] GetOutputs();
		public NodeBody[] GetBodies();
		public NodeState[] GetStates();

		public int BodySize => 0;
		public void SetBody(SafePtr bodyPtr);

		public int StateSize => 0;
		public void SetStateAndOutput(ref CompiledBlueprint compiledBlueprint, SafePtr statePtr, SafePtr<EdgeToData> outputPtr);
	}

	public class NodeStateInput<T> : NodeInput<T> where T : unmanaged
	{
	}

	public class NodeInput<T> : NodeInput where T : unmanaged
	{
	}

	public class NodeInput
	{
	}

	public class NodeBody<T> : NodeBody where T : unmanaged
	{
		public override int DataSize => TSize<T>.size;
		public virtual T Value => default;
		public override void SetValue(SafePtr<EdgeDataHeader> ptr)
		{
			ptr.Cast<EdgeData<T>>().Value().data = Value;
		}
	}

	public abstract class NodeBody
	{
		public PtrOffset BodyOffset { get; set; }

		public abstract int DataSize { get; }
		/// <summary>
		/// Устанавливает дефолтное значение.
		/// </summary>
		public abstract void SetValue(SafePtr<EdgeDataHeader> ptr);
	}

	public class NodeState<T> : NodeState where T : unmanaged
	{
		public override int DataSize => TSize<T>.size;
		public virtual T DefaultValueValue => default;
		public override void SetPreCalculated(SafePtr<EdgeDataHeader> ptr)
		{
			ptr.Cast<EdgeData<T>>().Value().data = DefaultValueValue;
		}
	}

	public abstract class NodeState
	{
		public abstract int DataSize { get; }
		/// <summary>
		/// Устанавливает дефолтное значение.
		/// </summary>
		public abstract void SetPreCalculated(SafePtr<EdgeDataHeader> ptr);
	}

	public class NodeStateOutput<T> : NodeOutput<T> where T : unmanaged
	{
		public NodeState<T> passThroughState = new NodeState<T>();

		public override NodeState PassThroughState => passThroughState;
	}

	public class NodeOutput<T> : NodeOutput where T : unmanaged
	{
		public override int DataSize => TSize<T>.size;
		public override bool IsPreCalculated => false;
		public virtual T DefaultValue => default;
		public override void SetPreCalculated(SafePtr<EdgeDataHeader> ptr)
		{
			if (PassThroughState == null)
				ptr.Cast<EdgeData<T>>().Value().data = DefaultValue;
			else
				PassThroughState.SetPreCalculated(ptr);
		}

		public override NodeState PassThroughState => null;
	}

	public abstract class NodeOutput
	{
		public abstract int DataSize { get; }
		/// <summary>
		/// True если это просто дефолтное значение и не является выходом никакой ноды (Только входом для инпута)
		/// </summary>
		public abstract bool IsPreCalculated { get; }
		/// <summary>
		/// Устанавливает дефолтное значение.
		/// </summary>
		public abstract void SetPreCalculated(SafePtr<EdgeDataHeader> ptr);

		/// <summary>
		/// True если значение хранится в стейте ноды и вместо значения output хранит ссылку на значение.
		/// Например, если мы хотим чтобы это значение было изменяемым из других нод.
		/// </summary>
		public abstract NodeState PassThroughState { get; }
	}
}
