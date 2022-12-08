using System.Runtime.CompilerServices;
using UnityEngine;

namespace Sapientia.Extensions
{
	public static class ServiceLocator<T> where T: IService
	{
		public static T Instance { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }

		public static void Register(T service)
		{
			Debug.Assert(Instance == null);
			Instance = service;
		}

		public static void UnRegister(T service)
		{
			Debug.Assert(Instance.Equals(service));
			Instance = default;
		}
	}

	public static class ServiceLocator
	{
		public static void RegisterAsService<T>(this T service) where T: IService
		{
			ServiceLocator<T>.Register(service);
		}

		public static void UnRegisterAsService<T>(this T service) where T: IService
		{
			ServiceLocator<T>.UnRegister(service);
		}
	}

	public interface IService {}
}