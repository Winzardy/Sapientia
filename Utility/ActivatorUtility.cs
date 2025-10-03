using System;

namespace Sapientia.Reflection
{
	public static class ActivatorUtility
	{
		public static bool TryCreateInstance<T>(this Type type, out T instance)
		{
			try
			{
				instance = type.CreateInstance<T>();
				return true;
			}
			catch (Exception e)
			{
#if UNITY_EDITOR
				UnityEngine.Debug.LogError(e.Message);
#endif
				instance = default;
				return false;
			}
		}

		public static T CreateInstance<T>(this Type type, params object[] args)
		{
			return (T) Activator.CreateInstance(type, args);
		}

		public static T CreateInstance<T>(this Type type)
			where T : class
		{
			return (T) Activator.CreateInstance(type);
		}
	}
}
