using Sapientia.Extensions;

namespace Submodules.Sapientia.Data
{
	/// <summary>
	/// Вспомогательная обёртка для ссылочных типов, позволяющая получать из них типы-наследники минуя стандартный боксинг.
	/// Следует использовать только для чтения и в Hot местах.
	/// </summary>
	public ref struct Boxed<T>
		where T: class
	{
		public T value;

		public T1 As<T1>()
			where T1 : T
		{
			return UnsafeExt.As<T, T1>(ref value);
		}
	}

	public static class BoxedExt
	{
		public static Boxed<T> Box<T>(this T value)
			where T: class
		{
			return new Boxed<T>
			{
				value = value,
			};
		}
	}
}
