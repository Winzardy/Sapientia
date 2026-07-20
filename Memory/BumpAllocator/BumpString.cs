using System;
using System.Text;

namespace Sapientia.Memory
{
	public struct BumpString
	{
		/// <summary>
		/// Сырые байты строки. Публичное поле (как <see cref="BumpArray{T}.offset"/>/length у самого
		/// массива): валидация формата обязана проверять диапазон данных по ref, а вернуть ref на поле
		/// this из метода структуры C# не позволяет.
		/// </summary>
		public BumpArray<byte> encodedString;

		/// <summary>Длина строки в БАЙТАХ выбранной кодировки (не в символах).</summary>
		public readonly int Length => encodedString.Length;

		public void SetString_UTF8(ref BumpHeader header, string value)
		{
			SetString(ref header, Encoding.UTF8, value);
		}

		public void SetString(ref BumpHeader header, Encoding encoding, string value)
		{
			var bytes = encoding.GetBytes(value).AsSpan();
			encodedString.Alloc(ref header, bytes);
		}

		public string GetString_UTF8()
		{
			return GetString(Encoding.UTF8);
		}

		public string GetString(Encoding encoding)
		{
			return encoding.GetString(encodedString.GetSpan());
		}
	}
}
