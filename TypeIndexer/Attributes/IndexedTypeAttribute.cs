using System;

namespace Sapientia.TypeIndexer
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface)]
	public class IndexedTypeAttribute : Attribute
	{
		public IndexedTypeAttribute()
		{
		}
	}
}
