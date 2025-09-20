using System;

namespace Sapientia
{
	[Serializable]
	public class IntToFloatConverter : BlackboardConverter<int, float>
	{
		protected override float Convert(int value) => value;
	}
}