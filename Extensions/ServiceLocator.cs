using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sapientia.Extensions
{
	public static class ServiceLocator<T> where T: IService
	{
		public static T Instance { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T1 Register<T1>() where T1: T, new()
		{
			var value = new T1();
			Register(value);
			return value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Register(T service)
		{
			Debug.Assert(Instance == null);
			Instance = service;

			return Instance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegister()
		{
			Instance = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegister(T service)
		{
			Debug.Assert(Instance.Equals(service));
			Instance = default;
		}
	}

	public static class ServiceLocator
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T RegisterService<T, T1>() where T: IService where T1 : T, new()
		{
			return ServiceLocator<T>.Register<T1>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T RegisterService<T>() where T: IService, new()
		{
			return ServiceLocator<T>.Register<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T RegisterAsService<T>(this T service) where T: IService
		{
			return ServiceLocator<T>.Register(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegisterAsService<T>(this T service) where T: IService
		{
			ServiceLocator<T>.UnRegister(service);
		}
	}

	public interface IService {}
}