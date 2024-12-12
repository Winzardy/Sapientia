using System;
using System.Runtime.CompilerServices;

namespace Sapientia.Extensions
{
	/// <summary>
	/// Use it to implement static wrapper for features.
	/// Check each method invocation with InitializationCheck to lot invalid use.
	/// Should only be one wrapper per type of T.
	/// </summary>
	public abstract class StaticWrapper<T> where T : class
	{
		protected static T _instance;

		public static bool IsInitialized => _instance != null;

		public static void Initialize(T instance)
		{
			Terminate();

			_instance = instance;
		}

		public static void Terminate()
		{
			switch (_instance)
			{
				case null:
					return;
				case IDisposable disposable:
					disposable.Dispose();
					break;
			}

			_instance = null;
		}
	}
}
