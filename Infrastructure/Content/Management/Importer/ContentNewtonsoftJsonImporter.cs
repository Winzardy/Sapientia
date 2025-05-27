#if NEWTONSOFT
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Content.Management
{
	public class ContentNewtonsoftJsonImporter
	{
		public static JsonSerializerSettings Settings =
			new()
			{
				Converters = new List<JsonConverter>
				{
					new ContentReferenceJsonConverter(),
					new SerializableGuidJsonConverter()
				},
				TypeNameHandling = TypeNameHandling.Auto
			};
	}
}
#endif
