using System.Threading;
using System.Threading.Tasks;

namespace Content.Management
{
	public interface IContentJsonTextResolver
	{
		public Task<string> ResolveAsync(CancellationToken cancellationToken = default);
	}
}
