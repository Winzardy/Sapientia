using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Management
{
	public interface IContentImporter
	{
		public Task<IList<IContentEntry>> ImportAsync(CancellationToken cancellationToken = default);
	}
}
