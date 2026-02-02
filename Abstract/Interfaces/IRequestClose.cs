#nullable disable
using System;

namespace Sapientia
{
	public interface ICloseRequestor
	{
		event Action CloseRequested;
	}
}
