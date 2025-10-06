using System;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.LogicGraph.Logic
{
	[Serializable]
	// Набросал чисто чтобы примерно было понятно как пришивать к логике
	// На стороне графа редактора
	public partial class AddNode : INode<AddLogicNode>
	{
		// Можно это вместо обёртки атрибутами реализовать
		// Может быть связано как с NodeOutput, так и с NodeStateOutput (Нам без разницы, мы только читаем и не можем его менять)
		public NodeInput<int> a;
		// Это значение должно быть связано с NodeStateOutput другой ноды
		public NodeStateInput<int> b;

		public NodeOutput<int> result;
		// Это значение лежит в стейте, но привязано к аутпуту чтобы можно было его читать из других нод
		// Если делать через атрибуты, то просто 2 атрибута на поле фигануть (Аутпут и стейт, они совместимы между собой)
		public NodeStateOutput<int> lastResult = new NodeStateOutput<int>()
		{
			passThroughState = new NodeState<int>(),
		};

		public NodeBody<int> c;

		public NodeState<int> someData;
	}

	// Тут кодогенерация
	public partial class AddNode
	{
		AddLogicNode INode<AddLogicNode>.CreateBody()
		{
			return new AddLogicNode
			(
				c.Value
			);
		}

		int INode.StateSize => TSize<AddLogicNode.State>.size;

		public void SetStateAndOutput(ref CompiledBlueprint compiledBlueprint, SafePtr statePtr, SafePtr<EdgeToData> outputPtr)
		{
			ref var state = ref statePtr.Value<AddLogicNode.State>();
			state.lastResult = lastResult.DefaultValue;
			state.someData = someData.DefaultValueValue;

			ref var output = ref outputPtr.Value<AddLogicNode.Output>();
			output.lastResult.GetOffset(ref compiledBlueprint) = (state.lastResult.AsSafePtr() - statePtr);
			output.result.GetData(ref compiledBlueprint) = result.DefaultValue;
		}
	}

	// На стороне графа логики
	// Тут кодогенерация
	public readonly partial struct AddLogicNode : ILogicNode<AddLogicNode.Input, AddLogicNode.Output, AddLogicNode.State>
	{
		public struct Input
		{
			// Эта значение может как лежать в стейте, так и быть обычным аутпутом
			// Но мы всегда его читаем и не меняем
			public InputData<int> a;
			// Это значение лежит в стейте другой ноды
			// Мы точно знаем что его можно менять и мы специально указали это в NodeStateInput
			public StateData<int> b;
		}

		public struct Output
		{
			// Это значение не лежит в стейте, но является аутпутом и мы можем его менять
			public OutputData<int> result;
			// Это значение лежит в стейте, к нему можно получить доступ и от сюда тоже
			// Мы его не убираем чтобы не ломать каст данных к этому Output
			public StateData<int> lastResult;
		}

		public struct State
		{
			public int lastResult;
			public int someData;
		}

		// Это тело ноды, которое копируется из ноды редактора.
		// Это статические данные для всех инстансов, поэтому их изменение не желательно.
		public readonly int c;

		public AddLogicNode(int c)
		{
			this.c = c;
		}
	}

	public partial struct AddLogicNode
	{
		// Тут скорее всего будет несколько возможных методов
		// - Один под бёрстом может работать
		// - Какие-то скорее всего без бёрста
		public void DoBurst(ref CompiledBlueprint compiledBlueprint, NodeId nodeId, in Input input, in Output output, SafePtr<State> state)
		{
			var a = input.a.ReadData(ref compiledBlueprint, state);
			var b = input.b.GetData(ref compiledBlueprint, state);

			ref var result = ref output.result.GetData(ref compiledBlueprint);
			state.Value().lastResult = result;

			result += a + b - c;
		}
	}
}
