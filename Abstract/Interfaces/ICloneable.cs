namespace Sapientia
{
	public interface ICloneable<out T>
	{
		public T Clone();
	}
}
