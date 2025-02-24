using System;
using System.Collections.Generic;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IInterfaceProxyTypeProxy : IProxy
	{
		public static readonly ProxyIndex ProxyIndex = 0;
		ProxyIndex IProxy.ProxyIndex
		{
			[System.Runtime.CompilerServices.MethodImplAttribute(256)]
			get => ProxyIndex;
		}

		private DelegateIndex _firstDelegateIndex;
		DelegateIndex IProxy.FirstDelegateIndex
		{
			[System.Runtime.CompilerServices.MethodImplAttribute(256)]
			get => _firstDelegateIndex;
			[System.Runtime.CompilerServices.MethodImplAttribute(256)]
			set => _firstDelegateIndex = value;
		}

	}

	public static unsafe class IInterfaceProxyTypeProxyExt
	{
	}

	public unsafe struct IInterfaceProxyTypeProxy<TSource> where TSource: struct, Sapientia.TypeIndexer.IInterfaceProxyType
	{
	}
}
