using System;
using System.Diagnostics;

namespace Content
{
	[Conditional("CLIENT")]
	public class ContentTypeFilterAttribute : Attribute
	{
		public Type[] Types { get; private set; }

		/// <inheritdoc cref="Sirenix.OdinInspector.TypeFilterAttribute.DropdownTitle"/>
		public string DropdownTitle;

		/// <inheritdoc cref="Sirenix.OdinInspector.TypeFilterAttribute.DrawValueNormally"/>
		public bool DrawValueNormally;

		public string Expression { get; }

		public ContentTypeFilterAttribute(params Type[] types)
		{
			Types = types;
		}

		public ContentTypeFilterAttribute(string expression)
		{
			Expression = expression;
		}
	}
}
