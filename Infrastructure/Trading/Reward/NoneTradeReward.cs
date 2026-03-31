using System;

namespace Trading
{
	[Serializable]
	public partial class NoneTradeReward : TradeReward
	{
		protected override bool Receive(Tradeboard _) => true;
	}
}
