#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Sapientia.Extensions.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;

namespace Sapientia.Editor
{
	public class PackAttributeProcessor : OdinAttributeProcessor<IPack>
	{
		public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
		{
			base.ProcessChildMemberAttributes(parentProperty, member, attributes);

			var type = member.GetReturnType();
			var has = type.HasAttribute<DisableConvertParentAttribute>();

			switch (member.Name)
			{
				case IPack.TARGET_FIELD_NAME:
					attributes.Add(new HorizontalGroupAttribute());
					attributes.Add(new HideLabelAttribute());
					break;
				case IPack.AMOUNT_FIELD_NAME:
					attributes.Add(new HorizontalGroupAttribute(width:0.35f));
					attributes.Add(new PropertyOrderAttribute(-1));
					attributes.Add(new HideLabelAttribute());
					foreach (var parentAttribute in parentProperty.Attributes)
						if (parentAttribute is ParentAttribute attribute)
							attributes.Add(has ? attribute : attribute.Convert());

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
