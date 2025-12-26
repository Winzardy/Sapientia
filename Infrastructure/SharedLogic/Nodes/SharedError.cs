using System;

namespace SharedLogic
{
	public struct SharedError<T>
	{
		public readonly T context;
		public readonly string message;

		public SharedError(string message, T context)
		{
			this.message = message;
			this.context = context;
		}

		public static implicit operator SharedError<T>((string message, T context) tuple) => new(tuple.message, tuple.context);

		public override string ToString() => message;
		public Exception ToException() => new Exception(message);
	}
}
