using System;
using System.Runtime.CompilerServices;

namespace Content
{
	/// <summary>
	/// Запись контента, которая оборачивает данные в контейнер с Guid, что делает его уникальным
	/// <br/>
	/// <para>Unity: Применяется в качестве вложенного элемента в других ContentEntry (например, ScriptableContentEntry)</para>
	/// <para>Вне Unity: Основной контейнер данных</para>
	/// </summary>
	/// <typeparam name="T">Тип данных</typeparam>
	/// <remarks>
	/// Контент — это статичные данные, которые не изменяются.<br/>
	/// Важно отметить, что полиморфизм не поддерживается системой напрямую, так как это может быть неэффективным и
	/// требовать значительных ресурсов, хотя решение этого вопроса возможно
	/// </remarks>
	[Serializable]
	public sealed partial class ContentEntry<T> : UniqueContentEntry<T>
	{
#if !CLIENT
		[NonSerialized]
		private int _index = ContentConstants.INVALID_INDEX;
		public ref readonly T Value => ref guid.Get<T>(ref _index);
#endif

		public ContentEntry(in T value) : base(in value, SerializableGuid.New())
		{
		}
	}

	/// <inheritdoc cref="ContentEntry{T}"/>
	/// <typeparam name="TFilter">Тип по которому ограничивает запись (ограничение в основном редакторское)</typeparam>
	[Serializable]
	public partial struct ContentEntry<T, TFilter>
		where T : class
		where TFilter : class, T
	{
		public ContentEntry<T> entry;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(ContentEntry<T, TFilter> entry) => entry.entry;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContentReference<T>(ContentEntry<T, TFilter> entry) => entry.entry;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContentEntry<T>(ContentEntry<T, TFilter> entry) => entry.entry;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator bool(ContentEntry<T, TFilter> entry) => entry.entry;
	}
}
