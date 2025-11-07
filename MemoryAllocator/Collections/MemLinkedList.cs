using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sapientia.MemoryAllocator
{
	[DebuggerTypeProxy(typeof(MemLinkedList<>.LinkedListProxy))]
	public struct MemLinkedList<T> : IMemLinkedListEnumerable<T> where T : unmanaged
	{
		private struct MemLinkedListData
		{
			// This LinkedList is a doubly-Linked circular list.
			public MemLinkedListNode<T> head;
			public int count;
		}

		private CachedPtr<MemLinkedListData> _data;

#if DEBUG
		private WorldId _worldId;
#endif

		public int GetCount(WorldState worldState) => _data.GetValue(worldState).count;

		public bool IsValid() => _data.IsValid();

#if DEBUG
		internal WorldState GetWorldState_DEBUG()
		{
			return _worldId.GetWorldState();
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemLinkedList(WorldState worldState)
		{
			_data = CachedPtr<MemLinkedListData>.Create(worldState);
#if DEBUG
			_worldId = worldState.WorldId;
#endif
		}

		public MemLinkedListNode<T> GetFirst(WorldState worldState)
		{
			return _data.GetValue(worldState).head;
		}

		public MemLinkedListNode<T> GetLast(WorldState worldState)
		{
			ref var head = ref _data.GetValue(worldState).head;
			return head.IsValid() ? head.GetPrevious(worldState) : default;
		}

		public MemLinkedListNode<T> AddAfter(WorldState worldState, MemLinkedListNode<T> node, T value)
		{
			ValidateNode(worldState, node);

			var result = MemLinkedListNodeData<T>.Create(worldState, this, value);
			InternalInsertNodeBefore(worldState, node.GetNext(worldState), result);
			return result;
		}

		public MemLinkedListNode<T> AddBefore(WorldState worldState, MemLinkedListNode<T> node, T value)
		{
			ref var data = ref _data.GetValue(worldState);

			ValidateNode(worldState, node);

			var result = MemLinkedListNodeData<T>.Create(worldState, this, value);
			InternalInsertNodeBefore(worldState, node, result);
			if (node == data.head)
				data.head = result;

			return result;
		}

		public MemLinkedListNode<T> AddFirst(WorldState worldState, T value)
		{
			ref var data = ref _data.GetValue(worldState);

			var result = MemLinkedListNodeData<T>.Create(worldState, this, value);
			if (data.head.IsValid())
			{
				InternalInsertNodeBefore(worldState, data.head, result);
				data.head = result;
			}
			else
			{
				InternalInsertNodeToEmptyList(worldState, result);
			}

			return result;
		}

		public MemLinkedListNode<T> AddLast(WorldState worldState, T value)
		{
			ref var data = ref _data.GetValue(worldState);

			var result = MemLinkedListNodeData<T>.Create(worldState, this, value);
			if (data.head.IsValid())
			{
				InternalInsertNodeBefore(worldState, data.head, result);
			}
			else
			{
				InternalInsertNodeToEmptyList(worldState, result);
			}

			return result;
		}

		public void Clear(WorldState worldState)
		{
			ref var data = ref _data.GetValue(worldState);

			var current = data.head;
			while (current.IsValid())
			{
				var temp = current;
				current = temp.GetNext(worldState); // use Next the instead of "next", otherwise it will loop forever
				temp.Dispose(worldState);
			}

			data.head = default;
			data.count = 0;
		}

		public void Dispose(WorldState worldState)
		{
			Clear(worldState);
			_data.Dispose(worldState);
		}

		public bool Contains(WorldState worldState, T value)
		{
			return Find(worldState, value).IsValid();
		}

		public MemLinkedListNode<T> Find(WorldState worldState, T value)
		{
			ref var data = ref _data.GetValue(worldState);

			var node = data.head;
			EqualityComparer<T> c = EqualityComparer<T>.Default;
			if (node.IsValid())
			{
				do
				{
					ref var nodeValue = ref node.GetData(worldState);
					if (c.Equals(nodeValue.value, value))
					{
						return node;
					}

					node = nodeValue.next;
				} while (node != data.head);
			}

			return default;
		}

		public MemLinkedListNode<T> FindLast(WorldState worldState, T value)
		{
			ref var data = ref _data.GetValue(worldState);

			if (!data.head.IsValid())
				return default;

			var last = data.head.GetPrevious(worldState);
			var node = last;
			EqualityComparer<T> c = EqualityComparer<T>.Default;
			if (node.IsValid())
			{
				do
				{
					ref var nodeData = ref node.GetData(worldState);
					if (c.Equals(nodeData.value, value))
					{
						return node;
					}

					node = nodeData.prev;
				} while (node != last);
			}

			return default;
		}

		MemLinkedList<T> IMemLinkedListEnumerable<T>.GetList(WorldState worldState)
		{
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemLinkedListEnumerator<T> GetEnumerator(WorldState worldState)
		{
			return new MemLinkedListEnumerator<T>(worldState, this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemLinkedListEnumerable<T> GetEnumerable(WorldState worldState)
		{
			return new MemLinkedListEnumerable<T>(GetEnumerator(worldState));
		}

		public bool Remove(WorldState worldState, T value)
		{
			var node = Find(worldState, value);
			if (node.IsValid())
			{
				InternalRemoveNode(worldState, node);
				return true;
			}

			return false;
		}

		public void Remove(WorldState worldState, ref MemLinkedListNode<T> node)
		{
			ValidateNode(worldState, node);
			InternalRemoveNode(worldState, node);
			node = default;
		}

		public void RemoveFirst(WorldState worldState)
		{
			ref var data = ref _data.GetValue(worldState);

			if (!data.head.IsValid())
			{
				throw new InvalidOperationException("Попытка удалить ноду из пустого MemLinkedList");
			}

			InternalRemoveNode(worldState, data.head);
		}

		public void RemoveLast(WorldState worldState)
		{
			ref var data = ref _data.GetValue(worldState);

			if (!data.head.IsValid())
			{
				throw new InvalidOperationException("Попытка удалить ноду из пустого MemLinkedList");
			}

			InternalRemoveNode(worldState, data.head.GetPrevious(worldState));
		}

		private void InternalInsertNodeBefore(WorldState worldState, CachedPtr<MemLinkedListNodeData<T>> node, CachedPtr<MemLinkedListNodeData<T>> newNode)
		{
			ref var data = ref _data.GetValue(worldState);

			newNode.GetValue(worldState).next = node;
			newNode.GetValue(worldState).prev = node.GetValue(worldState).prev;
			node.GetValue(worldState).prev.GetValue(worldState).next = newNode;
			node.GetValue(worldState).prev = newNode;
			data.count++;

#if DEBUG
			_worldId = worldState.WorldId;
#endif
		}

		private void InternalInsertNodeToEmptyList(WorldState worldState, CachedPtr<MemLinkedListNodeData<T>> newNode)
		{
			ref var data = ref _data.GetValue(worldState);
			Debug.Assert(!data.head.IsValid() && data.count == 0, "LinkedList must be empty when this method is called!");

			newNode.GetValue(worldState).next = newNode;
			newNode.GetValue(worldState).prev = newNode;
			data.head = newNode;
			data.count++;

#if DEBUG
			_worldId = worldState.WorldId;
#endif
		}

		private void InternalRemoveNode(WorldState worldState, CachedPtr<MemLinkedListNodeData<T>> node)
		{
			ref var data = ref _data.GetValue(worldState);
			ref var nodeValue = ref node.GetValue(worldState);
			Debug.Assert(nodeValue.list._data == _data, "Deleting the node from another list!");
			Debug.Assert(data.head.IsValid(), "This method shouldn't be called on empty list!");
			if (nodeValue.next == node)
			{
				Debug.Assert(data.count == 1 && data.head == node, "this should only be true for a list with only one node");
				data.head = default;
			}
			else
			{
				nodeValue.next.GetValue(worldState).prev = nodeValue.prev;
				nodeValue.prev.GetValue(worldState).next = nodeValue.next;
				if (data.head == node)
				{
					data.head = nodeValue.next;
				}
			}

			node.Dispose(worldState);

			data.count--;
		}

		internal void ValidateNewNode(WorldState worldState, ref MemLinkedListNodeData<T> node)
		{
			if (node.list.IsValid())
			{
				throw new InvalidOperationException("Нода уже прикреплена!");
			}

#if DEBUG
			if (_worldId.IsValid() && _worldId != worldState.WorldId)
			{
				throw new InvalidOperationException("Попытка использовать MemLinkedList, используя другой WorldState.");
			}
#endif
		}

		private void ValidateNode(WorldState worldState, CachedPtr<MemLinkedListNodeData<T>> node)
		{
			if (node.IsValid())
			{
				throw new ArgumentNullException("node");
			}

			if (node.GetValue(worldState).list.GetFirst(worldState) == GetFirst(worldState))
			{
				throw new InvalidOperationException("В MemLinkedList Была передана некорректная нода.");
			}

#if DEBUG
			if (_worldId.IsValid() && _worldId != worldState.WorldId)
			{
				throw new InvalidOperationException("Попытка использовать MemLinkedList, используя другой WorldState.");
			}
#endif
		}

		private class LinkedListProxy
		{
			private MemLinkedList<T> _list;

			public LinkedListProxy(MemLinkedList<T> list)
			{
				_list = list;
			}

			public int Count
			{
				get
				{
#if DEBUG
					var worldState = _list.GetWorldState_DEBUG();
					return _list.GetCount(worldState);
#endif
					return default;
				}
			}

			public T[] Items
			{
				get
				{
#if DEBUG
					var worldState = _list.GetWorldState_DEBUG();
					var arr = new T[_list.GetCount(worldState)];

					var i = 0;
					foreach (var value in _list.GetEnumerable(worldState))
					{
						arr[i++] = value.GetValue(worldState);
					}

					return arr;
#else
					return Array.Empty<T>();
#endif
				}
			}
		}
	}

	// Note following class is not serializable since we customized the serialization of LinkedList.
	[System.Runtime.InteropServices.ComVisible(false)]
	public struct MemLinkedListNodeData<T> where T : unmanaged
	{
		internal MemLinkedList<T> list;
		internal CachedPtr<MemLinkedListNodeData<T>> next;
		internal CachedPtr<MemLinkedListNodeData<T>> prev;

		public T value;

		internal static CachedPtr<MemLinkedListNodeData<T>> Create(WorldState worldState, MemLinkedList<T> list, T value)
		{
			var node = new MemLinkedListNodeData<T>
			{
				list = list,
				value = value,
			};
			return CachedPtr<MemLinkedListNodeData<T>>.Create(worldState, node);
		}
	}

	[System.Runtime.InteropServices.ComVisible(false)]
	public struct MemLinkedListNode<T> : IEquatable<MemLinkedListNode<T>> where T : unmanaged
	{
		internal CachedPtr<MemLinkedListNodeData<T>> cachedPtr;

		public MemLinkedList<T> GetList(WorldState worldState) => cachedPtr.GetValue(worldState).list;
		public MemLinkedListNode<T> GetNext(WorldState worldState) => cachedPtr.GetValue(worldState).next;
		public MemLinkedListNode<T> GetPrevious(WorldState worldState) => cachedPtr.GetValue(worldState).prev;
		public ref T GetValue(WorldState worldState) => ref cachedPtr.GetValue(worldState).value;
		internal ref MemLinkedListNodeData<T> GetData(WorldState worldState) => ref cachedPtr.GetValue(worldState);

		public bool IsValid() => cachedPtr.IsValid();

		public static implicit operator CachedPtr<MemLinkedListNodeData<T>>(MemLinkedListNode<T> node)
		{
			return node.cachedPtr;
		}

		public static implicit operator MemLinkedListNode<T>(CachedPtr<MemLinkedListNodeData<T>> node)
		{
			return new MemLinkedListNode<T>()
			{
				cachedPtr = node,
			};
		}

		public static bool operator ==(MemLinkedListNode<T> a, CachedPtr<MemLinkedListNodeData<T>> b)
		{
			return a.cachedPtr == b;
		}

		public static bool operator !=(MemLinkedListNode<T> a, CachedPtr<MemLinkedListNodeData<T>> b)
		{
			return a.cachedPtr != b;
		}

		public static bool operator ==(CachedPtr<MemLinkedListNodeData<T>> a, MemLinkedListNode<T> b)
		{
			return a == b.cachedPtr;
		}

		public static bool operator !=(CachedPtr<MemLinkedListNodeData<T>> a, MemLinkedListNode<T> b)
		{
			return a != b.cachedPtr;
		}

		public static bool operator ==(MemLinkedListNode<T> a, MemLinkedListNode<T> b)
		{
			return a.cachedPtr == b.cachedPtr;
		}

		public static bool operator !=(MemLinkedListNode<T> a, MemLinkedListNode<T> b)
		{
			return a.cachedPtr != b.cachedPtr;
		}

		public void Dispose(WorldState worldState)
		{
			cachedPtr.Dispose(worldState);
		}

		public bool Equals(MemLinkedListNode<T> other)
		{
			return cachedPtr == other.cachedPtr;
		}

		public override bool Equals(object obj)
		{
			return obj is MemLinkedListNode<T> other && Equals(other);
		}

		public override int GetHashCode()
		{
			return cachedPtr.GetHashCode();
		}
	}
}
