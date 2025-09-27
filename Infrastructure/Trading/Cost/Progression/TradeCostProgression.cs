using System;
using Sapientia;
#if CLIENT
using UnityEngine;
#endif

namespace Trading
{
	public partial class TradeCostProgression : TradeCost
	{
		// ReSharper disable once UseArrayEmptyMethod
		[SerializeReference]
		public TradeCost[] costs = new TradeCost[0];

		public ref readonly TradeCost this[int index] => ref costs[index];

		public ProgressionResetEntry reset;

		protected override bool CanPay(Tradeboard board, out TradePayError? error)
		{
			var cost = Current(board);
			return cost
			   .CanExecute(board, out error);
		}

		protected override bool Pay(Tradeboard board)
		{
			var cost = Current(board);
			var success = cost
			   .Execute(board);

			if (success)
				Next(board);

			return success;
		}

		private TradeCost Current(Tradeboard board)
		{
			var backend = board.Get<ITradingNode>();
			var index = 0;
			//backend.Current(board, this);

			return costs[index];
		}

		private void Next(Tradeboard board)
		{
			var backend = board.Get<ITradingNode>();
			//	backend.Next(board, this);
		}
	}

	[Serializable]
	public struct ProgressionResetEntry
	{
		public ProgressionResetType type;
		public ScheduleEntry schedule;
	}

	public enum ProgressionResetType
	{
		None,

		Full,
		Incremental // В таком кейсе надо как-то уметь высчитывать сколько прошло времени
	}
}
