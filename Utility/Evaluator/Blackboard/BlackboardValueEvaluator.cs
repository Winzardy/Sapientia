#nullable disable
using System;
using Sapientia.Deterministic;
#if CLIENT
using Sirenix.OdinInspector;
#endif

namespace Sapientia.Evaluators
{
	[Serializable]
	public class BlackboardValueEvaluator<TValue> : Evaluator<Blackboard, TValue>
	{
#if CLIENT
		public const string SELECTOR_NAME = "\u2009Blackboard Value";
		public const string SELECTOR_CATEGORY = "/";
		public const SdfIconType SELECTOR_ICON = SdfIconType.BoxArrowLeft;
#endif

		public BlackboardValueEvaluator()
		{
		}

		public string key;
		protected override TValue OnEvaluate(Blackboard blackboard) => blackboard.Get<TValue>(key);
	}

#if CLIENT
	[TypeRegistryItem(
		SELECTOR_NAME,
		SELECTOR_CATEGORY,
		SELECTOR_ICON,
		darkIconColorR: R, darkIconColorG: G,
		darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G,
		lightIconColorB: B,
		lightIconColorA: A,
		priority: 1
	)]
#endif
	[Serializable]
	public sealed class IntBlackboardValueEvaluator : BlackboardValueEvaluator<int>
	{
	}

#if CLIENT
	[TypeRegistryItem(
		SELECTOR_NAME,
		SELECTOR_CATEGORY,
		SELECTOR_ICON,
		darkIconColorR: R, darkIconColorG: G,
		darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G,
		lightIconColorB: B,
		lightIconColorA: A,
		priority: 1
	)]
#endif
	[Serializable]
	public sealed class Fix64BlackboardValueEvaluator : BlackboardValueEvaluator<Fix64>
	{
	}
}
