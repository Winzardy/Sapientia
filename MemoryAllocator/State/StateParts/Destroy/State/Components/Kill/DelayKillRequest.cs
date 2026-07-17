namespace Sapientia.MemoryAllocator.State
{
	// Флат-компонент (float delay, без Entity-полей) - копируется значением без атрибута, как
	// AliveDuration/AliveTimeDebt. Без копии таймер терялся бы, сущность стала бы бессмертной в новом мире.
	public struct DelayKillRequest : IComponent
	{
		public float delay;
	}
}
