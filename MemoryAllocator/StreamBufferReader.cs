using Sapientia.Data;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator.Core
{
	public unsafe struct StreamBufferReader
	{
		private readonly SafePtr arr;
		private readonly int arrSize;
		private int position;

		[INLINE(256)]
		public StreamBufferReader(byte[] bytes)
		{
			this = default;
			var size = bytes.Length;
			arr = MemoryExt.MakeArray<byte>(size);
			fixed (byte* ptrRaw = &bytes[0])
			{
				var ptr = new SafePtr(ptrRaw, size);
				MemoryExt.MemCopy(ptr, arr, size);
			}

			arrSize = size;
			position = 0;
		}

		[INLINE(256)]
		public void Dispose()
		{
			if (arr != null)
				MemoryExt.MemFree(arr);
		}

		[INLINE(256)]
		public void ReadBlittable<T>(ref T value, int size) where T : unmanaged
		{
			var ptr = GetPointerAndMove(size);
			value = ptr.Value<T>();
		}

		[INLINE(256)]
		public ref T ReadBlittable<T>(int size) where T : unmanaged
		{
			var ptr = GetPointerAndMove(size);
			return ref ptr.Value<T>();
		}

		[INLINE(256)]
		public ref T ReadBlittable<T>() where T : unmanaged
		{
			var ptr = GetPointerAndMove(TSize<T>.size);
			return ref ptr.Value<T>();
		}

		[INLINE(256)]
		public SafePtr GetPointer() => arr + position;

		[INLINE(256)]
		public SafePtr GetPointerAndMove(int size)
		{
			if (position + size > arrSize)
				throw new System.Exception();

			var pos = position;
			position += size;
			return arr + pos;
		}

		[INLINE(256)]
		public void Read(ref SafePtr value, int length)
		{
			var ptr = GetPointerAndMove(length);
			MemoryExt.MemCopy(ptr, value, length);
		}

		[INLINE(256)]
		public void Read<T>(ref SafePtr<T> value, int length) where T : unmanaged
		{
			var size = TSize<T>.size * length;
			var ptr = GetPointerAndMove(size);
			MemoryExt.MemCopy(ptr, (SafePtr)value, size);
		}

		[INLINE(256)]
		public void Read<T>(ref T value) where T : unmanaged
		{
			ReadBlittable(ref value, TSize<T>.size);
		}

		[INLINE(256)]
		public ref T Read<T>() where T : unmanaged
		{
			return ref ReadBlittable<T>();
		}

		[INLINE(256)]
		public void Read(ref int value)
		{
			const int size = 4;
			ReadBlittable(ref value, size);
		}

		[INLINE(256)]
		public void Read(ref long value)
		{
			const int size = 8;
			ReadBlittable(ref value, size);
		}
	}

	public unsafe struct StreamBufferWriter
	{
		private SafePtr arr;
		private int arrSize;
		private int position;

		[INLINE(256)]
		public StreamBufferWriter(int capacity)
		{
			arr = default;
			arrSize = 0;
			position = 0;

			if (capacity > 0)
			{
				SetCapacity(capacity);
			}
		}

		[INLINE(256)]
		public void Dispose()
		{
			if (arr != null)
				MemoryExt.MemFree(arr);
			this = default;
		}

		[INLINE(256)]
		public void Reset()
		{
			position = 0;
		}

		[INLINE(256)]
		public byte[] ToArray()
		{
			var bytes = new byte[position];
			fixed (byte* ptrRaw = &bytes[0])
			{
				var ptr = new SafePtr(ptrRaw, position);
				MemoryExt.MemCopy(arr, ptr, position);
			}

			return bytes;
		}

		[INLINE(256)]
		private void SetCapacity(int size)
		{
			if (size >= arrSize)
			{
				MemoryExt.ResizeArray(ref arr, ref arrSize, size);
			}
		}

		[INLINE(256)]
		public SafePtr GetPointer() => arr + position;

		[INLINE(256)]
		public SafePtr GetPointerAndMove(int size)
		{
			var pos = position;
			SetCapacity(position + size);
			position += size;

			return arr + pos;
		}

		[INLINE(256)]
		public void WriteBlittable<T>(T value, int size) where T : unmanaged
		{
			var ptr = GetPointerAndMove(size);
			ptr.Value<T>() = value;
		}

		[INLINE(256)]
		public void Write(SafePtr arrBytes, int length)
		{
			var ptr = GetPointerAndMove(length);
			MemoryExt.MemCopy(arrBytes, ptr, length);
		}

		[INLINE(256)]
		public void Write<T>(SafePtr<T> arrBytes, int length) where T : unmanaged
		{
			var size = TSize<T>.size * length;
			var ptr = GetPointerAndMove(size);
			MemoryExt.MemCopy((SafePtr)arrBytes, ptr, size);
		}

		[INLINE(256)]
		public void Write<T>(T value) where T : unmanaged
		{
			WriteBlittable(value, TSize<T>.size);
		}

		[INLINE(256)]
		public void Write(int value)
		{
			const int size = 4;
			WriteBlittable(value, size);
		}

		[INLINE(256)]
		public void Write(long value)
		{
			const int size = 8;
			WriteBlittable(value, size);
		}
	}
}
