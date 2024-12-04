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
		private static T _instance;

		public static bool IsInitialized => _instance != null;

		protected static T Instance
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if (_instance == null)
					throw new Exception($"Trying to access [ {typeof(T).Name} ] functions while its not initialized!");

				return _instance;
			}
		}

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
