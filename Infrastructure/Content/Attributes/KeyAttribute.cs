using System;
using System.Diagnostics;

namespace Content
{
	/// <summary>
	/// Связывает ключ с каталогом допустимых значений и их человеко-читаемыми подписями в инспекторе
	/// Для корректной работы требуется контентный каталог с указанным идентификатором
	/// </summary>
	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class KeyAttribute : Attribute
	{
		/// <summary>
		/// Идентификатор каталога, из которого берутся допустимые ключи и подписи
		/// </summary>
		public string CatalogId { get; }

		public KeyAttribute(string catalogId)
		{
			CatalogId = catalogId;
		}
	}
}
