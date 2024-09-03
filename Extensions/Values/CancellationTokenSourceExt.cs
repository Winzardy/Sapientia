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
	}
}
