using System;
using System.Collections.Generic;
using Sapientia.Pooling;
using UnityEngine;

namespace Sapientia
{
	public enum WeightRollManyMode
	{
		Default,

		[Tooltip("Выдаёт награды без повторений, если запрошенное количество меньше или равно размеру списка")]
		Sequence
	}

	public static class WeightRollManyUtility
	{
		/// <returns>Возвращает индексы из основной коллекции</returns>
		public static IEnumerable<int> RollMany<T, TContext>(this IList<T> elements,
			TContext context,
			WeightRollManyMode mode,
			int count,
			IRandomizer<int> randomizer)
			where T : IWeightable<TContext>
		{
			switch (mode)
			{
				case WeightRollManyMode.Default:
					for (int i = 0; i < count; i++)
					{
						if (elements.Roll(context, randomizer, out var pickedIndex))
							yield return pickedIndex;
					}

					break;
				case WeightRollManyMode.Sequence:
					using (ListPool<T>.Get(out var list))
					using (ListPool<int>.Get(out var indexes))
					{
						Fill();

						for (int i = 0; i < count; i++)
						{
							if (list.Roll(context, randomizer, out var pickedIndex))
							{
								yield return indexes[pickedIndex];
								list.RemoveAt(pickedIndex);
								indexes.RemoveAt(pickedIndex);
							}

							if (list.Count == 0)
								Fill();
						}

						void Fill()
						{
							list.Clear();
							indexes.Clear();

							list.AddRange(elements);
							for (int i = 0; i < elements.Count; i++)
								indexes.Add(i);
						}
					}

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <returns>Возвращает индексы из основной коллекции</returns>
		public static IEnumerable<int> RollMany<T>(this IList<T> elements,
			WeightRollManyMode mode,
			int count,
			IRandomizer<int> randomizer
		)
			where T : IWeightable
		{
			switch (mode)
			{
				case WeightRollManyMode.Default:

					for (int i = 0; i < count; i++)
					{
						if (elements.Roll(randomizer, out var pickedIndex))
							yield return pickedIndex;
					}

					break;
				case WeightRollManyMode.Sequence:
					using (ListPool<T>.Get(out var list))
					using (ListPool<int>.Get(out var indexes))
					{
						Fill();

						for (int i = 0; i < count; i++)
						{
							if (list.Roll(randomizer, out var pickedIndex))
							{
								yield return indexes[pickedIndex];
								list.RemoveAt(pickedIndex);
								indexes.RemoveAt(pickedIndex);
							}

							if (list.Count == 0)
								Fill();
						}

						void Fill()
						{
							list.Clear();
							indexes.Clear();

							list.AddRange(elements);
							for (int i = 0; i < elements.Count; i++)
								indexes.Add(i);
						}
					}

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
