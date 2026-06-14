using Sapientia.Data;
using Sapientia.Memory;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Отражает граф связей между нодами.
	/// Содержит массивы входов и выходов для каждой ноды.
	/// Используется для создания графа исполнения нод и очерёдности их исполнения.
	/// </summary>
	public struct NodeMapHeader
	{
		public RelativePtr<NodeRelativesHeader> relatives;

		// Написать методы для создания и/или модификации (Инъекции) списка очерёдности исполнения нод.
		// Возможно учесть возможность распараллеливания цепочек нод.
		// В качестве аргументов передаётся Id<NodeHeader> от которого строится цепочка и BlueprintInstanceId.
		// А также при инъекции может передаваться индекс элемента, куда нужно сделать инъекцию.
	}

	public struct NodeRelativesHeader
	{
		public BumpArray<Id<NodeHeader>> inputs;
		public BumpArray<Id<NodeHeader>> outputs;
	}
}
