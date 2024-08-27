using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public class ListPool<T> : StaticObjectPool<List<T>>
	{
		static ListPool() => Initialize(new(new Policy(), true));

		public static PooledObject<List<T>> GetCopy(IEnumerable<T> source, out List<T> result)
		{
			var disposable = instance.Get(out result);

			if (source != null)
			{
				result.AddRange(source);
			}

			return disposable;
		}

		private class Policy : DefaultObjectPoolPolicy<List<T>>
		{
			public override void OnRelease(List<T> list) => list.Clear();
		}
	}
}
