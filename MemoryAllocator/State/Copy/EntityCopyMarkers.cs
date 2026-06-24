using System;

namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Метка: сущность не копируется (например, моб или игрок). В обходе её пропускают, а ссылки на неё
	/// заменяют на <see cref="Entity.EMPTY"/>.
	/// </summary>
	public struct IgnoreEntityCopy : IComponent {}

	/// <summary>
	/// Генератор пишет partial <see cref="ICopiable{T}"/> для этого ссылочного компонента. Ставится
	/// вместе с partial, когда компонент вводят в копирование. Плоским компонентам не нужен — они
	/// копируются значением без интерфейса. Без метки ссылочный компонент идёт в лог-отчёт (worklist).
	/// </summary>
	[AttributeUsage(AttributeTargets.Struct)]
	public sealed class GenerateCopyAttribute : Attribute {}

	/// <summary>
	/// Компонент реализует <see cref="ICopiable{T}"/> вручную (держатели union-структур). Генератор
	/// partial не пишет, но компонент попадает в диспатч.
	/// </summary>
	[AttributeUsage(AttributeTargets.Struct)]
	public sealed class ManualCopyAttribute : Attribute {}

	/// <summary>
	/// Компонент не копируется (временные данные навигации или физики - в новом мире создаются заново).
	/// </summary>
	[AttributeUsage(AttributeTargets.Struct)]
	public sealed class SkipCopyAttribute : Attribute {}

	/// <summary>
	/// Поле-сущность принадлежит компоненту (дочерняя). Кладётся в обход и копируется. Без метки поле
	/// <see cref="Entity"/> считается ссылкой на чужую сущность — только перенастройка, в обход не идёт.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class OwnedAttribute : Attribute {}

	/// <summary>
	/// Коллекция сущностей — это ссылки на чужие сущности, не владение. В обход не кладётся, только
	/// поэлементная перенастройка. Без метки коллекция <see cref="Entity"/> считается владением.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class LinkAttribute : Attribute {}

	/// <summary>
	/// Поле при копировании обнуляется и не попадает в AppendEntities.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class IgnoreCopyAttribute : Attribute {}
}
