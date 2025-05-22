using System.Collections.Generic;
using Sapientia.Collections;

namespace Sapientia
{
	public static class RollUtility
	{
		/// <summary>
		/// Возвращает index чтобы не было боксинга
		/// </summary>
		/// <returns>Успешность ролла</returns>
		public static bool Roll<T>(this IList<T> elements, IRandomizer<int> random,  out int index)
			where T : IWeightable
		{
			index = 0;

			if (elements.IsNullOrEmpty())
				return false;

			if (elements.Count == 1)
				return true;

			var totalWeight = elements.TotalWeight();

			if (totalWeight <= 0)
				return false;

			var rolledWeight = random.Next(0, totalWeight);
			index = elements.GetIndexByWeight(rolledWeight);
			return true;
		}

		public static int TotalWeight<T>(this IList<T> elements) where T : IWeightable
		{
			var sum = 0;

			for (int i = 0; i < elements.Count; i++)
				sum += elements[i].Weight;

			return sum;
		}

		public static int GetIndexByWeight<T>(this IList<T> elements, int weight) where T : IWeightable
		{
			var index = 0;
			var x = 0;

			for (int i = 0; i < elements.Count; i++)
			{
				x += elements[i].Weight;

				if (weight < x)
					break;

				index++;
			}

			return index;
		}
	}
}
