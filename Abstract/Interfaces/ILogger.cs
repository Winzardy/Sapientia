using System;

namespace Sapientia
{
	public interface ILogger
	{
		public void Log(object msg, object context = null);
		public void LogWarning(object msg, object context = null);
		public void LogError(object msg, object context = null);
		public void LogException(Exception exception, object context = null);
		NullReferenceException NullReferenceException(object msg);
		Exception Exception(object msg);
	}
}
