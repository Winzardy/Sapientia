using System;

namespace Sapientia
{
	public interface IProgressNotifier
	{
		float Progress { get; }

		event Action<float> ProgressChanged;
	}
}
