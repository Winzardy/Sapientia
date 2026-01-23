#if UNITASK_ENABLED
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Sapientia.Utility
{
	public static partial class AsyncUtility
	{
		/// <summary>
		/// Преобразует вызов метода с колбеком в UniTask<br/>
		/// Использовать: <b>int res = await FromCallback&lt;int&gt;(callback => MethodWithCallback(callback))</b>
		/// </summary>
		public static async UniTask<T> FromCallback<T>(Action<Action<T>> action, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var tcs = new UniTaskCompletionSource<T>();

			await using var _ = cancellationToken.CanBeCanceled
				? cancellationToken.Register(static tcs => ((UniTaskCompletionSource)tcs).TrySetCanceled(), tcs)
				: default;
			try
			{
				action(result => tcs.TrySetResult(result));
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}

			return await tcs.Task;
		}
	}
}
#endif
