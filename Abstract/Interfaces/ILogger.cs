using System;
using System.Runtime.CompilerServices;

namespace Sapientia
{
	public interface ILogger
	{
		public void Log(object msg, object context = null, [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0);
		public void LogWarning(object msg, object context = null, [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0);
		public void LogError(object msg, object context = null, [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0);
		public void LogException(Exception exception, object context = null, [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0);
		NullReferenceException NullReferenceException(object msg);
		Exception Exception(object msg);
	}
}
