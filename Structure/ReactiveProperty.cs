using System;

namespace Sapientia
{
	[Serializable]
	public struct ReactiveField<T> : IReactiveProperty<T>
	{
		private T _value;
		public T Value => _value;

		private event Receiver<T> _receiver;

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
			_receiver?.Invoke(in _value);
		}

		public void Subscribe(Receiver<T> receiver, bool invokeOnSubscribe = true)
		{
			if (invokeOnSubscribe)
				receiver?.Invoke(in _value);

			_receiver += receiver;
		}

		public void Unsubscribe(Receiver<T> receiver) => _receiver -= receiver;
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

	public delegate void Receiver<T>(in T value);
}
