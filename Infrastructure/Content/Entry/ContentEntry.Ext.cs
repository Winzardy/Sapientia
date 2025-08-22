using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Fusumity.Attributes.Odin;

namespace Content
{
	[Serializable]
	[SingleFieldInline]
	public struct ContentEntry<T, TSub>
		where TSub : T
	{
		[ContentTypeFilter(nameof(Filter))]
		public ContentEntry<T> baseEntry;

		private IEnumerable<Type> Filter()
		{
			yield return typeof(TSub);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(ContentEntry<T, TSub> entry) => entry.baseEntry;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContentReference<T>(ContentEntry<T, TSub> entry) => entry.baseEntry;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContentEntry<T>(ContentEntry<T, TSub> entry) => entry.baseEntry;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator bool(ContentEntry<T, TSub> entry) => entry.baseEntry;
	}
}
