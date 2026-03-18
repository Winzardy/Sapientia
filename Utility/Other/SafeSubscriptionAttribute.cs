using System;

namespace Sapientia
{
	/// <remarks>
	/// ⚠️ Защита от утечки при dispose или release
	/// </remarks>
	[AttributeUsage(AttributeTargets.Event)]
	public sealed class SafeSubscriptionAttribute : Attribute
	{
	}
}
