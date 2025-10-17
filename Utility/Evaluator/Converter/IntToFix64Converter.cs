using System;
using Sapientia.Deterministic;

namespace Sapientia.Evaluators.Converter
{
	[Serializable]
	public class IntToFix64Converter<TContext> : Converter<TContext, int, Fix64>
	{
		protected override Fix64 Convert(int value) => value;
	}
}
