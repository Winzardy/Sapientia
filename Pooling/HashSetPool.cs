using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public class HashSetPool<T> : StaticObjectPool<HashSet<T>>
	{
		static HashSetPool() => Initialize(new (new Policy(),true));

		private class Policy : DefaultObjectPoolPolicy<HashSet<T>>
		{
			public override void OnRelease(HashSet<T> hashSet) => hashSet.Clear();
		}
	}
}
