using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Sapientia;
using Sapientia.Extensions;

namespace Content
{
	public partial class UniqueContentEntry<T> : BaseContentEntry<T>, IUniqueContentEntry<T>
	{
		// ReSharper disable once MemberInitializerValueIgnored
		// ReSharper disable once InconsistentNaming
#if CLIENT
		[UnityEngine.SerializeField]
#endif
		protected SerializableGuid guid;

		/// <summary>
		/// Индексация (only runtime)
		/// </summary>
		[NonSerialized]
		private int _index;

		/// <inheritdoc cref="UniqueContentEntry{T}._index"/>
		public int Index => _index;

		public ref readonly SerializableGuid Guid => ref guid;

		private string _id;

		public virtual string Id => _id.IsNullOrEmpty() ? guid.ToString() : _id;

		public sealed override bool IsUnique() => true;

		protected UniqueContentEntry(in T value, in SerializableGuid guid, string id = null) : base(in value)
		{
			this.guid = guid;
			_id = id;
		}

		protected override void OnRegister() => ContentManager.Register(this);

		protected override void OnUnregister() => ContentManager.Unregister(this);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SerializableGuid(UniqueContentEntry<T> entry) => entry.Guid;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContentReference(UniqueContentEntry<T> entry) => new(in entry.Guid, entry.Index);

		public override string ToString() => $"Entry Type: [ {typeof(T).Name} ] Guid: [ {guid} ]";

		void IUniqueContentEntry.SetIndex(int index) => _index = index;
	}

	public interface IUniqueContentEntry<T> : IContentEntry<T>, IUniqueContentEntry
	{
	}

	public partial interface IUniqueContentEntry : IContentEntry, IIdentifiable
	{
		public ref readonly SerializableGuid Guid { get; }

		public int Index { get; }

		internal void SetIndex(int index);
	}
}
