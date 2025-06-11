using System;
using System.Runtime.CompilerServices;

namespace Sapientia
{
	public delegate void Receiver<T>(in T value);

	/// <summary>
	/// Легковесное реактивное поле, уведомляющее подписчиков при изменении значения.
	/// Поддерживает опциональный вызов подписчика при подписке.
	/// </summary>
	[Serializable]
	public struct ReactiveField<T> : IReactiveProperty<T>
	{
		private T _value;
		public T Value { get => _value; internal set => Set(in value); }

		private event Receiver<T> _valueChanged;

		public static implicit operator T(ReactiveField<T> property) => property.Value;

		public void Set(in T value, bool equals)
		{
			if (equals && Equals(value, _value))
				return;

			Set(in value);
		}

		public void Set(in T value)
		{
			_value = value;
			_valueChanged?.Invoke(in _value);
		}

		public void Subscribe(Receiver<T> receiver, bool invokeOnSubscribe = true)
		{
			if (invokeOnSubscribe)
				receiver?.Invoke(in _value);

			_valueChanged += receiver;
		}

		public void Unsubscribe(Receiver<T> receiver) => _valueChanged -= receiver;
	}

	public interface IReactiveProperty<T>
	{
		public T Value { get; }

		public void Subscribe(Receiver<T> receiver, bool invokeOnSubscribe = true);
		public void Unsubscribe(Receiver<T> receiver);

		/// <summary>
		/// Не работает, если нет сеттера ( { set; } )
		/// </summary>
		public static IReactiveProperty<T> operator +(IReactiveProperty<T> property, Receiver<T> receiver)
		{
			property.Subscribe(receiver);
			return property;
		}

		/// <summary>
		/// Не работает, если нет сеттера ( { set; } )
		/// </summary>
		public static IReactiveProperty<T> operator -(IReactiveProperty<T> property, Receiver<T> receiver)
		{
			property.Unsubscribe(receiver);
			return property;
		}

		public static Receiver<T> operator +(Receiver<T> receiver, IReactiveProperty<T> property)
		{
			property.Subscribe(receiver);
			return receiver;
		}

		public static Receiver<T> operator -(Receiver<T> receiver, IReactiveProperty<T> property)
		{
			property.Unsubscribe(receiver);
			return receiver;
		}
	}

	public static class ReactivePropertyExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Increment(this ref ReactiveField<int> field) =>
			field.Value++;
	}
}
