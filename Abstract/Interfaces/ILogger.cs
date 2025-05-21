using System;

namespace Sapientia
{
	public interface ILogger
	{
		public void Log(string msg, object context = null);
		public void LogWarning(string msg, object context = null);
		public void LogError(string msg, object context = null);
		public void LogException(Exception exception, object context = null);
		NullReferenceException NullReferenceException(string msg);
		Exception Exception(string msg);
	}
}
