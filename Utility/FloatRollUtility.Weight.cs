using System;
using System.Collections.Generic;
using Sapientia.Collections;
using Sapientia.Pooling;

namespace Sapientia
{
	public static class FloatRollUtility
	{
		/// <summary>
		/// Возвращает index чтобы не было боксинга
		/// </summary>
		/// <returns>Успешность ролла</returns>
		public static bool Roll<T>(this IList<T> elements, IRandomizer<float> random, out int index)
			where T : IFloatWeightable
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
		/// <summary>
		/// Возвращает index чтобы не было боксинга
		/// </summary>
		/// <returns>Успешность ролла</returns>
		public static bool Roll<T>(this Span<T> elements, IRandomizer<float> random, out int index)
			where T : IFloatWeightable
		{
			index = 0;

			if (elements.IsEmpty)
				return false;

			if (elements.Length == 1)
				return true;

			var totalWeight = elements.TotalWeight();

			if (totalWeight <= 0f)
				return false;

			var rolledWeight = random.Next(0, totalWeight);
			index = elements.GetIndexByWeight(rolledWeight);
			return true;
		}

		public static float TotalWeight<T>(this Span<T> elements) where T : IFloatWeightable
		{
			var sum = 0f;

			foreach (var element in elements)
				sum += element.Weight;

			return sum;
		}

		public static float TotalWeight<T>(this IList<T> elements) where T : IFloatWeightable
		{
			var sum = 0f;

			foreach (var element in elements)
				sum += element.Weight;

			return sum;
		}

		public static int GetIndexByWeight<T>(this IList<T> elements, float weight) where T : IFloatWeightable
		{
			var index = 0;
			var x = 0f;

			foreach (var element in elements)
			{
				x += element.Weight;

				if (weight < x)
					break;

				index++;
			}

			return index;
		}

		public static int GetIndexByWeight<T>(this Span<T> elements, float weight) where T : IFloatWeightable
		{
			var index = 0;
			var x = 0f;

			foreach (var element in elements)
			{
				x += element.Weight;

				if (weight < x)
					break;

				index++;
			}

			return index;
		}

		public static bool Roll<T, TContext>(this IList<T> elements, TContext context, IRandomizer<float> random, out int index)
			where T : IFloatWeightableWithEvaluator<TContext>
		{
			index = 0;

			if (elements.IsNullOrEmpty())
				return false;

			if (elements.Count == 1)
				return true;

			using (ListPool<float>.Get(out var weights))
			{
				elements.Fill(context, weights);

				if (weights.IsNullOrEmpty())
					return false;

				var totalWeight = weights.TotalWeight();

				if (totalWeight <= 0)
					return false;

				var rolledWeight = random.Next(0, totalWeight);
				index = weights.GetIndexByWeight(rolledWeight);
				return true;
			}
		}

		public static void Fill<T, TContext>(this IList<T> elements, TContext context, List<float> fill)
			where T : IFloatWeightableWithEvaluator<TContext>
		{
			for (var i = 0; i < elements.Count; i++)
				fill.Add(elements[i].Weight.Evaluate(context));
		}

		private static float TotalWeight(this IList<float> weights)
		{
			var sum = 0f;

			for (int i = 0; i < weights.Count; i++)
				sum += weights[i];

			return sum;
		}

		private static int GetIndexByWeight(this IList<float> weights, float weight)
		{
			var index = 0;
			var x = 0f;

			for (var i = 0; i < weights.Count; i++)
			{
				x += weights[i];

				if (weight < x)
					break;

				index++;
			}

			return index;
		}
	}
}
