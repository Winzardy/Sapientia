using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public interface IMemLinkedListEnumerable<T>
		where T: unmanaged
	{
		protected MemLinkedList<T> GetList(WorldState worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemLinkedListEnumerator<T> GetEnumerator(WorldState worldState)
		{
			return new MemLinkedListEnumerator<T>(worldState, GetList(worldState));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemLinkedListEnumerable<T> GetEnumerable(WorldState worldState)
		{
			return new (GetEnumerator(worldState));
		}
	}

	public readonly ref struct MemLinkedListEnumerable<T>
		where T : unmanaged
	{
		private readonly MemLinkedListEnumerator<T> _enumerator;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal MemLinkedListEnumerable(MemLinkedListEnumerator<T> enumerator)
		{
			_enumerator = enumerator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemLinkedListEnumerator<T> GetEnumerator()
		{
			return _enumerator;
		}
	}

	public ref struct MemLinkedListEnumerator<T> where T: unmanaged
	{
		private WorldState _worldState;
		private MemLinkedList<T> _list;
		private MemLinkedListNode<T> _node;
		private MemLinkedListNode<T> _current;
		private int _index;

		internal MemLinkedListEnumerator(WorldState worldState, MemLinkedList<T> list)
		{
			_worldState = worldState;
			_list = list;
			_node = list.GetFirst(worldState);
			_current = default;
			_index = 0;
		}

		public MemLinkedListNode<T> Current => _current;

		public bool MoveNext()
		{
			if (!_node.IsValid())
			{
				_index = _list.GetCount(_worldState) + 1;
				return false;
			}

			++_index;

			_current = _node;
			_node = _node.GetNext(_worldState);

			if (_node == _list.GetFirst(_worldState))
				_node = default;

			return true;
		}

		public void Reset()
		{
			_current = default;
			_node = _list.GetFirst(_worldState);
			_index = 0;
		}
	}
}
