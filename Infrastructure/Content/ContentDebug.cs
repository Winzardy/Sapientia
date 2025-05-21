using System;
using Sapientia;

namespace Content
{
	public static class ContentDebug
	{
		public static ILogger logger;

#if CLIENT
		[UnityEngine.HideInCallstack]
#endif
		public static void Log(string msg, object context = null) => logger?.Log(msg, context);

#if CLIENT
		[UnityEngine.HideInCallstack]
#endif
		public static void LogWarning(string msg, object context = null) => logger?.LogWarning(msg, context);

#if CLIENT
		[UnityEngine.HideInCallstack]
#endif
		public static void LogError(string msg, object context = null) => logger?.LogError(msg, context);
#if CLIENT
		[UnityEngine.HideInCallstack]
#endif
		public static void LogException(Exception exception, object context = null) => logger?.LogException(exception, context);

		public static Exception NullException(string msg) => logger?.NullReferenceException(msg) ?? new NullReferenceException(msg);
		public static Exception Exception(string msg) => logger?.NullReferenceException(msg) ?? new Exception(msg);

#if CLIENT
		public static UnityEngine.Color COLOR = new(1f, 0.5f, 0.5f);
#endif

		public static class Logging
		{
#if UNITY_EDITOR
			public static bool database = true;
#endif
			public static class Nested
			{
#if UNITY_EDITOR
				public static bool refresh = true;
				public static bool regenerate = true;
				public static bool restore = true;
#endif

				public static bool resolve =
#if DebugLog
					true;
#else
					false;
#endif
			}
		}
	}
}
