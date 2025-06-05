namespace Sapientia
{
	public static class BlackboardUtility
	{
		// Для каких целей Clone? На этот вопрос отвечу позже после активного использования
		public static T Clone<T>(this T source) where T : Blackboard, new()
		{
			var blackboard = new T();
			source.Clone(blackboard);
			return blackboard;
		}
	}
}
