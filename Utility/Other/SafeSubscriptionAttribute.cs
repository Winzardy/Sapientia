using System;

namespace Sapientia
{
	/// <remarks>
	/// ⚠️ Все подписчики автоматически удаляются после Dispose или Release объекта
	/// </remarks>
	[AttributeUsage(AttributeTargets.Event)]
	public sealed class AutoClearOnDisposeEventAttribute : Attribute
	{
	}

	/// <remarks>
	/// ⚠️ Все подписчики автоматически удаляются после первого вызова события
	/// </remarks>
	[AttributeUsage(AttributeTargets.Event)]
	public sealed class InvokeOnceEventAttribute : Attribute
	{
	}
}
