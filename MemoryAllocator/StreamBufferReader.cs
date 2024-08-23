using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator.Core
{
	public unsafe struct StreamBufferReader
	{
		private readonly byte* arr;
		private readonly uint arrSize;
		private uint position;

		[INLINE(256)]
		public StreamBufferReader(byte[] bytes)
		{
			this = default;
			var size = bytes.Length;
			arr = MemoryExt.MakeArray<byte>(size);
			fixed (byte* ptr = &bytes[0])
			{
				MemoryExt.MemCopy(ptr, arr, size);
			}

			arrSize = (uint)size;
			position = 0;
		}

		[INLINE(256)]
		public void Dispose()
		{
			if (arr != null)
				MemoryExt.MemFree(arr);
		}

		[INLINE(256)]
		public void ReadBlittable<T>(ref T value, uint size) where T : unmanaged
		{
			var ptr = GetPointerAndMove(size);
			value = *(T*)ptr;
		}

		[INLINE(256)]
		public ref T ReadBlittable<T>(uint size) where T : unmanaged
		{
			var ptr = GetPointerAndMove(size);
			return ref *(T*)ptr;
		}

		[INLINE(256)]
		public ref T ReadBlittable<T>() where T : unmanaged
		{
			var ptr = GetPointerAndMove(TSize<T>.uSize);
			return ref *(T*)ptr;
		}

		[INLINE(256)]
		public byte* GetPointer() => arr + position;

		[INLINE(256)]
		public byte* GetPointerAndMove(uint size)
		{
			if (position + size > arrSize)
				throw new System.Exception();

			var pos = position;
			position += size;
			return arr + pos;
		}

		[INLINE(256)]
		public void Read(ref byte* value, uint length)
		{
			var ptr = GetPointerAndMove(length);
			MemoryExt.MemCopy(ptr, value, length);
		}

		[INLINE(256)]
		public void Read<T>(ref T* value, uint length) where T : unmanaged
		{
			var size = TSize<T>.uSize * length;
			var ptr = GetPointerAndMove(size);
			MemoryExt.MemCopy(ptr, (byte*)value, size);
		}

		[INLINE(256)]
		public void Read<T>(ref T value) where T : unmanaged
		{
			ReadBlittable(ref value, TSize<T>.uSize);
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
		private byte* arr;
		private uint arrSize;
		private uint position;

		[INLINE(256)]
		public StreamBufferWriter(uint capacity)
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
			fixed (byte* ptr = &bytes[0])
			{
				MemoryExt.MemCopy(arr, ptr, position);
			}

			return bytes;
		}

		[INLINE(256)]
		private void SetCapacity(uint size)
		{
			if (size >= arrSize)
			{
				MemoryExt.ResizeArray(ref arr, ref arrSize, size);
			}
		}

		[INLINE(256)]
		public byte* GetPointer() => arr + position;

		[INLINE(256)]
		public byte* GetPointerAndMove(uint size)
		{
			var pos = position;
			SetCapacity(position + size);
			position += size;

			return arr + pos;
		}

		[INLINE(256)]
		public void WriteBlittable<T>(T value, uint size) where T : unmanaged
		{
			var ptr = GetPointerAndMove(size);
			*(T*)ptr = value;
		}

		[INLINE(256)]
		public void Write(byte* arrBytes, uint length)
		{
			var ptr = GetPointerAndMove(length);
			MemoryExt.MemCopy(arrBytes, ptr, length);
		}

		[INLINE(256)]
		public void Write<T>(T* arrBytes, uint length) where T : unmanaged
		{
			var size = TSize<T>.uSize * length;
			var ptr = GetPointerAndMove(size);
			MemoryExt.MemCopy((byte*)arrBytes, ptr, size);
		}

		[INLINE(256)]
		public void Write<T>(T value) where T : unmanaged
		{
			WriteBlittable(value, TSize<T>.uSize);
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
