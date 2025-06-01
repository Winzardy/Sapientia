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

		public static Exception NullException(object msg) =>
			logger?.NullReferenceException(msg) ?? new NullReferenceException(msg.ToString());

		public static Exception Exception(object msg) => logger?.Exception(msg) ?? new Exception(msg.ToString());

#if CLIENT
		public static UnityEngine.Color COLOR = new(1f, 0.5f, 0.5f);
#endif

		// ReSharper disable once FieldCanBeMadeReadOnly.Global
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
