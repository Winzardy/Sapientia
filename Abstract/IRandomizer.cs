using System;

namespace Sapientia
{
	public interface IRandomizer<T>
		where T : struct, IComparable<T>
	{
		T Next();
		T Next(T max);
		T Next(T min, T max);
	}
}
