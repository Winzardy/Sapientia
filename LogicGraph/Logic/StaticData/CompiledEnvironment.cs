namespace Sapientia.LogicGraph
{
	/// <summary>
	/// <b>Скомпилированное окружение</b> группы блюпринтов (version gate, M6-E). Несёт «версию кода» —
	/// <see cref="contractHash"/> (<see cref="NodeContractHash"/>): контракт нод-функций, под который собрана группа.
	/// Это <b>верхнеуровневая сущность контроля версий группы блюпринтов</b> — версия живёт <b>здесь</b>, а <b>не</b>
	/// в каждом блобе (<see cref="CompiledBlueprintHeader"/> о хеше ничего не знает). Позиционные ordinal'ы
	/// <c>NodeHeader.typeId</c> у блобов группы валидны ровно при совпадении этой версии у источника и приёмника.
	/// <b>Компилируется заранее</b> (build-time, вместе с блюпринтами) и <b>загружается</b> в рантайме — версия
	/// <b>не</b> вычисляется рефлексией на исполнении.
	/// </summary>
	/// <remarks>
	/// <see cref="CompiledBlueprintStorage"/> рождается со своим окружением (версия, которую ждёт рантайм). При
	/// добавлении группы (<c>Add</c>) ей передаётся <b>её</b> окружение; сторедж сверяет его со своим
	/// (<see cref="IsCompatibleWith"/>) и отклоняет несовместимую группу. Минимальный носитель: сейчас только
	/// «версия кода»; сюда же позже лягут прочие настройки окружения и сериализация в/из конфига (M11/конфиг-слой).
	/// Build-сторона использует <see cref="Compile"/>; рантайм конструирует из загруженного значения.
	/// </remarks>
	public struct CompiledEnvironment
	{
		/// <summary>«Версия кода» группы (контракт нод-функций). Сверяется между окружением группы и окружением
		/// стореджа при <c>Add</c> (<see cref="IsCompatibleWith"/>).</summary>
		public ulong contractHash;

		public CompiledEnvironment(ulong contractHash)
		{
			this.contractHash = contractHash;
		}

		/// <summary>
		/// <b>Сборка окружения на компиляции</b> (build-time): фиксирует текущую «версию кода»
		/// (<see cref="NodeContractHash.Local"/> — хеш function-table). Результат сериализуется в конфиг и грузится
		/// в рантайме. <b>Не</b> вызывать на горячем пути исполнения — это build/bake-сторона.
		/// </summary>
		public static CompiledEnvironment Compile()
		{
			return new CompiledEnvironment(NodeContractHash.Local);
		}

		/// <summary>
		/// Version gate (M6-E): совместимы ли два окружения — равенство «версий кода»
		/// (<see cref="contractHash"/>). Несовпадение ⇒ группы собраны под разный набор/порядок нод-функций,
		/// их ordinal'ы укажут на чужие функции ⇒ смешивать <b>нельзя</b>.
		/// </summary>
		public readonly bool IsCompatibleWith(in CompiledEnvironment other)
		{
			return contractHash == other.contractHash;
		}
	}
}
