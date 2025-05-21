#if CLIENT

namespace Content
{
	public abstract partial class UniqueContentEntry<T>
	{
		public virtual void RegenerateGuid() => SetGuid(SerializableGuid.New());

		public void SetGuid(in SerializableGuid value) => guid = value;
	}

	public partial interface IUniqueContentEntry
	{
		public void RegenerateGuid();
		public void SetGuid(in SerializableGuid value);
	}
}
#endif
