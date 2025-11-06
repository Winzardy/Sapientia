namespace Sapientia.MemoryAllocator.State
{
	public struct AliveDuration : IComponent
	{
		public float currentDuration;
		public OptionalValue<float> destroyDuration;
	}
}
