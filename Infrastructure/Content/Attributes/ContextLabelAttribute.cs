using System;
using System.Diagnostics;
using System.Runtime.Remoting.Contexts;
using Fusumity.Attributes;

namespace Content
{
	/// <summary>
	/// Превращает поле в человеко-читаемый лейбл в инспекторе.
	/// Для корректной работы требуется, чтобы в контенте существовала коллекция с типом <paramref name="type"/>.
	/// </summary>
	[Conditional("CLIENT")]
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class ContextLabelAttribute : Attribute
	{
		/// <summary>
		/// Тип коллекции, из которой берутся значения для отображения.
		/// Обычно это идентификатор или путь к коллекции (например, "Attachments", "Buffs" и т.д.)
		/// </summary>
		public string Catalog { get; }

		public ContextLabelAttribute(string catalog)
		{
			Catalog = catalog;
		}
	}

	/// <summary>
	/// Превращает поле в человеко-читаемый лейбл в инспекторе.
	/// Для корректной работы требуется, чтобы в контенте существовала коллекция с типом <paramref name="type"/>.
	/// </summary>
	[Conditional("CLIENT")]
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class ContextLabelParentAttribute : ParentAttribute
	{
		/// <summary>
		/// Тип коллекции, из которой берутся значения для отображения.
		/// Обычно это идентификатор или путь к коллекции (например, "Attachments", "Buffs" и т.д.)
		/// </summary>
		public string Catalog { get; }

		public ContextLabelParentAttribute(string catalog)
		{
			Catalog = catalog;
		}

		public override Attribute Convert() => new ContextAttribute(Catalog);
	}
}
