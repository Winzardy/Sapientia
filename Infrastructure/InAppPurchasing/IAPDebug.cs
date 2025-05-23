using System;
using Sapientia;

namespace InAppPurchasing
{
	public class IAPDebug
	{
		public static ILogger logger;

#if CLIENT
		[UnityEngine.HideInCallstack]
#endif
		public static void Log(object msg, object context = null) => logger?.Log(msg, context);

#if CLIENT
		[UnityEngine.HideInCallstack]
#endif
		public static void LogWarning(object msg, object context = null) => logger?.LogWarning(msg, context);

#if CLIENT
		[UnityEngine.HideInCallstack]
#endif
		public static void LogError(object msg, object context = null) => logger?.LogError(msg, context);
#if CLIENT
		[UnityEngine.HideInCallstack]
#endif
		public static void LogException(Exception exception, object context = null) => logger?.LogException(exception, context);

		public static Exception NullException(object msg) => logger?.NullReferenceException(msg) ?? new NullReferenceException(msg.ToString());
		public static Exception Exception(object msg) => logger?.Exception(msg) ?? new Exception(msg.ToString());

#if CLIENT
		public static UnityEngine.Color COLOR = UnityEngine.Color.yellow;
#endif
	}
}
