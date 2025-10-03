using System;
using System.Runtime.CompilerServices;
#if CLIENT
using UnityEngine;
#endif

namespace SharedLogic
{
	public static class SLDebug
	{
		public static Sapientia.ILogger logger;

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
		public static UnityEngine.Color COLOR = Color.magenta;
#endif

		public static class Logging
		{
			public static class Command
			{
				public static bool execute = true;
			}
		}
	}
}
