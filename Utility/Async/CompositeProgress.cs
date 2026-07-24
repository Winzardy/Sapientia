using System;

namespace Sapientia.Utility
{
	public interface IProgressValue
	{
		float Progress { get; set; }
	}

	public sealed class CompositeProgress<T> where T : struct, IProgressValue
	{
		private readonly IProgress<T>? _target;

		private bool _sealed;

		public float MaxWeight { get; private set; }
		public float AllocatedWeight { get; private set; }
		public float CurrentWeightedValue { get; private set; }
		public float CurrentValue { get; private set; }

		public CompositeProgress(float maxWeight = 0f, IProgress<T>? target = null)
		{
			if (maxWeight < 0f)
				throw new ArgumentOutOfRangeException(nameof(maxWeight), maxWeight, "Maximum weight cannot be negative");

			MaxWeight = maxWeight;
			_target = target;
		}

		public void Seal()
		{
			_sealed = true;
		}

		public IProgress<T> CreateChild(float weight)
		{
			if (weight < 0f)
				throw new ArgumentOutOfRangeException(nameof(weight), weight, "Weight must be positive");

			var newAllocatedWeight = AllocatedWeight + weight;

			if (_sealed && newAllocatedWeight > MaxWeight)
			{
				// защита, если пытаемся добавить чилд с большим весом на уже запечатанный
				// если такое произойдет, то чилды просто будут с 0 весом и не учитываться в расчетах общего прогресса
				weight = Math.Clamp(weight, 0f, MaxWeight - AllocatedWeight);
				newAllocatedWeight = MaxWeight;
			}

			AllocatedWeight = newAllocatedWeight;

			if (!_sealed && AllocatedWeight > MaxWeight)
			{
				MaxWeight = AllocatedWeight;
			}

			return new ChildProgress(this, weight);
		}

		private void ReportChild(ChildProgress child, T progress)
		{
			_sealed = true;

			var newValue = Math.Clamp(progress.Progress, 0f, 1f);

			var delta = newValue - child.CurrentValue;
			child.CurrentValue = newValue;

			CurrentWeightedValue += delta * child.Weight;
			CurrentWeightedValue = Math.Clamp(CurrentWeightedValue, 0f, MaxWeight);

			CurrentValue = Math.Clamp(CurrentWeightedValue / MaxWeight, 0f, 1f);
			progress.Progress = CurrentValue;

			_target?.Report(progress);
		}

		private sealed class ChildProgress : IProgress<T>
		{
			private readonly CompositeProgress<T> _owner;

			public float Weight { get; }
			public float CurrentValue { get; set; }

			public ChildProgress(CompositeProgress<T> owner, float weight)
			{
				_owner = owner;
				Weight = weight;
			}

			public void Report(T value)
			{
				_owner.ReportChild(this, value);
			}
		}
	}
}
