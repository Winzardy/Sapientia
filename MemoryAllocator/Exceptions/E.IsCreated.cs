namespace Sapientia.MemoryAllocator
{
	using System.Diagnostics;
	using BURST_DISCARD = Unity.Burst.BurstDiscardAttribute;
	using HIDE_CALLSTACK = UnityEngine.HideInCallstackAttribute;

	public partial class E
	{
		public unsafe class NotCreatedException : System.Exception
		{
			public NotCreatedException(string str) : base(str)
			{
			}

			[HIDE_CALLSTACK]
			public static void Throw<T>(T obj)
			{
				ThrowNotBurst(obj);
				throw new NotCreatedException("Object is not created");
			}

			[HIDE_CALLSTACK]
			public static void Throw<T>(T* obj) where T : unmanaged
			{
				ThrowNotBurst(obj);
				throw new NotCreatedException("Object is not created");
			}

			[BURST_DISCARD]
			[HIDE_CALLSTACK]
			private static void ThrowNotBurst<T>(T obj) =>
				throw new NotCreatedException($"{Exception.Format(typeof(T).Name)} is not created");

			[BURST_DISCARD]
			[HIDE_CALLSTACK]
			private static void ThrowNotBurst<T>(T* obj) where T : unmanaged =>
				throw new NotCreatedException($"{Exception.Format(typeof(T).Name)} is not created");
		}
	}

	public static partial class E
	{
		[Conditional(COND.EXCEPTIONS)]
		[HIDE_CALLSTACK]
		public static void IS_CREATED<T>(T list) where T : unmanaged, IIsCreated
		{
			if (list.IsCreated == true) return;
			NotCreatedException.Throw(list);
		}
	}

	public static partial class E
	{
		[Conditional(COND.EXCEPTIONS)]
		[HIDE_CALLSTACK]
		public static void IS_CREATED<K, V>(EquatableDictionary<K, V> dic)
			where K : unmanaged, System.IEquatable<K> where V : unmanaged
		{
			if (dic.isCreated == true) return;
			NotCreatedException.Throw(dic);
		}
	}

	public static partial class E
	{
		[Conditional(COND.EXCEPTIONS)]
		[HIDE_CALLSTACK]
		public static void IS_CREATED<V>(UIntDictionary<V> dic) where V : unmanaged
		{
			if (dic.isCreated == true) return;
			NotCreatedException.Throw(dic);
		}
	}

	public static partial class E
	{
		[Conditional(COND.EXCEPTIONS)]
		[HIDE_CALLSTACK]
		public static void IS_CREATED<V>(ULongDictionary<V> dic) where V : unmanaged
		{
			if (dic.isCreated == true) return;
			NotCreatedException.Throw(dic);
		}
	}

	public static unsafe partial class E
	{
		[Conditional(COND.EXCEPTIONS)]
		[HIDE_CALLSTACK]
		public static void IS_CREATED(UIntHashSet list)
		{
			if (list.IsCreated == true) return;
			NotCreatedException.Throw(list);
		}

		[Conditional(COND.EXCEPTIONS)]
		[HIDE_CALLSTACK]
		public static void IS_CREATED(UIntPairHashSet list)
		{
			if (list.IsCreated == true) return;
			NotCreatedException.Throw(list);
		}

		[Conditional(COND.EXCEPTIONS)]
		[HIDE_CALLSTACK]
		public static void IS_CREATED(UIntHashSet* list)
		{
			if (list->IsCreated == true) return;
			NotCreatedException.Throw(list);
		}
	}

	public static partial class E
	{
		[Conditional(COND.EXCEPTIONS)]
		[HIDE_CALLSTACK]
		public static void IS_CREATED<T>(List<T> list) where T : unmanaged
		{
			if (list.IsCreated == true) return;
			NotCreatedException.Throw(list);
		}
	}

	public static partial class E
	{
		[Conditional(COND.EXCEPTIONS)]
		[HIDE_CALLSTACK]
		public static void IS_CREATED(UIntListHash list)
		{
			if (list.IsCreated == true) return;
			NotCreatedException.Throw(list);
		}
	}

	public static partial class E
	{
		[Conditional(COND.EXCEPTIONS)]
		[HIDE_CALLSTACK]
		public static void IS_CREATED<T>(MemArray<T> arr) where T : unmanaged
		{
			if (arr.IsCreated == true) return;
			NotCreatedException.Throw(arr);
		}
	}
}
