namespace Sapientia.Pooling
{
    public interface IObjectPoolPolicy<T>
    {
        public T Create();
        public void OnGet(T obj);
        public void OnRelease(T obj);
        public void OnDispose(T obj);
    }
}