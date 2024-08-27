using System.Collections.Generic;

namespace Sapientia.Pooling
{
	public class DictionaryPool<T0, T1> : StaticObjectPool<Dictionary<T0, T1>>
	{
		static DictionaryPool() => Initialize(new(new Policy(), true));

		private class Policy : DefaultObjectPoolPolicy<Dictionary<T0, T1>>
		{
			public override void OnRelease(Dictionary<T0, T1> dict) => dict.Clear();
		}
	}
}
