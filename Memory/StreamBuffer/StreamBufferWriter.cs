using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Memory;

namespace Sapientia.Memory
{
	public unsafe struct StreamBufferWriter
	{
		private SafePtr _arr;
		private int _arrSize;
		private int _position;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public StreamBufferWriter(int capacity)
		{
			_arr = default;
			_arrSize = 0;
			_position = 0;

			if (capacity > 0)
			{
				SetCapacity(capacity);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			if (_arr.IsValid)
				MemoryExt.MemFree(_arr);
			this = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			_position = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte[] ToArray()
		{
			var bytes = new byte[_position];
			fixed (byte* ptrRaw = &bytes[0])
			{
				var ptr = new SafePtr(ptrRaw, _position);
				MemoryExt.MemCopy(_arr, ptr, _position);
			}

			return bytes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetCapacity(int size)
		{
			if (size >= _arrSize)
			{
				MemoryExt.ResizeArray(ref _arr, ref _arrSize, size);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetPointer() => _arr + _position;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetPointerAndMove(int size)
		{
			var pos = _position;
			SetCapacity(_position + size);
			_position += size;

			return _arr + pos;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteBlittable<T>(T value, int size) where T : unmanaged
		{
			var ptr = GetPointerAndMove(size);
			ptr.Value<T>() = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(SafePtr arrBytes, int length)
		{
			var ptr = GetPointerAndMove(length);
			MemoryExt.MemCopy(arrBytes, ptr, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write<T>(SafePtr<T> arrBytes, int length) where T : unmanaged
		{
			var size = TSize<T>.size * length;
			var ptr = GetPointerAndMove(size);
			MemoryExt.MemCopy((SafePtr)arrBytes, ptr, size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write<T>(T value) where T : unmanaged
		{
			WriteBlittable(value, TSize<T>.size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(int value)
		{
			const int size = 4;
			WriteBlittable(value, size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(long value)
		{
			const int size = 8;
			WriteBlittable(value, size);
		}
	}
}
