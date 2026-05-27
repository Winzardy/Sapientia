using JetBrains.Annotations;

namespace Sapientia
{
	public interface IValidatable
	{
		bool Validate(out string message);
	}
}
