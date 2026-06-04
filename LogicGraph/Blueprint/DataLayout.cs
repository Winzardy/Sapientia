using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Пять областей данных ноды/блюпринта (см. CLAUDE.md §4). Числовое значение = индекс слота
	/// в fixed-буферах <see cref="DataSizes"/>/<see cref="NodeLayoutOffsets"/> — порядок менять нельзя.
	/// </summary>
	public enum DataLayout : byte
	{
		/// <summary>
		/// Неизменяемые общие данные, дедуп по (id, version) — живут в скомпилированном блобе.
		/// </summary>
		Static = 0,
		/// <summary>
		/// Рабочий кеш области (scope), обнуляется каждый run.
		/// </summary>
		StaticCache = 1,
		/// <summary>
		/// Долгоживущие данные области (scope), не обнуляются.
		/// </summary>
		StaticPersistent = 2,
		/// <summary>
		/// Кеш инстанса, обнуляется каждый run.
		/// </summary>
		InstanceCache = 3,
		/// <summary>
		/// Долгоживущие данные инстанса, не обнуляются.
		/// </summary>
		InstancePersistent = 4,
	}

	/// <summary>
	/// Байтовые размеры всех 5 областей одной ноды, объявляемые на этапе компиляции (см.
	/// <see cref="INode.DataSizes"/>). Индексируется по <see cref="DataLayout"/>. Любой размер может быть 0
	/// — нода вправе не занимать какую-либо область.
	/// </summary>
	public unsafe struct DataSizes
	{
		public const int Count = 5;
		public const int Alignment = 8;

		private fixed int _sizes[Count];

		public DataSizes(int staticSize, int staticCacheSize, int staticPersistentSize, int instanceCacheSize, int instancePersistentSize)
		{
			_sizes[DataLayout.Static.ToInt()] = staticSize;
			_sizes[DataLayout.StaticCache.ToInt()] = staticCacheSize;
			_sizes[DataLayout.StaticPersistent.ToInt()] = staticPersistentSize;
			_sizes[DataLayout.InstanceCache.ToInt()] = instanceCacheSize;
			_sizes[DataLayout.InstancePersistent.ToInt()] = instancePersistentSize;
		}

		public int this[DataLayout layout]
		{
			get => _sizes[layout.ToInt()];
			set => _sizes[layout.ToInt()] = value;
		}

		public int this[int layout]
		{
			get => _sizes[layout];
			set => _sizes[layout] = value;
		}

		public int GetAligned(DataLayout layout)
		{
			return GetAligned(layout.ToInt());
		}

		public int GetAligned(int layout)
		{
			return _sizes[layout].AlignUp(Alignment);
		}

		/// <summary>Копия с каждым размером, выровненным до <see cref="Alignment"/>.</summary>
		public DataSizes GetAligned()
		{
			var result = this;
			for (var l = 0; l < Count; l++)
				result[l] = result[l].AlignUp(Alignment);
			return result;
		}

		/// <summary>Поэлементная сумма размеров по всем областям.</summary>
		public static DataSizes operator +(DataSizes a, DataSizes b)
		{
			for (var l = 0; l < Count; l++)
				a[l] += b[l];
			return a;
		}
	}

	/// <summary>
	/// Байтовые офсеты слайса одной ноды внутри блока каждой из 5 областей. Заполняются при компиляции
	/// раскладки (<see cref="CompiledBlueprint.SetupLayout"/>) и читаются как «база блока + офсет».
	/// </summary>
	public unsafe struct NodeLayoutOffsets
	{
		private fixed int _offsets[DataSizes.Count];

		public PtrOffset this[DataLayout layout]
		{
			get => new PtrOffset(_offsets[layout.ToInt()]);
			set => _offsets[layout.ToInt()] = value.byteOffset;
		}

		public PtrOffset this[int layout]
		{
			get => new PtrOffset(_offsets[layout]);
			set => _offsets[layout] = value.byteOffset;
		}
	}
}
