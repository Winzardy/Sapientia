using Sapientia;

namespace Trading
{
	/// <summary>
	/// Контекст сделки, сюда попадают все объекты участвующие в сделке
	/// </summary>
	/// <remarks>
	///	Наследуется от <see cref="Blackboard"/>, поэтому может участвовать во вложенных взаимодействиях,
	/// например для передачи в RewardBox или еще каких системах
	/// </remarks>
	public sealed class Tradeboard : Blackboard
	{
	}
}
