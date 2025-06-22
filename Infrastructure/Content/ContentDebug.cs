using System;
using System.Runtime.CompilerServices;
#if CLIENT
using UnityEngine;
#endif

namespace Content
{
	using Logger = Sapientia.ILogger;

	public static class ContentDebug
	{
		public static Logger logger;

#if CLIENT
		[HideInCallstack]
#endif
		public static void Log(object msg, object context = null,
			[CallerMemberName] string memberName = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> logger?.Log(msg, context, memberName, sourceLineNumber);

#if CLIENT
		[HideInCallstack]
#endif
		public static void LogWarning(object msg, object context = null,
			[CallerMemberName] string memberName = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> logger?.LogWarning(msg, context, memberName, sourceLineNumber);

#if CLIENT
		[HideInCallstack]
#endif
		public static void LogError(object msg, object context = null,
			[CallerMemberName] string memberName = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> logger?.LogError(msg, context, memberName, sourceLineNumber);
#if CLIENT
		[HideInCallstack]
#endif
		public static void LogException(Exception exception, object context = null,
			[CallerMemberName] string memberName = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> logger?.LogException(exception, context, memberName, sourceLineNumber);

		public static Exception NullException(object msg)
			=> logger?.NullReferenceException(msg) ?? new NullReferenceException(msg.ToString());

		public static Exception Exception(object msg) => logger?.Exception(msg) ?? new Exception(msg.ToString());

#if CLIENT
		public static Color COLOR = new(1f, 0.5f, 0.5f);
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
