namespace SharedLogic
{
	public interface ICommandRunner
	{
		// TODO: имеет смысл переназвать метод в Schedule, Submit или что-то более подходящее
		/// <returns>
		/// <c>true</c> — команда прошла валидацию и успешно добавлена в буфер на исполнение<br/>
		/// <c>false</c> — валидация провалена
		/// </returns>
		/// <remarks>
		/// Команда может быть выполнена асинхронно или с задержкой.
		/// Возврат <c>true</c> означает только факт добавления в буфер
		/// и не отражает результат её выполнения.
		/// <para/>
		/// ⚠️ Не используйте возвращаемое значение для проверки успешности выполнения самой команды
		/// </remarks>
		bool Execute<T>(in T command) where T : struct, ICommand;
		bool IsEmpty { get; }

		/// <summary>
		/// Такая же команда уже принята, но ещё не исполнена. Сравнение через
		/// <see cref="System.Collections.Generic.EqualityComparer{T}.Default"/>, поэтому команде стоит
		/// реализовать <see cref="System.IEquatable{T}"/>. У синхронного раннера очереди нет
		/// </summary>
		bool HasPending<T>(in T command) where T : struct, ICommand => false;
	}

	public abstract class CommandRunnerDecorator : ICommandRunner
	{
		private readonly ICommandRunner _runner;

		protected CommandRunnerDecorator(ICommandRunner runner)
		{
			_runner = runner;
		}

		public virtual bool Execute<T>(in T command) where T : struct, ICommand
		{
			return _runner.Execute(in command);
		}

		public virtual bool IsEmpty => _runner.IsEmpty;

		public virtual bool HasPending<T>(in T command) where T : struct, ICommand
		{
			return _runner.HasPending(in command);
		}
	}
}
