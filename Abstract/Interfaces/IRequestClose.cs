using System;

namespace Sapientia
{
	public interface IRequestClose
	{
		public event Action RequestedClose;

		public void RequestClose();
	}

}
