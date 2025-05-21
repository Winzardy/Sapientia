using System;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

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

	//TODO: Конкурс на лучшее название
	[DisableConvertParent]
	[Serializable]
	public struct Toggle<T> : IToggle<T>
	{
		[FormerlySerializedAs("use")]
		public bool enable;

		[FormerlySerializedAs("duration")]
		public T value;

		public Toggle(T value, bool enable = true)
		{
			this.enable = enable;
			this.value = value;
		}

		public static implicit operator T(Toggle<T> toggle) => toggle.enable ? toggle.value : default;

		public static implicit operator Toggle<T>(T value)
		{
			if (value == null)
				return default;

			return new Toggle<T>()
			{
				enable = true,
				value = value
			};
		}

		public static implicit operator bool(Toggle<T> obj) => obj.enable;

		bool IToggle.Enable => enable;
		T IToggle<T>.Value => value;
	}
}
