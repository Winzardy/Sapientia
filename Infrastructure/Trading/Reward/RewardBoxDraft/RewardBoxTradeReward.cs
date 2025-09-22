// using System;
// using Content;
//
// namespace Trading
// {
// 	// TODO: есть подводные камни, надо подумать
// 	// В голове крутится кейс когда есть ограничение на получение награды, но в большинстве случаев ограничение зависит
// 	// от tradeId (хранится количество по этому ключу) который задается в board, но вдруг мы хотим сделать ограничение относительно TradeReward.
// 	// Пока много вопросов
// 	[Serializable]
// 	public partial class RewardBoxTradeReward : TradeReward
// 	{
// 		public ContentReference<TradeReward> reference;
//
// 		protected override bool CanReceive(Tradeboard board, out TradeReceiveError? error)
// 			=> reference.Read().CanExecute(board, out error);
//
// 		protected override bool Receive(Tradeboard board)
// 			=> reference.Read().Execute(board);
//
// 		protected override bool CanReturn(Tradeboard board, out TradeRewardReturnError? error)
// 			=> reference.Read().CanExecuteReturn(board, out error);
//
// 		protected override bool Return(Tradeboard board)
// 			=> reference.Read().ExecuteReturn(board);
// 	}
// }
