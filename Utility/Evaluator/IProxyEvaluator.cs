using System;

namespace Sapientia.Evaluators
{
	public interface IProxyEvaluator
	{
		public IEvaluator Proxy { get; }
		public Type ProxyType { get; }
#if CLIENT
		public const string SELECTOR_NAME = "\u2009Meta (shared, interop)";
		public const string SELECTOR_GROUP = "/";
		public const Sirenix.OdinInspector.SdfIconType SELECTOR_ICON = Sirenix.OdinInspector.SdfIconType.Joystick;
#endif
	}
}
