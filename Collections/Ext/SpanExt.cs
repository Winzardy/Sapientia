using System;
using System.Collections.Generic;

namespace Sapientia.Collections
{
	public static class SpanExt
	{
		public static void RemoveAtSwapBack<T>(this ref Span<T> span, int index)
		{
			(span[index], span[^1]) = (span[^1], span[index]);
			span = span[..^1];
		}

		/// <summary>
		/// Сортировка по возрастанию (От меньшего к большему).
		/// </summary>
		public static void Sort<T>(this Span<T> span, Comparison<T> comparison) where T: unmanaged
		{
			span.Sort(new LambdaComparer<T>(comparison));
		}

		/// <summary>
		/// Сортировка по возрастанию (От меньшего к большему).
		/// </summary>
		public static void Sort<T>(this Span<T> span) where T: unmanaged, IComparable<T>
		{
			span.Sort(0, span.Length, new DefaultComparer<T>());
		}

		/// <summary>
		/// Сортировка по возрастанию (От меньшего к большему).
		/// </summary>
		public static void Sort<T>(this Span<T> span, int index, int count) where T: unmanaged, IComparable<T>
		{
			span.Sort(index, count, new DefaultComparer<T>());
		}

		/// <summary>
		/// Сортировка по возрастанию (От меньшего к большему).
		/// </summary>
		public static void Sort<T, TComparer>(this Span<T> span, TComparer comparer) where TComparer : IComparer<T> where T: unmanaged
		{
			span.Sort(0, span.Length, comparer);
		}

		/// <summary>
		/// Сортировка по возрастанию (От меньшего к большему).
		/// </summary>
		public static unsafe void Sort<T, TComparer>(this Span<T> span, int index, int count, TComparer comparer)
			where TComparer : IComparer<T>
			where T : unmanaged
		{
			if (count <= 1)
				return;

			// Используем алгоритм QuickSort
			// Максимальный размер стека:
			// k = ceil(log2(count)) (для count == 1 → 0)
			// stack length = 2 * (k + 1) // (+1 для запаса/удобства)
			//
			// Если произойдёт выход за пределы массива, то просто увеличить длину до 2 * 64.
			// Если баг сохранится, то вычисляем k и реализуем формулу.
			var stack = (Span<int>)stackalloc int[2 * 32];
			var sp = 0;

			var left = 0;
			var right = count - 1;

			// Работать удобнее на срезе 0..count-1
			span = span.Slice(index, count);

			while (true)
			{
				while (left < right)
				{
					var i = left;
					var j = right;
					var pivot = span[left + ((right - left) >> 1)];

					while (i <= j)
					{
						while (comparer.Compare(span[i], pivot) < 0)
							i++;
						while (comparer.Compare(span[j], pivot) > 0)
							j--;

						if (i > j)
							continue;

						(span[i], span[j]) = (span[j], span[i]);
						i++;
						j--;
					}

					// Теперь два подинтервала: [left..j] и [i..right]
					// Чтобы стек был O(log n): продолжаем на меньшем, больший пушим.
					var leftSize = j - left;
					var rightSize = right - i;

					if (leftSize < rightSize)
					{
						// Правый больше -> в стек, левый обрабатываем дальше
						if (i < right)
						{
							if (sp + 2 > stack.Length)
								throw new InvalidOperationException("Interval stack overflow. Increase stackInts or fix logic.");

							stack[sp++] = i;
							stack[sp++] = right;
						}

						right = j;
					}
					else
					{
						// Левый больше -> в стек, правый обрабатываем дальше
						if (left < j)
						{
							if (sp + 2 > stack.Length)
								throw new InvalidOperationException("Interval stack overflow. Increase stackInts or fix logic.");

							stack[sp++] = left;
							stack[sp++] = j;
						}

						left = i;
					}
				}

				if (sp == 0)
					return;

				right = stack[--sp];
				left = stack[--sp];
			}
		}


		/*public static void Sort<T, TComparer>(this ref Span<T> span, int index, int count, TComparer comparer)
			where TComparer : IComparer<T>
			where T : unmanaged
		{
			if (count <= 1)
				return;

			// Используем алгоритм QuickSort
			// Максимальный размер стека:
			// k = ceil(log2(count)) (для count == 1 → 0)
			// stack length = 2 * (k + 1) // (+1 для запаса/удобства)
			//
			// Если произойдёт выход за пределы массива, то просто увеличить длину до 2 * 64.
			// Если баг сохранится, то
			var stack = (Span<int>)stackalloc int[2 * 32];

			var sp = 0;

			stack[sp++] = index;
			stack[sp++] = index + count - 1;

			while (sp > 0)
			{
				var right = stack[--sp];
				var left = stack[--sp];

				var i = left;
				var j = right;
				var pivot = span[(left + right) / 2];

				while (i <= j)
				{
					while (comparer.Compare(span[i], pivot) < 0)
						i++;
					while (comparer.Compare(span[j], pivot) > 0)
						j--;

					if (i <= j)
					{
						(span[i], span[j]) = (span[j], span[i]);
						i++;
						j--;
					}
				}

				if (left < j)
				{
					stack[sp++] = left;
					stack[sp++] = j;
				}
				if (i < right)
				{
					stack[sp++] = i;
					stack[sp++] = right;
				}
			}
		}*/
	}
}
