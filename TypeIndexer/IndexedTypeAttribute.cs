using System;

namespace Sapientia.TypeIndexer
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = true)]
	public class IndexedTypeAttribute : Attribute
	{
		public IndexedTypeAttribute()
		{
		}
	}
}
