using System.Collections.Generic;

namespace Sapientia.Pooling
{
    public class DictionaryPool<T0, T1>
    {
        private static ObjectPool<Dictionary<T0, T1>> _instance = new (new Policy(), true);

        public static Dictionary<T0, T1> Get() => _instance.Get();

        public static PooledObject<Dictionary<T0, T1>> Get(out Dictionary<T0, T1> result) => _instance.Get(out result);

        public static void Release(Dictionary<T0, T1> dict) => _instance.Release(dict);

        private class Policy : DefaultObjectPoolPolicy<Dictionary<T0, T1>>
        {
            public override void OnRelease(Dictionary<T0, T1> dict)
            {
                dict.Clear();
            }
        }
    }
}