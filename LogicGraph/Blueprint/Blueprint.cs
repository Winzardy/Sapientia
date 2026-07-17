using System.Collections.Generic;
using Submodules.Sapientia.Data;
using UnityEngine;

namespace Sapientia.LogicGraph
{
	public class Blueprint
	{
		public int version;
		/// <summary>
		/// Уникальный идентификатор.
		/// Внимание! При удалении блюпринта, его id должен переиспользоваться для новых блюпринтов!
		/// Т.к. по Id мы ищем блюпринт в массиве и его индекс должен оставаться постоянным.
		/// Если Id бесконтрольно растёт и мы оставляем "дыры" в массиве, то это может привести к проблемам с памятью.
		/// </summary>
		public Id<Blueprint> id;

		[SerializeReference]
		public INode[] nodes;

		// Кеш связей: источник каждого In (по нему строится Static.Map) и обратная топология (для шедулинга).
		public Dictionary<NodeInput, NodeOutput> inputToOutput = new ();
		public Dictionary<NodeOutput, NodeInput[]> outputToInputs = new ();

		// Все аутпуты графа по порядку; отсюда берутся константы (IsPreCalculated) для Static-региона.
		public NodeOutput[] outputs;
	}
}
