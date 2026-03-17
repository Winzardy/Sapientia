using Sapientia.Collections;
using Sapientia.Data;
using System.Runtime.CompilerServices;

namespace Sapientia.ServiceManagement
{
	public static partial class ServiceLocator<TService>
	{
		internal static readonly SimpleList<IContextSubscriber> contextSubscribers = new();

		private static AsyncValue<TService> _instance = new(default);

		public static TService Instance
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _instance.ReadValue();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set => _instance.SetValue(value);
		}

		public static bool HasInstance()
		{
			return Instance != null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsRegistered(TService service)
		{
			return Instance != null && Instance.Equals(service);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService GetOrCreate<T>() where T : TService, new()
		{
			if (Instance == null)
				return Create<T>();
			return Instance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGet(out TService service)
		{
			service = Instance;
			return Instance != null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Create<T>() where T : TService, new()
		{
			var value = new T();
			Register(value);
			return value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRegister(TService service)
		{
			if (Instance != null)
				return false;
			Instance = service;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService Register(TService service)
		{
			E.ASSERT(Instance == null);
			Instance = service;

			return Instance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryUnRegister(TService service)
		{
			if (Instance == null || !Instance.Equals(service))
				return false;
			Instance = default;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TService ReplaceService(TService service)
		{
			var result = Instance;
			Instance = service;
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegister()
		{
			Instance = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UnRegister(TService service)
		{
			if (Instance == null || !Instance.Equals(service))
				return;
			Instance = default;
		}

		public static void RemoveAllContext(bool dispose = false)
		{
			foreach (var subscriber in contextSubscribers)
			{
				subscriber.RemoveAllContext(dispose);
			}
		}
	}
}
