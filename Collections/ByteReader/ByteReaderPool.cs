using System;
using System.Runtime.CompilerServices;

namespace Sapientia.Collections.ByteReader
{
	public class ByteReaderPool : IDisposable
	{
		private ByteReaderField _pool;
		private int _allocatedCount;

		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _pool.ReaderField.Count;
		}

		public int AllocatedCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _allocatedCount;
		}

		public ByteReaderPool(int elementCapacity, int poolCapacity, int poolCount) : this(elementCapacity,
			poolCapacity, poolCount, poolCount)
		{
		}

		public ByteReaderPool(int elementCapacity, int poolCapacity, int poolCount, int poolCountExpandStep)
		{
			_pool = new ByteReaderField(elementCapacity, poolCapacity, poolCount, poolCountExpandStep);
			_allocatedCount = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Element Allocate()
		{
			return new Element(this, _pool.ReaderField[_allocatedCount++]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Element AllocateWithExpand()
		{
			if (AllocatedCount + 1 > Capacity)
				_pool.Expand();

			return new Element(this, _pool.ReaderField[_allocatedCount++]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Release(ByteReader reader)
		{
			reader.Reset();
			_pool.ReaderField[--_allocatedCount] = reader;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			_pool.Dispose();
		}

		public struct Element : IDisposable
		{
			private ByteReaderPool _pool;
			private ByteReader _reader;

			public ByteReader Reader
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => _reader;
			}

			internal Element(ByteReaderPool pool, ByteReader reader)
			{
				_pool = pool;
				_reader = reader;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose()
			{
				_pool?.Release(_reader);

				_pool = default;
				_reader = default;
			}
		}
	}
}