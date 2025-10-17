#if CLIENT
using Sapientia;
using Sapientia.Evaluators;
using UnityEngine;
using UnityEngine.Serialization;

namespace Trading.Advertising
{
	// TODO: удалить после 14.11.2025
	public partial class RewardedAdTradeCost : ISerializationCallbackReceiver
	{
		[HideInInspector]
		[SerializeReference]
		[FormerlySerializedAs("count")]
		public Evaluator<Blackboard, int> countLegacy;

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (countLegacy)
			{
				if (countLegacy is IConstantEvaluator<int> l)
				{
					count = l.Value;
				}
				else
				{
					count = countLegacy;
				}

				countLegacy = null;
			}
		}
	}
}
#endif
