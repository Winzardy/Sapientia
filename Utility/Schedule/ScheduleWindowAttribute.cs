using System;

namespace Sapientia
{
	/// <summary>
	/// Включает редактирование длительностей окон (<see cref="ScheduleScheme.durations"/>) у точек
	/// расписания в поле: вешается на поле со <see cref="ScheduleScheme"/> (или на сам массив точек).
	/// Без атрибута длительность скрыта — большинству расписаний (сбросы, триггеры) окно не нужно
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class ScheduleWindowAttribute : Attribute
	{
	}
}
