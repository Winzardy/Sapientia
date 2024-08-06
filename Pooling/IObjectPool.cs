namespace Sapientia.Pooling
{
    public interface IObjectPool<T>
    {
        T Get();
        void Release(T obj);
    }
}