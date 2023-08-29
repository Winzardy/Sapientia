using System;
using System.Collections.Generic;

namespace Sapientia.Data.Events
{
	public class ComplexAction : AsyncClass
	{
		public bool IsOneShot { get; private set; }

		private event Action ActionEvent;
		private Dictionary<object, (int executed, int executionRequired)> _executorToExecutionState = new();

		private int _executed;
		private int _executionRequired;

		public ComplexAction(bool isOneShot = false)
		{
			IsOneShot = isOneShot;
		}

		public void Subscribe(Action action)
		{
			if (_executorToExecutionState == null)
				action?.Invoke();
			else
				ActionEvent += action;
		}

		public void UnSubscribe(Action action)
		{
			ActionEvent -= action;
		}

		public void AddExecutors(params ActionContainer<object>[] executors)
		{
			using var scope = GetAsyncBusyScope();

			if (_executorToExecutionState == null)
				return;

			foreach (var executor in executors)
			{
				_executorToExecutionState.TryGetValue(executor, out var executionState);
				executionState.executionRequired++;
				_executorToExecutionState[executor] = executionState;

				_executionRequired++;
			}
			foreach (var executor in executors)
			{
				executor.Subscribe(OnExecutorActed);
			}
		}

		public void AddExecutor(ActionContainer<object> executor)
		{
			using var scope = GetAsyncBusyScope();

			if (_executorToExecutionState == null)
				return;

			_executorToExecutionState.TryGetValue(executor, out var executionState);
			executionState.executionRequired++;
			_executorToExecutionState[executor] = executionState;

			_executionRequired++;
			executor.Subscribe(OnExecutorActed);
		}

		public void RemoveExecutor(ActionContainer<object> executor)
		{
			using var scope = GetAsyncBusyScope();
			if (_executorToExecutionState == null)
				return;

			if (_executorToExecutionState.TryGetValue(executor, out var executionState))
			{
				executionState.executionRequired--;
				_executionRequired--;
				if (executionState.executed > executionState.executionRequired)
				{
					executionState.executed--;
					_executed--;
				}
				if (executionState.executionRequired > 0)
					_executorToExecutionState[executor] = executionState;
				else
					_executorToExecutionState.Remove(executor);

				executor.UnSubscribe(OnExecutorActed);
			}
		}

		private void OnExecutorActed(object executorKey)
		{
			using var scope = GetAsyncBusyScope();
			if (_executorToExecutionState == null)
				return;
			if (!_executorToExecutionState.TryGetValue(executorKey, out var executionState))
				return;
			if (executionState.executed >= executionState.executionRequired)
				return;

			_executed++;
			if (_executed < _executionRequired)
			{
				executionState.executed++;
				_executorToExecutionState[executorKey] = executionState;
				return;
			}

			ActionEvent?.Invoke();
			if (IsOneShot)
			{
				foreach (var (key, value) in _executorToExecutionState)
				{
					var executor = (ActionContainer<object>)key;
					executor.UnSubscribe(OnExecutorActed);
				}
				_executorToExecutionState = null;
				ActionEvent = null;
			}
			else
			{
				foreach (var (key, value) in _executorToExecutionState)
				{
					var newValue = value;
					newValue.executed = 0;
					_executorToExecutionState[key] = newValue;
				}
			}
		}
	}
}