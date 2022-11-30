using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sapientia.Collections.ByteReader
{
	public class ByteReaderField : IDisposable
	{
		public readonly int poolCountExpandStep;
		public readonly int elementDataCapacity;
		public readonly int poolCapacity;

		private SimpleList<IntPtr> _poolsPointers;
		private SimpleList<ByteReader> _readerField;

		public SimpleList<ByteReader> ReaderField
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _readerField;
		}

		public int PoolCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _poolsPointers.Count;
		}

		public ByteReaderField(int elementDataCapacity, int poolCapacity, int poolCount) : this(elementDataCapacity,
			poolCapacity, poolCount, poolCount)
		{
		}

		public ByteReaderField(int elementDataCapacity, int poolCapacity, int poolCount, int poolCountExpandStep)
		{
			_poolsPointers = new SimpleList<IntPtr>(poolCount);
			_readerField = new SimpleList<ByteReader>(poolCount * poolCapacity);

			this.elementDataCapacity = elementDataCapacity;
			this.poolCapacity = poolCapacity;
			this.poolCountExpandStep = poolCountExpandStep;

			Fill();
		}

		public void Expand()
		{
			var newPoolCount = PoolCount + poolCountExpandStep;

			_poolsPointers.Expand(newPoolCount);
			_readerField.Expand(newPoolCount * poolCapacity);

			Fill();
		}

		private void Fill()
		{
			while (_poolsPointers.Count < _poolsPointers.Capacity)
			{
				_poolsPointers.Add(Marshal.AllocHGlobal(elementDataCapacity * poolCapacity));

				var ptr = _poolsPointers.Last;
				for (var i = 0; i < poolCapacity; i++)
				{
					_readerField.Add(new ByteReader(ptr, elementDataCapacity));
					ptr += elementDataCapacity;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			for (var i = 0; i < _poolsPointers.Count; i++)
			{
				Marshal.FreeHGlobal(_poolsPointers[i]);
			}

			_poolsPointers.Dispose();
			_readerField.Dispose();

			_poolsPointers = default;
			_readerField = default;
		}
	}
}