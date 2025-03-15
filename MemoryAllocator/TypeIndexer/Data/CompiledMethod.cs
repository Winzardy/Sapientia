//#define BURST
#if UNITY_5_4_OR_NEWER
using UnityEngine.Scripting;
#if BURST
using System.Reflection;
#endif
#endif
using System;
using System.Runtime.InteropServices;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.TypeIndexer
{
	public unsafe struct CompiledMethod : IDisposable
	{
		public GCHandle handle;
		public IntPtr functionPointer;

		[INLINE(256)]
#if UNITY_5_4_OR_NEWER
		[Preserve]
#endif
		public static CompiledMethod Create<TDelegate>(TDelegate call) where TDelegate : Delegate
		{
			var handle = GCHandle.Alloc(call);
			return new CompiledMethod
			{
				handle = handle,
				functionPointer = Marshal.GetFunctionPointerForDelegate<TDelegate>(call),
			};
		}

#if BURST
		[INLINE(256)]
#if UNITY_5_4_OR_NEWER
		[Preserve]
#endif
		public static CompiledMethod CreateBurst<TDelegate>(TDelegate call) where TDelegate : System.Delegate
		{
			return new CompiledMethod
			{
				handle = default,
				functionPointer = Unity.Burst.BurstCompiler.CompileFunctionPointer(call).Value,
			};
		}
#endif

		public void Dispose()
		{
			if (handle.IsAllocated)
				handle.Free();
			handle = default;
			functionPointer = default;
		}
	}
}
