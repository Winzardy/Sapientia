namespace Content
{
	public static class ContentUtility
	{
		/// <summary>
		/// Создание уникального идентификатора по guid и маркеру
		/// </summary>
		/// <param name="guid">Уникальный идентификатор записи</param>
		/// <param name="mark">Маркер по которому можно определить источник</param>
		/// <returns>Уникальный идентификатор</returns>
		public static string Combine(in SerializableGuid guid, string mark) => $"{mark}_{guid}";
	}
}
