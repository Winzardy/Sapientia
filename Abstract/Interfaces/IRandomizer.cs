using System;

namespace Sapientia
{
	public interface IRandomizer<T>
		where T : struct, IComparable<T>
	{
		T Next();
		T Next(T maxExclusive);
		T Next(T minInclusive, T maxExclusive);
	}
}
