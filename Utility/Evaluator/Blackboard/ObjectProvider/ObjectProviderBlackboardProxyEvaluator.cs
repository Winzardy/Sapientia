namespace Sapientia.Evaluators
{
	public abstract class ObjectProviderBlackboardProxyEvaluator<TValue> : ProxyEvaluator<Blackboard, IObjectsProvider, TValue>
	{
		protected override IObjectsProvider Convert(Blackboard board) => board.Get<IObjectsProvider>();
	}
}
