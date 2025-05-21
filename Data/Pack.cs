using System;

namespace Sapientia
{
	public interface IPack
	{
		/// <summary>
		/// <see cref="Pack{T}.target"/>
		/// </summary>
		public const string TARGET_FIELD_NAME = "target";

		/// <summary>
		/// <see cref="Pack{T}.amount"/>
		/// </summary>
		public const string AMOUNT_FIELD_NAME = "amount";
	}

	/// <summary>
	/// Пачка, куча чего-то {T}
	/// </summary>
	/// <typeparam name="T">Что в пачке</typeparam>
	[Serializable]
	public struct Pack<T> : IPack
	{
		public T target;
		public int amount;

		public Pack(T target, int amount)
		{
			this.target = target;
			this.amount = amount;
		}

		public void Deconstruct(out T target, out int amount)
		{
			target = this.target;
			amount = this.amount;
		}

		public static implicit operator Pack<T>((T, int amount) tuple) => new(tuple.Item1, tuple.amount);
		public static implicit operator bool(Pack<T> pack) => pack.amount > 0 && pack.target != null;
	}
}
