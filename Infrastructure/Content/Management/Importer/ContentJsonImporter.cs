using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sapientia.Extensions;
using Sapientia.JsonConverters;

namespace Content.Management
{
	public class ContentJsonImporter : IContentImporter, IDisposable
	{
		public static readonly JsonSerializerSettings serializerSettings = new()
		{
			Converters = new JsonConverter[]
			{
				new DictionaryConverter()
			},
			DefaultValueHandling = DefaultValueHandling.Ignore,
			TypeNameHandling = TypeNameHandling.Auto,
			NullValueHandling = NullValueHandling.Ignore,
		};

		public static readonly JsonSerializer defaultSerializer = JsonSerializer.Create(serializerSettings);

		private readonly IContentJsonTextResolver _resolver;
		private readonly bool _useCache;

		private List<IContentEntry> _cache;

		public ContentJsonImporter(IContentJsonTextResolver resolver, bool useCache = true)
		{
			_resolver = resolver;
			_useCache = useCache;
		}

		public void Dispose() => Clear();

		public async Task<IList<IContentEntry>> ImportAsync(CancellationToken cancellationToken = default)
		{
			if (_useCache && _cache != null)
				return _cache;

			var text = await _resolver.ResolveAsync(cancellationToken);
			var json = text.FromJson<ContentJsonFormat>(serializerSettings);
			_cache?.Clear();
			_cache ??= new List<IContentEntry>();
			json.Fill(_cache);
			return _cache;
		}

		public void Clear()
		{
			_cache.Clear();
			_cache = null;
		}
	}
}
