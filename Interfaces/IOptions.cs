using Sapientia.Collections;
using Sapientia.MemoryAllocator;

public interface IOptions<T>
{
	public T[] Options { get; }
	public ref readonly T this[int index] { get; }
	public bool TrySelect(int index);
}
