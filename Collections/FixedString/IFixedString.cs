using Sapientia.Extensions;

namespace Sapientia.Collections.Fixed
{
	public interface IFixedString
	{
		bool IsEmpty { get; }
		int Length { get; set; }
		int Capacity { get; }

		unsafe byte* GetUnsafePtr();

		bool TryResize(int newLength, ClearOptions clearOptions = ClearOptions.ClearMemory);
	}
}
