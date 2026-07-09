namespace Sapientia.MemoryAllocator.State
{
	// Пустой POD, копируется как есть (как DelayKillRequest) - маркер должен доехать с сущностью,
	// иначе смерть не доиграет: DestroyUpdateLogic в новом мире её просто не увидит.
	public struct DestroyRequest : IComponent {}
}
