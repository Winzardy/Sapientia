using System;
using System.Runtime.CompilerServices;
using Sapientia.TypeIndexer;

namespace Sapientia.LogicGraph.Data
{
	public static class NodeTypeId<T>
	{
		public static readonly NodeTypeId nodeTypeId;

		static NodeTypeId()
		{
			//IndexedTypes.GetTypeIndex(typeof(T), out nodeTypeId);
		}
	}

	public struct NodeTypeId : IEquatable<NodeTypeId>
	{
		public static readonly NodeTypeId Empty = -1;

		internal int index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NodeTypeId Create<T>()
		{
			return NodeTypeId<T>.nodeTypeId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int(NodeTypeId nodeTypeId)
		{
			return nodeTypeId.index;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator NodeTypeId(int index)
		{
			return new NodeTypeId{ index = index, };
		}

		public static bool operator ==(NodeTypeId a, NodeTypeId b)
		{
			return a.index == b.index;
		}

		public static bool operator !=(NodeTypeId a, NodeTypeId b)
		{
			return a.index != b.index;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(NodeTypeId other)
		{
			return index == other.index;
		}

		public override int GetHashCode()
		{
			return index;
		}
	}
}
