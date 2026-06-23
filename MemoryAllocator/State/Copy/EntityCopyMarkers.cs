using System;

namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Метка: сущность не копируется (например, моб или игрок). В обходе её пропускают, а ссылки на неё
	/// заменяют на <see cref="Entity.EMPTY"/>.
	/// </summary>
	public struct IgnoreEntityCopy : IComponent {}

	/// <summary>
	/// Компонент пишет реализацию <see cref="ICopiable{T}"/> вручную. Генератор его пропускает.
	/// </summary>
	[AttributeUsage(AttributeTargets.Struct)]
	public sealed class ManualCopyAttribute : Attribute {}

	/// <summary>
	/// Компонент не копируется (временные данные навигации или физики - в новом мире создаются заново).
	/// </summary>
	[AttributeUsage(AttributeTargets.Struct)]
	public sealed class SkipCopyAttribute : Attribute {}

	/// <summary>
	/// Поле при копировании обнуляется и не попадает в AppendEntities.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class IgnoreCopyAttribute : Attribute {}
}
