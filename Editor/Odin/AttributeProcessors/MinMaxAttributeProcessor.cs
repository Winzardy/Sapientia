﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Sapientia.Extensions.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;

namespace Sapientia.Editor
{
	public class MinMaxAttributeProcessor : OdinAttributeProcessor<IMinMax>
	{
		public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
		{
			const string MIN = "min";
			const string MAX = "max";

			base.ProcessChildMemberAttributes(parentProperty, member, attributes);

			var type = member.GetReturnType();
			var has = type.HasAttribute<DisableConvertParentAttribute>();

			switch (member.Name)
			{
				case MIN:
					attributes.Add(new HorizontalGroupAttribute());
					attributes.Add(new MaxValueAttribute(MAX));

					foreach (var parentAttribute in parentProperty.Attributes)
						if (parentAttribute is ParentAttribute attribute)
							attributes.Add(has ? attribute : attribute.Convert());

					break;

				case MAX:

					attributes.Add(new HorizontalGroupAttribute());

					foreach (var parentAttribute in parentProperty.Attributes)
						if (parentAttribute is ParentAttribute attribute)
							attributes.Add(has ? attribute : attribute.Convert());

					attributes.Add(new MinValueAttribute(MIN));
					break;
			}
		}

		public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
		{
			base.ProcessSelfAttributes(property, attributes);

			attributes.Add(new InlinePropertyAttribute
			{
				LabelWidth = 28
			});
		}
	}
}
#endif
