namespace Sapientia.TypeIndexer
{
	public static class TypeIdOf<T>
	{
		public static readonly TypeId typeId;
		public static readonly int Count;

		static TypeIdOf()
		{
			IndexedTypes.GetTypeId(typeof(T), out typeId);
			Count = IndexedTypes.GetContextCount(typeof(T));
		}
	}

	public static class TypeIdOf<TContext, TType>
		where TType : IIndexedType
	{
		public static readonly TypeId<TContext> typeId;

		static TypeIdOf()
		{
			IndexedTypes.GetContextTypeId(typeof(TContext), typeof(TType), out var rawId);
			typeId = (int)rawId;
		}
	}
}
