using System;

namespace Content
{
	/// <summary>
	/// Cсылка (указатель) на элемент массива, массива который из контента!
	/// </summary>
	/// <remarks>
	/// Данное решение пришло с миграцией старого ValueEntry на новый ContentEntry.
	/// В старую реализацию старался сильно не влезать
	/// </remarks>
	public struct ContentReferenceArrayPointer<T> : IEquatable<ContentReferenceArrayPointer<T>>
	{
		private readonly ArrayPointer _pointer;
		private ContentReference<T[]> _arrayReference;

		public readonly ref readonly T Read() => ref _arrayReference.Read()[_pointer];

		public ContentReferenceArrayPointer(in ArrayPointer point, in ContentReference<T[]> arrayReference)
		{
			_pointer = point;
			_arrayReference = arrayReference;
		}

		public bool Equals(ContentReferenceArrayPointer<T> other) =>
			_arrayReference.Equals(other._arrayReference) && _pointer == other._pointer;

		public override bool Equals(object obj) => obj is ContentReferenceArrayPointer<T> other && Equals(other);

		public override int GetHashCode() => HashCode.Combine(_arrayReference, _pointer);

		public static implicit operator T(in ContentReferenceArrayPointer<T> pointer) => pointer.Read();

		public bool IsEmpty() => _pointer.IsEmpty || _arrayReference.IsEmpty();
	}

	/// <summary>
	/// Cсылка (указатель) на элемент массива, массива который из контента!
	/// </summary>
	/// <remarks>
	/// Данное решение пришло с миграцией старого ValueEntry на новый ContentEntry.
	/// В старую реализацию старался сильно не влезать
	/// </remarks>
	public struct ArrayPointer : IEquatable<ArrayPointer>
	{
		private int _ptr;

		public bool IsEmpty => _ptr == 0;

		public static implicit operator ArrayPointer(int index) => new() {_ptr = index + 1};
		public static implicit operator int(ArrayPointer pointer) => pointer._ptr - 1;

		public bool Equals(ArrayPointer other) => _ptr == other._ptr;
		public override bool Equals(object obj) => obj is ArrayPointer other && Equals(other);
		public override int GetHashCode() => _ptr;
	}
}
