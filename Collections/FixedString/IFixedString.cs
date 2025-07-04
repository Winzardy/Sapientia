using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.Collections.FixedString
{
	public interface IFixedString
	{
		bool IsEmpty { get; }
		int Length { get; set; }
		int Capacity { get; }

		SafePtr GetSafePtr();

		bool TryResize(int newLength, ClearOptions clearOptions = ClearOptions.ClearMemory);
	}
}
