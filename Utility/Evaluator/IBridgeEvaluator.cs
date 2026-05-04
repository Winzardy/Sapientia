using System;

namespace Sapientia.Evaluators
{
	public interface IBridgeEvaluator
	{
		IEvaluator Proxy { get; }
		Type ProxyType { get; }
	}
}
