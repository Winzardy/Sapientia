using System;

namespace Sapientia
{
	[AttributeUsage(AttributeTargets.Event)]
	public abstract class EventPolicyAttribute : Attribute
	{
	}

	/// <remarks>
	/// ⚠️ Все подписчики автоматически удаляются после Dispose или Release объекта
	/// </remarks>
	public sealed class AutoClearOnDisposeEventAttribute : EventPolicyAttribute
	{
	}

	/// <remarks>
	/// ⚠️ Все подписчики автоматически удаляются после первого вызова события
	/// </remarks>
	public sealed class InvokeOnceEventAttribute : EventPolicyAttribute
	{
	}
}
