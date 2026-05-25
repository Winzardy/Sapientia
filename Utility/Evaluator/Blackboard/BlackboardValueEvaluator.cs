#nullable disable
using System;
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
}
