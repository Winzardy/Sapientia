#if CLIENT
using UnityEngine;

namespace Sapientia.Conditions
{
	public abstract partial class Condition<T>
	{
		public static readonly Color COLOR = new(R, G, B, A);

		public const float R = ICondition.R;
		public const float G = ICondition.G;
		public const float B = ICondition.B;
		public const float A = ICondition.A;

		public const float BOX_R = 0.8f;
		public const float BOX_G = 0.8f;
		public const float BOX_B = 1;

		public const string GROUP = ICondition.GROUP;
		public const int OPERATOR_WIDTH = ICondition.OPERATOR_WIDTH;
	}
}
#endif
