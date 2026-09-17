using System.Collections.Generic;
using System;

#if CLIENT
using UnityEngine.Serialization;
#endif

namespace Sapientia
{
	public interface IToggle
	{
		public bool Enable { get; }
	}

	public interface IToggle<T> : IToggle
	{
		public T Value { get; }
	}

	[Serializable]
	public partial struct Toggle<T> : IToggle<T>, IEquatable<Toggle<T>>
	{
#if CLIENT
		[FormerlySerializedAs("use")]
#endif
		public bool enable;

#if CLIENT
		[FormerlySerializedAs("duration")]
#endif
		public T value;

		public Toggle(T value, bool enable = true)
		{
			this.enable = enable;
			this.value  = value;
		}

		public readonly bool IsEnable(out T value)
		{
			value = this.value;
			return enable;
		}

		public static implicit operator T(Toggle<T> toggle) => toggle.enable ? toggle.value : default;

		public static implicit operator Toggle<T>(T value)
		{
			if (value == null)
				return default;

			return new Toggle<T>()
			{
				enable = true,
				value  = value
			};
		}

		public static implicit operator bool(Toggle<T> obj) => obj.enable;

		bool IToggle.Enable => enable;
		T IToggle<T>.Value => value;
		public bool Equals(Toggle<T> other)
			=> enable == other.enable && EqualityComparer<T>.Default.Equals(value, other.value);

		public override bool Equals(object obj) => obj is Toggle<T> other && Equals(other);

		public override int GetHashCode() => HashCode.Combine(enable, value);
	}
}
