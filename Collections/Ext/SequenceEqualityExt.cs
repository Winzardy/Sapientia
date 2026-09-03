using System.Collections.Generic;

namespace Sapientia.Collections
{
	/// <summary>
	/// Поэлементное сравнение коллекций без аллокаций
	/// </summary>
	/// <remarks>
	/// Нужно там, где структура-аргумент держит ссылку на коллекцию: по умолчанию такие поля
	/// сравниваются по ссылке, а коллекция обычно пересобирается на каждый показ
	/// </remarks>
	public static class SequenceEqualityExt
	{
		public static bool SequenceEquals<T>(this T[] a, T[] b)
		{
			if (ReferenceEquals(a, b))
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			var comparer = EqualityComparer<T>.Default;

			for (var i = 0; i < a.Length; i++)
			{
				if (!comparer.Equals(a[i], b[i]))
					return false;
			}

			return true;
		}

		public static bool SequenceEquals<T>(this List<T> a, List<T> b)
		{
			if (ReferenceEquals(a, b))
				return true;

			if (a == null || b == null)
				return false;

			if (a.Count != b.Count)
				return false;

			var comparer = EqualityComparer<T>.Default;

			for (var i = 0; i < a.Count; i++)
			{
				if (!comparer.Equals(a[i], b[i]))
					return false;
			}

			return true;
		}

		public static bool SequenceEquals<T>(this SimpleList<T> a, SimpleList<T> b)
		{
			if (ReferenceEquals(a, b))
				return true;

			if (a == null || b == null)
				return false;

			if (a.Count != b.Count)
				return false;

			var comparer = EqualityComparer<T>.Default;

			for (var i = 0; i < a.Count; i++)
			{
				if (!comparer.Equals(a[i], b[i]))
					return false;
			}

			return true;
		}
	}
}
