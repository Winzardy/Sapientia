using System;
using Sapientia;
#if CLIENT
using UnityEngine;
#endif

namespace Trading
{
	/// <summary>
	/// Сделка, рецепт, обмен <b>чего-то</b> (<see cref="TradeCost"/>) на <b>что-то</b> (<see cref="TradeReward"/>)
	/// </summary>
	[Serializable]
	public class TradeConfig : IExternallyIdentifiable
	{
		private string _id;

		[SerializeReference]
		public TradeReward reward;

		[SerializeReference]
		[TradeAccess(TradeAccessType.High)]
		public TradeCost cost;

		public string Id => _id;

		void IExternallyIdentifiable.SetId(string id) => _id = id;

		public TradeConfig(string id, TradeReward reward, TradeCost cost)
		{
			_id = id;
			this.reward = reward;
			this.cost = cost;
		}
	}
}
