using System.Threading;

namespace Sapientia.Extensions
{
	public static class CancellationTokenSourceExt
	{
		public static void Trigger(this CancellationTokenSource cts)
		{
			if (cts == null)
				return;

			cts.Cancel();
			cts.Dispose();
		}

		public static void Trigger(ref CancellationTokenSource cts)
		{
			if (cts == null)
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
	}
}
