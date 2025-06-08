using System;

namespace Sapientia
{
	public interface IPack
	{
	}

	/// <summary>
	/// Пачка, куча чего-то {T}
	/// </summary>
	/// <typeparam name="T">Что в пачке</typeparam>
	[Serializable]
	public struct Pack<T> : IPack, IHolder<T>
	{
		public T target;
#if CLIENT
		[UnityEngine.Serialization.FormerlySerializedAs("amount")]
#endif
		public int count;

		public Pack(T target, int count)
		{
			this.target = target;
			this.count = count;
		}

		public void Deconstruct(out T target, out int count)
		{
			target = this.target;
			count = this.count;
		}

		public static implicit operator Pack<T>((T, int amount) tuple) => new(tuple.Item1, tuple.amount);
		public static implicit operator bool(Pack<T> pack) => pack.count > 0 && pack.target != null;
	}
}
