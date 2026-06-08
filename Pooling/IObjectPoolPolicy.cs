namespace Sapientia.Pooling
{
	public interface IObjectPoolPolicy<T>
	{
		T Create();
		void OnGet(T obj);
		void OnRelease(T obj);
		void OnDispose(T obj);
	}
}
