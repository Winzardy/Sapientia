namespace Sapientia.MemoryAllocator.State
{
	public struct AliveTimeDebt : IComponent
	{
		/// <summary>
		/// Время, которое нужно отнять от длительности жизни
		/// </summary>
		public OneShotValue<float> timeDebt;
	}
}
