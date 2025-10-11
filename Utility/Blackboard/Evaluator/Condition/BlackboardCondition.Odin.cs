#if CLIENT
using UnityEngine;

namespace Sapientia.Conditions
{
	public abstract partial class BlackboardCondition
	{
		public static readonly Color COLOR = new(R, G, B, A);

		public const float R = 0.5f;
		public const float G = 0.5f;
		public const float B = 1;
		public const float A = 1;

		public const float BOX_R = 0.8f;
		public const float BOX_G = 0.8f;
		public const float BOX_B = 1;

		public const string GROUP =  "g";
		public const int OPERATOR_WIDTH = 55;
	}
}
#endif
