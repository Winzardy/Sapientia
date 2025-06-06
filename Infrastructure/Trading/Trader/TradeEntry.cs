using System;
using Sapientia;

namespace Trading
{
	// TODO: Нужно будет регистрировать сделки в общей системе если сами сделки динамические

	/// <summary>
	/// Сделка, рецепт, обмен <b>чего-то</b> (<see cref="TradeCost"/>) на <b>что-то</b> (<see cref="TradeReward"/>)
	/// </summary>
	[Serializable]
	public struct TradeEntry : IExternallyIdentifiable
	{
		private string _id;
#if CLIENT
		[UnityEngine.SerializeReference]
#endif
		public TradeReward reward;
#if CLIENT
		[UnityEngine.SerializeReference]
#endif
		public TradeCost cost;

		public string Id => _id;

		void IExternallyIdentifiable.SetId(string id) => _id = id;

		public TradeEntry(string id, TradeReward reward, TradeCost cost)
		{
			_id = id;
			this.reward = reward;
			this.cost = cost;
		}
	}
}
