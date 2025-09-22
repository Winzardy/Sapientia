using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Content
{
	/// <summary>
	/// Не использовать в обычных полях! (нет поддержки и надобность поддержки под вопросом)
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Serializable]
	public partial class SingleContentEntry<T> : BaseContentEntry<T>, ISingleContentEntry<T>
	{
		protected SingleContentEntry(in T value) : base(in value)
		{
		}

		public sealed override bool IsUnique() => false;

		protected override void OnRegister() => ContentManager.Register(this);
		protected override void OnUnregister() => ContentManager.Unregister<T>();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(SingleContentEntry<T> entry) => entry.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContentReference(SingleContentEntry<T> _) => new(in IContentReference.SINGLE_GUID);
	}

	public interface ISingleContentEntry<T> : IContentEntry<T>, ISingleContentEntry
	{
	}

	public partial interface ISingleContentEntry : IContentEntry
	{
	}
}
