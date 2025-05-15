using System.Threading;

namespace Sapientia.Extensions
{
	public static class CancellationTokenSourceUtility
	{
		public static void Trigger(ref CancellationTokenSource cts)
		{
			if (cts == null || cts.IsCancellationRequested)
				return;

			cts.Cancel();
			cts.Dispose();

			cts = null;
		}

		public static bool AnyCancellation(CancellationToken a, CancellationToken b)
			=> a.IsCancellationRequested || b.IsCancellationRequested;

		public static bool AnyCancellation(CancellationToken a, CancellationToken b, CancellationToken c)
			=> a.IsCancellationRequested || b.IsCancellationRequested || c.IsCancellationRequested;

		public static bool AnyCancellation(params CancellationToken[] tokens)
		{
			for (int i = 0; i < tokens.Length; i++)
			{
				if (tokens[i].IsCancellationRequested)
					return true;
			}

			return false;
		}

		public static void Trigger(this CancellationTokenSource cts)
		{
			if (cts == null || cts.IsCancellationRequested)
				return;

			cts.Cancel();
			cts.Dispose();
		}

		public static CancellationTokenSource Link(this CancellationTokenSource cts,
			CancellationToken token)
			=> CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token);

		public static CancellationTokenSource Link(this CancellationTokenSource cts,
			CancellationToken token1,
			CancellationToken token2)
			=> CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token1, token2);

		public static CancellationTokenSource Link(this ref
			CancellationToken a,
			CancellationToken b)
			=> CancellationTokenSource.CreateLinkedTokenSource(a, b);

		public static CancellationTokenSource Link(this ref
			CancellationToken a,
			CancellationToken b,
			CancellationToken c)
			=> CancellationTokenSource.CreateLinkedTokenSource(a, b, c);
	}
}
