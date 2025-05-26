namespace Content
{
	public class SingleContentEntry<T> : BaseContentEntry<T>
	{
		protected SingleContentEntry(in T value) : base(in value)
		{
		}

		public sealed override bool IsUnique() => false;

		protected override void OnRegister() => ContentManager.Register(this);
		protected override void OnUnregister() => ContentManager.Unregister<T>();

		public static implicit operator T(SingleContentEntry<T> entry) => entry.Value;

		public SingleContentEntry<T> Clone() => new(in Value);
	}
}
