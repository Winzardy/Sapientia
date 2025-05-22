using System;
using System.Collections.Generic;

namespace Sapientia.Collections
{
	/// <summary>
	/// Обобщённая структура для хранения и поиска значений по рекурсивной цепочке ключей.
	///
	/// Используется, когда результат <typeparamref name="TValue"/> зависит не от одного ключа,
	/// а от уникальной комбинации нескольких <typeparamref name="TKey"/>.
	///
	/// Структура построена в виде дерева, где каждый узел содержит значение и вложенные саб-ветки,
	/// позволяя эффективно находить самое глубокое совпадение среди всех возможных путей.
	///
	/// Поддерживает произвольные типы ключей <typeparamref name="TKey"/> и значений <typeparamref name="TValue"/>.
	///
	/// Пример применения: отображение иконки по набору характеристик, конфигурации по условиям, приоритетные правила.
	/// </summary>
	public abstract class RecursivePathMap<TValue, TKey, TNode>
		where TNode : class, IRecursiveMapNode<TValue, TKey, TNode>
	{
		public abstract TNode Root { get; }

		public TValue this[IList<TKey> path] => Find(path);

		public TValue Find(IList<TKey> keys)
		{
			TValue targetValue = default;
			int maxDepth = -1;

			Recurse(Root, 0, 0);

			return targetValue ?? Root.Value;

			void Recurse(TNode currentNode, int depth, int usedMask)
			{
				// если у текущего узла есть Value, и глубина максимальная — сохраняем
				if (currentNode.Value != null && depth > maxDepth)
				{
					targetValue = currentNode.Value;
					maxDepth = depth;
				}

				if (currentNode.Dictionary == null)
					return;

				for (int i = 0; i < keys.Count; i++)
				{
					if ((usedMask & (1 << i)) != 0)
						continue;

					var key = keys[i];
					if (currentNode.Dictionary.TryGetValue(key, out var nextNode))
						Recurse(nextNode, depth + 1, usedMask | (1 << i));
				}
			}
		}
	}

	public interface IRecursiveMapNode<TValue, TKey, TNode>
		where TNode : IRecursiveMapNode<TValue, TKey, TNode>
	{
		TValue Value { get; }
		Dictionary<TKey, TNode> Dictionary { get; }
	}

	public abstract class SerializableRecursivePathMap<TValue, TKey> : RecursivePathMap<TValue, TKey, Node<TValue, TKey>>
	{
		public Node<TValue, TKey> root;
		public override Node<TValue, TKey> Root => Root;
	}

	[Serializable]
	public class Node<TValue, TKey> : IRecursiveMapNode<TValue, TKey, Node<TValue, TKey>>
	{
		public TValue value;
		public SerializableDictionary<TKey, Node<TValue, TKey>> dictionary;
		public TValue Value => value;
		public Dictionary<TKey, Node<TValue, TKey>> Dictionary => dictionary;
	}
}
