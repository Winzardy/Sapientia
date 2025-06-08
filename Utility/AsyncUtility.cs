using System;
using System.Threading.Tasks;

namespace Sapientia.Utility
{
	public static partial class AsyncUtility
	{
		public static void Forget(this Task task, ILogger? logger = null)
		{
			if (task == null)
				throw logger?.Exception(nameof(task)) ?? new ArgumentNullException(nameof(task));

			_ = Run(task);

			async Task Run(Task t)
			{
				try
				{
					await t;
				}
				catch (Exception e)
				{
					logger?.LogException(e);
				}
			}
		}
	}
}
