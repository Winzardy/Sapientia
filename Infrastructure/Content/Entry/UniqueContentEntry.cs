using System;

namespace Content
{
	public partial class UniqueContentEntry<T> : BaseContentEntry<T>, IUniqueContentEntry<T>
	{
		// ReSharper disable once MemberInitializerValueIgnored
		// ReSharper disable once InconsistentNaming
#if CLIENT
		[UnityEngine.SerializeField]
#endif
		protected SerializableGuid guid = SerializableGuid.New();

		/// <summary>
		/// Индексация (only runtime)
		/// </summary>
		[NonSerialized]
		private int _index;

		/// <inheritdoc cref="UniqueContentEntry{T}._index"/>
		public int Index => _index;

		public ref readonly SerializableGuid Guid => ref guid;

		public sealed override bool IsUnique() => true;
		public virtual string Id => Guid.ToString();

		protected UniqueContentEntry(in T value, in SerializableGuid guid) : base(in value)
		{
			this.guid = guid;
		}

		protected override void OnRegister() => ContentManager.Register(this);

		protected override void OnUnregister() => ContentManager.Unregister(this);

		public static implicit operator SerializableGuid(UniqueContentEntry<T> entry) => entry.Guid;
		public static implicit operator ContentReference(UniqueContentEntry<T> entry) => new(in entry.Guid, entry.Index);
		public static implicit operator ContentReference<T>(UniqueContentEntry<T> entry) => new(in entry.Guid, entry.Index);

		public override string ToString() => $"Entry Type: [ {typeof(T).Name} ] Guid: [ {guid} ]";

		void IUniqueContentEntry.SetIndex(int index) => _index = index;
	}

	public interface IUniqueContentEntry<T> : IContentEntry<T>, IUniqueContentEntry
	{
	}

	public partial interface IUniqueContentEntry : IContentEntry
	{
		public ref readonly SerializableGuid Guid { get; }

		public int Index { get; }

		internal void SetIndex(int index);
	}
}
