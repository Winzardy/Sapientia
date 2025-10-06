using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Memory;

namespace Sapientia.Memory
{
	public unsafe struct StreamBufferReader
	{
		private readonly SafePtr _arr;
		private readonly int _arrSize;
		private int _position;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public StreamBufferReader(byte[] bytes)
		{
			this = default;
			var size = bytes.Length;
			_arr = MemoryExt.MakeArray<byte>(size);
			fixed (byte* ptrRaw = &bytes[0])
			{
				var ptr = new SafePtr(ptrRaw, size);
				MemoryExt.MemCopy(ptr, _arr, size);
			}

			_arrSize = size;
			_position = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			if (_arr.IsValid)
				MemoryExt.MemFree(_arr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReadBlittable<T>(ref T value, int size) where T : unmanaged
		{
			var ptr = GetPointerAndMove(size);
			value = ptr.Value<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T ReadBlittable<T>(int size) where T : unmanaged
		{
			var ptr = GetPointerAndMove(size);
			return ref ptr.Value<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T ReadBlittable<T>() where T : unmanaged
		{
			var ptr = GetPointerAndMove(TSize<T>.size);
			return ref ptr.Value<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetPointer() => _arr + _position;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetPointerAndMove(int size)
		{
			if (_position + size > _arrSize)
				throw new System.Exception();

			var pos = _position;
			_position += size;
			return _arr + pos;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Read(ref SafePtr value, int length)
		{
			var ptr = GetPointerAndMove(length);
			MemoryExt.MemCopy(ptr, value, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Read<T>(ref SafePtr<T> value, int length) where T : unmanaged
		{
			var size = TSize<T>.size * length;
			var ptr = GetPointerAndMove(size);
			MemoryExt.MemCopy(ptr, (SafePtr)value, size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Read<T>(ref T value) where T : unmanaged
		{
			ReadBlittable(ref value, TSize<T>.size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Read<T>() where T : unmanaged
		{
			return ref ReadBlittable<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Read(ref int value)
		{
			const int size = 4;
			ReadBlittable(ref value, size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Read(ref long value)
		{
			const int size = 8;
			ReadBlittable(ref value, size);
		}
	}
}
