using System;

namespace Trading
{
	/// <summary>
	/// Данный аттрибут дает возможность в редакторе фильтровать допустимые цены!
	/// </summary>
	/// <list type="table">
	/// <item>
	/// <term>field</term>
	/// <description>выдает доступ</description>
	/// </item>
	/// <item>
	/// <term>class</term>
	/// <description>требует доступ</description>
	/// </item>
	/// </list>
	public class TradeAccessAttribute : Attribute
	{
		public TradeAccessType Access { get; }

		public TradeAccessAttribute(TradeAccessType access)
		{
			Access = access;
		}
	}

	public enum TradeAccessType
	{
		Low,

		ByParent, // Доступ определяется родителем

		Medium,
		High,
	}
}
