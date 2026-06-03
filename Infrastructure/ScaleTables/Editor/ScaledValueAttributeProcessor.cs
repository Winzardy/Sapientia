#if UNITY_EDITOR
using Content;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sapientia.ScaleTables.Editor
{
	public class ScaledValueAttributeProcessor : OdinAttributeProcessor<ScaledValue>
	{
		public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
		{
			switch (member.Name)
			{
				case nameof(ScaledValue.scaleTableId):
					attributes.Add(new ContentReferenceAttribute(typeof(ScaleTableConfig)));
					break;

				default:
					break;
			}
		}
	}
}
#endif

