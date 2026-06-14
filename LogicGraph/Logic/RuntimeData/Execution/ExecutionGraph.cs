using Sapientia.Collections;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.LogicGraph
{
	public struct ExecutionGraph
	{
		public static readonly int runtimesCount = EnumValues<RuntimeType>.ENUM_LENGHT;

		public Id<ExecutionIteration> currentIteration;

		public UnsafeArray<ExecutionRuntime> runtimes; // Length = runtimesCount

		public UnsafeList<ExecutionIteration> iterationsToSchedule; // Length = количество параллельных потоков * runtimesCount.

		public ExecutionGraph(Id<MemoryManager> memoryId = default)
		{
			currentIteration = 0;
			runtimes = new UnsafeArray<ExecutionRuntime>(memoryId, runtimesCount);
			iterationsToSchedule = new UnsafeList<ExecutionIteration>(memoryId);
		}

		public bool TryRun(RuntimeType runtimeType, int parallelCount)
		{
			iterationsToSchedule.EnsureCount(parallelCount * runtimesCount);

			ref var runtime = ref runtimes[runtimeType.ToInt()];
			if (runtime.iterations.count == 0)
				return false;
			ref var iteration = ref runtime.iterations[0];

			E.ASSERT(iteration.iterationId >= currentIteration);
			if (iteration.iterationId > currentIteration)
				return false;

			// Прогоняем DAG-итерацию (батчи с зависимостями), затем снимаем её и передаём «эстафету»
			// другому runtime (Unmanaged↔Managed-чередование по currentIteration).
			// TODO(M7): модель батчей не доделана — см. решения в ревью (AsyncValue vs Interlocked, IterationTo,
			// iterationsToSchedule, общий DAG vs per-runtime). Здесь только продвижение курсора, чтобы компилировалось.
			iteration.Run(parallelCount);
			runtime.iterations.RemoveAt(0);
			currentIteration += 1;

			return true;
		}
	}

	public struct ExecutionRuntime
	{
		public UnsafeList<ExecutionIteration> iterations;
	}

	public struct ExecutionIteration
	{
		public Id<ExecutionIteration> iterationId;

		public UnsafeList<ExecutionBatch> batches;
		public UnsafeList<Id<ExecutionBatch>> startBatches; // Могут выполнятся параллельно

		public void Run(int maxParallelCount)
		{
			// Тут должны запускаться джобы (Если не Unity, то на потоки деление).
			// + должен быть порог, при котором всё выполняется в одном потоке.
		}
	}

	public struct IterationTo
	{
		public UnsafeList<ExecutionBatch> batches;
		public UnsafeList<Id<ExecutionBatch>> startBatches;
	}

	public enum RuntimeType : byte
	{
		Unmanaged,
		Managed,
	}

	public struct ExecutionBatch
	{
		public AsyncValue<int> previousBatchesCount; // Количество батчей, от которых зависит текущий батч.

		public UnsafeList<Id<ExecutionBatch>> nextBatches; // Могут выполнятся параллельно
		public UnsafeList<NodeInstanceId> nodesOrder;
	}
}
