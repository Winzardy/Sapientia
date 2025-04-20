using System;
using System.Collections;

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
		/// <param name="obj">Сначала передается корень, потом переиспользуется</param>
		/// <param name="cache">Нужно ли кэшировать, важно отметить что кеширует только если получилось достать значение</param>
		public T Resolve(object obj, bool cache = false)
		{
			if (_cache != null)
				return _cache;

			for (int i = 0; i < steps.Length; i++)
			{
				var step = steps[i];

				if (obj == null)
					return default;

				if (step.IsArray) //In Array
				{
					obj = obj.GetReflectionValueSafe(step.name);
					if (obj is IList list)
						obj = list[step.ArrayIndex];
					else
						return default;
				}
				else
				{
					obj = obj.GetReflectionValueSafe(step.name);
				}
			}

			if (obj is not T value)
				return default;

			if (cache)
				_cache = value;

			return value;
		}

		public void CacheClear() => _cache = default;

		public static implicit operator MemberReflectionReference<T>(MemberReferencePathStep[] steps) => new() {steps = steps};

		public override string ToString() => string.Join(".", steps ?? Array.Empty<MemberReferencePathStep>());
	}

	[Serializable]
	public struct MemberReferencePathStep
	{
		public string name;

		/// <summary>
		/// Индекс в массиве, если > 0, значит объект массив (чтобы получить индекс -1)
		/// </summary>
		public int index;

		public bool IsArray => index > 0;
		public int ArrayIndex => index - 1;

		public MemberReferencePathStep(string name, int index = -1)
		{
			this.name = name;
			this.index = index >= 0 ? index + 1 : 0;
		}

		public static implicit operator MemberReferencePathStep(string name) => new(name);
		public static implicit operator MemberReferencePathStep((string name, int index) tuple) => new(tuple.name, tuple.index);

		public override string ToString() => IsArray ? $"{name}[{ArrayIndex}]" : name;
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
