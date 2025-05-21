namespace Content
{
	//TODO: перенести в ScriptableObject (там выдавать ConstantsGenerationData)
	public class ConstantsAttribute : System.Attribute
	{
		public const string CUSTOM_CONSTANT_SEPARATOR = ";";

		public (string from, string to)? ReplaceForClassName { get; private set; }
		public string[] CustomConstants { get; private set; }

		public string[] FilterOut { get; private set; }

		public ConstantsAttribute()
		{
		}

		public ConstantsAttribute(string remove, params string[] customConstants)
		{
			ReplaceForClassName = (remove, string.Empty);
			CustomConstants = customConstants;
		}

		public ConstantsAttribute(string[] filterOut, params string[] customConstants)
		{
			FilterOut = filterOut;
			CustomConstants = customConstants;
		}

		public ConstantsAttribute(string remove, string[] filterOut, params string[] customConstants)
		{
			FilterOut = filterOut;
			ReplaceForClassName = (remove, string.Empty);
			CustomConstants = customConstants;
		}

		public ConstantsAttribute(string[] filterOut, (string, string) replaceForClassName, params string[] customConstants)
		{
			FilterOut = filterOut;
			ReplaceForClassName = replaceForClassName;
			CustomConstants = customConstants;
		}

		public ConstantsAttribute((string, string) replaceForClassName, params string[] customConstants)
		{
			ReplaceForClassName = replaceForClassName;
			CustomConstants = customConstants;
		}
	}
}
