using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Sapientia.Utility
{
	/// <summary>
	/// Используется, чтобы разорвать синхронный поток исполнения на несколько кадров, например, чтобы обновить UI
	/// </summary>
	public interface IAsyncFlowController
	{
		Task NextIterationAsync(CancellationToken token = default);
	}

	/// <remarks>
	/// При использовании в асинхронном коде время может замеряться не верно, т.к. не учитывается время последнего апдейта, а только время последнего вызова NextIterationAsync
	/// </remarks>
	public abstract class AsyncFlowControllerBase : IAsyncFlowController
	{
		private long _frameStart;
		private readonly long _ticksBudgetPerFrame;

		protected AsyncFlowControllerBase(int intervalMs)
		{
			_ticksBudgetPerFrame = (long)(intervalMs * Stopwatch.Frequency / 1000.0);
		}

		public async Task NextIterationAsync(CancellationToken token = default)
		{
			if (Stopwatch.GetTimestamp() - _frameStart >= _ticksBudgetPerFrame)
			{
				await OnInterationAsync(token);
				_frameStart = Stopwatch.GetTimestamp();
			}
		}

		protected abstract Task OnInterationAsync(CancellationToken token);
	}
}
