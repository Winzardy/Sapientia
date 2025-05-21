namespace Content
{
	public static class ContentMenuConstants
	{
		public const int DATABASE_PRIORITY = -201;

		public const string GROUP_NAME = "Content";

		public const string CREATE_MENU = GROUP_NAME + "/"; // используется в Templates!
		public const string TOOLS_MENU = "Tools/" + GROUP_NAME + "/";

		public const string EDITOR_MENU = TOOLS_MENU + "Editor/";

		public const string LOG_MENU = TOOLS_MENU + "Log/";
		public const string LOG_NESTED_ENTRY_MENU = TOOLS_MENU + "Log/Nested Entry/";

		public const string CONSTANTS_MENU = TOOLS_MENU + "Constants/";
		public const string DATABASE_MENU = TOOLS_MENU + "Database/";

		public const string DATABASE_ITEM_NAME = "/Database";
		public const string FULL_CREATE_MENU = "Assets/Create/" + GROUP_NAME + "/";
	}
}
