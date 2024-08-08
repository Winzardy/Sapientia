using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public static class ListPool<T>
	{
		private static readonly ObjectPool<List<T>> _instance = new(new ListPoolPolicy<T>(), true);

		public static List<T> Get() => _instance.Get();

		public static PooledObject<List<T>> Get(out List<T> result) => _instance.Get(out result);

		public static void Release(List<T> list) => _instance.Release(list);

		public static PooledObject<List<T>> GetCopy(IEnumerable<T> source, out List<T> result)
		{
			var disposable = _instance.Get(out result);

			if (source != null)
			{
				result.AddRange(source);
			}

			return disposable;
		}
	}

	public class ListPoolPolicy<T> : DefaultObjectPoolPolicy<List<T>>
	{
		public override void OnRelease(List<T> list)
		{
			list.Clear();
		}
	}
}
