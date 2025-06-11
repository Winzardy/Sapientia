using System;
using System.Runtime.CompilerServices;

namespace Content
{
	public partial struct ContentReference<T>
	{
		#region Equals

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(in ContentReference<T> x, in ContentReference<T> y) => x.guid == y.guid;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(in ContentReference<T> x, in ContentReference<T> y) => !(x == y);

		#endregion

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator bool(in ContentReference<T> reference) => reference.Contains();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(in ContentReference<T> reference) => reference.Read();
		#region Guid

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Guid(in ContentReference<T> reference) => reference.guid;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SerializableGuid(in ContentReference<T> reference) => reference.guid;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContentReference<T>(in SerializableGuid guid) => new(in guid);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContentReference<T>(in Guid guid) => new() {guid = guid};

		#endregion

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContentReference<T>(string str) => ContentReferenceUtility.ToReference<T>(str);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContentReference(in ContentReference<T> reference) => new(in reference.guid, reference.index);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContentReference<T>(in ContentReference reference) => new(in reference.guid, reference.index);

		#region ContentEntry

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContentReference<T>(UniqueContentEntry<T> entry) => new(in entry.Guid, entry.Index);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContentReference<T>(SingleContentEntry<T> _) => new(in IContentReference.SINGLE_GUID);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContentReference<T>(ContentEntry<T> entry) => new(in entry.Guid, entry.Index);

		#endregion
	}

	public partial struct ContentReference
	{
		#region Equals

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(in ContentReference x, in ContentReference y) => x.guid == y.guid;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(in ContentReference x, in ContentReference y) => !(x == y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(in ContentReference x, in IContentReference y) => x.guid == y!.Guid;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(in ContentReference x, in IContentReference y) => !(x == y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(in IContentReference x, in ContentReference y) => x!.Guid == y.guid;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(in IContentReference x, in ContentReference y) => !(x == y);

		#endregion

		#region Guid

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContentReference(in SerializableGuid guid) => new(in guid);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContentReference(in Guid guid) => new() {guid = guid};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Guid(in ContentReference reference) => reference.guid;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SerializableGuid(in ContentReference reference) => reference.guid;

		#endregion

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator string(in ContentReference reference) => reference.ToString();
	}
}
