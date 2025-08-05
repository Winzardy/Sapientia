using System;
using System.Collections;
using System.Collections.Generic;
using Sapientia.Extensions;

namespace Sapientia.Reflection
{
	[Serializable]
	public class MemberReflectionReference<T> : IMemberReflectionReference
	{
		private T _cache;

		public MemberReferencePathStep[] steps;

		public Type MemberType => typeof(T);

		public MemberReferencePathStep[] Steps => steps;

		public string Path => ToString();

		/// <summary>
		/// Получить значение
		/// </summary>
		/// <param name="obj">Начальный объект, от которого последовательно разрешаются все шаги пути</param>
		/// <param name="cached">Нужно ли кэшировать, важно отметить что кеширует только если получилось достать значение</param>
		public T Resolve(object obj, bool cached = false)
		{
			var value = Resolve(obj, out var exception, cached);

			if (exception != null)
				throw exception;

			return value;
		}

		/// <summary>
		/// Безопасно получить значение
		/// </summary>
		/// <param name="obj">Начальный объект, от которого последовательно разрешаются все шаги пути</param>
		/// <param name="cached">Нужно ли кэшировать, важно отметить что кеширует только если получили значение</param>
		/// <returns>Может вернуть "default"</returns>
		public T ResolveSafe(object obj, bool cached = false) => Resolve(obj, out _, cached);

		private T Resolve(object obj, out Exception exception, bool cached = false)
		{
			exception = null;

			if (_cache != null)
				return _cache;

			for (int i = 0; i < steps.Length; i++)
			{
				var step = steps[i];

				if (obj == null)
				{
					exception = new NullReferenceException($"Object became null at step [ {i + 1} ]" +
						$" with name [ {step.name} ], path: {Path}");
					return default;
				}

				if (step.IsArrayElement)
				{
					obj = obj.GetValueByReflectionSafe(step.name);

					if (obj is IList list)
					{
						if (step.ArrayElementIndex < 0 || step.ArrayElementIndex >= list.Count)
						{
							exception = new IndexOutOfRangeException(
								$"Array index [ {step.ArrayElementIndex} ] out of bounds in list [ {step.name} ], path: {Path}");
							return default;
						}

						obj = list[step.ArrayElementIndex];
					}
					else
					{
						exception = new ArgumentException(
							$"Step '{step.name}' is marked as array element, but resolved object is not IList (actual: {obj?.GetType().Name ?? "null"}), path: {Path}");
						return default;
					}
				}
				else if (step.IsDictionaryElement)
				{
					obj = obj.GetValueByReflectionSafe(step.name);

					if (obj is IDictionary dictionary)
					{
						var keyType = obj.GetType().GetGenericArguments()[0];
						object key;
						if (keyType.IsEnum)
							key = Enum.Parse(keyType, step.key, ignoreCase: true);
						else if (keyType == typeof(int))
							key = int.Parse(step.key);
						else
							key = step.key;

						if (!dictionary.Contains(key))
						{
							exception = new KeyNotFoundException(
								$"Key [ {step.key} ] by type [ {keyType.Name} ] not found in dictionary [ {step.name} ], path: {Path}");
							return default;
						}

						obj = dictionary[key];
					}
					else
					{
						exception = new ArgumentException(
							$"Step '{step.name}' is marked as dictionary element, but resolved object is not IDictionary (actual: {obj?.GetType().Name ?? "null"}), path: {Path}");
						return default;
					}
				}
				else
				{
					obj = obj.GetValueByReflectionSafe(step.name);
				}
			}

			if (obj is not T value)
			{
				if (obj == null)
				{
					exception = new NullReferenceException($"Final resolved value is null, path: {Path}");
					return default;
				}

				exception = new ArgumentException(
					$"Final resolved type is '{obj.GetType().Name}', expected '{typeof(T).Name}', path: {Path}");
				return default;
			}

			if (cached)
				_cache = value;

			return value;
		}

		public void CacheClear() => _cache = default;

		public static implicit operator MemberReflectionReference<T>(MemberReferencePathStep[] steps) => new() {steps = steps};

		public override string ToString() => string.Join(".", steps ?? Array.Empty<MemberReferencePathStep>());

		public string ToString(bool type) =>
			string.Join(".", steps) + (type ? $" ({typeof(T).Name})" : "");
	}

	[Serializable]
	public struct MemberReferencePathStep
	{
		public string name;
		public string key;

		/// <summary>
		/// Индекс в массиве, если > 0, значит объект массив (чтобы получить индекс -1)
		/// </summary>
		public int index;

		public bool IsArrayElement => index > 0;
		public int ArrayElementIndex => index - 1;

		public bool IsDictionaryElement => !key.IsNullOrEmpty();

		public MemberReferencePathStep(string name, int index = -1, string key = null)
		{
			this.name = name;
			this.index = index >= 0 ? index + 1 : 0;
			this.key = key;
		}

		public static implicit operator MemberReferencePathStep(string name) => new(name);
		public static implicit operator MemberReferencePathStep((string name, int index) tuple) => new(tuple.name, tuple.index);
		public static implicit operator MemberReferencePathStep((string name, string key) tuple) => new(tuple.name, key: tuple.key);

		public override string ToString() =>
			IsArrayElement ? $"{name}[{ArrayElementIndex}]" : IsDictionaryElement ? $"{name}" + "{" + key + "}" : name;
	}

	public interface IMemberReflectionReference
	{
		/// <summary>
		/// <see cref="MemberReflectionReference{T}.steps"/>
		/// </summary>
		public const string STEPS_FIELD_NAME = "steps";

		/// <summary>
		/// <see cref="MemberReflectionReference{T}.Path"/>
		/// </summary>
		public const string PATH_FIELD_NAME = "Path";

		/// <summary>
		/// <see cref="MemberReflectionReference{T}._cache"/>
		/// </summary>
		public const string CACHE_FIELD_NAME = "_cache";

		public Type MemberType { get; }
		public MemberReferencePathStep[] Steps { get; }
	}
}
