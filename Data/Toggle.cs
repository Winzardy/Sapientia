using System;

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
	public partial struct Toggle<T> : IToggle<T>
	{
#if CLIENT
		[UnityEngine.Serialization.FormerlySerializedAs("use")]
#endif
		public bool enable;

#if CLIENT
		[UnityEngine.Serialization.FormerlySerializedAs("duration")]
#endif
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
