using System;
using System.Collections.Generic;
using Sapientia;
using Sapientia.Conditions;

#if CLIENT
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;
using UnityEngine;
#endif

namespace Trading
{
	[Serializable]
#if CLIENT
	[MovedFrom(true, "Trading", null, "ConditionalTradeReward")]
#endif
	public partial class IfElseTradeReward : TradeReward
	{
		[SerializeReference]
		public Condition<Blackboard> condition;

		[SerializeReference]
#if CLIENT
		[FormerlySerializedAs("reward")]
#endif
		public TradeReward a;

		[SerializeReference]
		public TradeReward b = new NoneTradeReward();

		protected override bool CanReceive(Tradeboard board, out TradeReceiveError? error)
		{
			if (condition.IsFulfilled(board))
				return a.CanExecute(board, out error);

			return b.CanExecute(board, out error);
		}

		protected override bool Receive(Tradeboard board)
		{
			if (condition.IsFulfilled(board))
				return a.Execute(board);

			return b.Execute(board);
		}

		protected internal override IEnumerable<TradeReward> EnumerateActualInternal(Tradeboard board)
		{
			if (condition.IsFulfilled(board))
			{
				foreach (var reward in a.EnumerateActual(board))
					yield return reward;
			}
			else
			{
				foreach (var reward in b.EnumerateActual(board))
					yield return reward;
			}
		}

		public override IEnumerator<TradeReward> GetEnumerator()
		{
			yield return this;
			yield return a;
			yield return b;
		}
	}
}
