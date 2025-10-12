namespace Sapientia
{
	/// <summary>
	/// An interop friendly implementation.
	/// Might be temporary, since scenarios system is in the works.
	/// </summary>
	public interface IAction
	{
		public const float R = 1f;
		public const float G = 0.55f;
		public const float B = 1f;
		public const float A = 1;

		public void Execute(IObjectsProvider provider);
	}
}
