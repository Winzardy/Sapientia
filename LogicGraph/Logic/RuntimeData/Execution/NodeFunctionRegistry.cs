using System;
using Sapientia.TypeIndexer;
#if UNITY_5_3_OR_NEWER
using Sapientia.Collections;
using Unity.Burst;
#endif

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// <b>Function-table нод по индексу</b> <c>TypeId&lt;ILogicNode&gt;</c> (M6-C): то, что <see cref="NodeInvoker"/>
	/// (M6-B) делал для <b>одного</b> типа (<see cref="NodeInvoker.Compile{T}"/>/<see cref="NodeInvoker.GetManaged{T}"/>),
	/// собрано в плоскую таблицу, заполняемую <b>один раз</b> (Burst-компиляция fn-pointer'а — раз/тип, не на каждый run).
	/// </summary>
	/// <remarks>
	/// <para><b>Инстанс, не статика.</b> Реестр — обычный инстанс (без <c>SharedStatic</c>: тот капризен в эдиторе —
	/// domain reload / межсессионное состояние). Строится один раз и <b>передаётся в исполнение</b> (владелец — тот, кто
	/// гоняет домен исполнения; в <see cref="ExecutionScope"/> прокидывается на M6-F). Кэш = сам инстанс (caller строит
	/// раз и шарит копией — как <c>CompiledBlueprintStorage</c> «передаётся, не хранит»). Копия по значению разделяет
	/// нижележащие таблицы (указатель/ссылку) ⇒ диспозит только владелец.</para>
	/// <para><b>Две таблицы.</b> Burst-таблица — <see cref="UnsafeArray{T}"/> от <see cref="FunctionPointer{T}"/>
	/// (под <c>#if UNITY</c>): <b>off-allocator unmanaged</b>, поэтому передаётся и читается <b>из Burst</b>
	/// (managed-массив Burst читать не может; <see cref="FunctionPointer{T}"/> — <c>readonly struct</c> над <c>IntPtr</c>,
	/// unmanaged). Managed-таблица — <see cref="_managed"/> (<see cref="ExecuteFn"/>-делегаты, managed-массив): путь
	/// .NET/fallback, <b>Burst её не трогает</b>. Население managed-таблицы — опционально (<see cref="Build"/>
	/// <c>buildManaged</c>): под Unity её можно пропустить для <b>Unmanaged</b>-нод (их покрывает Burst); для
	/// <b>Managed</b>-нод managed-делегат строится всегда (Burst-skip — единственный путь). <b>Выбор</b> таблицы по
	/// <c>NodeHeader.runtimeType</c> — <see cref="UseManaged"/>/<see cref="Invoke"/> (M6-D); прогон <c>Drain</c> — M6-F.</para>
	/// <para><b>Индекс — плотный ordinal</b> <c>TypeId&lt;ILogicNode&gt;</c> (== <c>NodeHeader.typeId</c>, бейканный в блоб
	/// на компиляции). Источник типов — уже построенные генератором <c>TypeIndexer</c> <b>дети контекста</b> <c>ILogicNode</c>
	/// (<see cref="IndexedTypes.GetContextChildren"/>, упорядочены по этому ordinal'у) ⇒ совпадение индекса с блобом
	/// гарантировано конструктивно, отдельный AppDomain-скан не нужен.</para>
	/// <para><b>Освобождение.</b> Сами <see cref="FunctionPointer{T}"/> диспозить не нужно (нативный код держит Burst
	/// на время процесса). Но <b>off-allocator</b> <see cref="UnsafeArray{T}"/> Burst-таблицы обязан освобождаться —
	/// это делает <see cref="Dispose"/> (вызывает владелец).</para>
	/// <para><b>Интерим (до кодогена M10).</b> Сборка идёт через reflection (<c>MakeGenericMethod</c>) — работает в
	/// редакторе (Mono) и plain .NET (JIT). Под <b>IL2CPP/AOT</b> инстанциация <c>Compile&lt;T&gt;</c>/<c>GetManaged&lt;T&gt;</c>
	/// для типа, не упомянутого статически, может быть вырезана линкером ⇒ нужен генерируемый initializer (M10, как у
	/// <c>TypeIndexer</c>) либо <c>[Preserve]</c>/link.xml. Не блокер верификации M6-C (редактор/.NET).</para>
	/// </remarks>
	public struct NodeFunctionRegistry : IDisposable
	{
		// Managed-таблица (universal-путь / .NET): индекс = TypeId<ILogicNode> ordinal. Под Unity при buildManaged:false
		// заполнены только Managed-ноды (у Unmanaged Burst покрывает путь), слоты прочих — null.
		private ExecuteFn[] _managed;
#if UNITY_5_3_OR_NEWER
		// Burst-таблица (нативные fn-pointer'ы): off-allocator UnsafeArray ⇒ передаётся/читается из Burst. Индекс = ordinal.
		private UnsafeArray<FunctionPointer<ExecuteFn>> _burst;
#endif
		// Глобальный managed-форс (M6-D): true ⇒ Invoke всегда идёт managed-путём независимо от per-node runtimeType
		// (требует населённой managed-таблицы). Свойство инстанса — шарится копией вместе с таблицами.
		private bool _forceManaged;
		private bool _isCreated;

		public readonly bool IsCreated => _isCreated;

		/// <summary>Глобальный managed-форс: при <c>true</c> <see cref="Invoke"/>/<see cref="UseManaged"/> всегда выбирают
		/// managed-путь (для .NET/сервера и A/B-проверки детерминизма). Ставится в <see cref="Build"/>/<see cref="Create"/>.</summary>
		public readonly bool ForceManaged => _forceManaged;

		/// <summary>
		/// Строит таблицу по плотным детям <see cref="ILogicNode"/> из <see cref="IndexedTypes"/> (интерим до кодогена
		/// M10). Вызывается <b>один раз</b> владельцем; результат шарится копией. Если <see cref="IndexedTypes"/> не
		/// инициализирован (нет детей — напр. EditMode) → пустая (но созданная) таблица.
		/// </summary>
		/// <param name="buildManaged">Заполнять ли managed-делегат для <b>Unmanaged</b>-нод. Под Unity <c>false</c>
		/// пропускает их (Burst покрывает unmanaged-ноды). Для <b>Managed</b>-нод managed-делегат строится
		/// <b>всегда</b> (единственный путь). В чистом .NET managed — единственный путь у всех; флаг соблюдается честно
		/// (<c>false</c> ⇒ заполнены только Managed-ноды).</param>
		/// <param name="forceManaged">Глобальный managed-форс (см. <see cref="ForceManaged"/>). При <c>true</c>
		/// managed-делегаты нужны для всех нод ⇒ <c>buildManaged</c> де-факто принудительно <c>true</c>.</param>
		public static NodeFunctionRegistry Build(bool buildManaged = true, bool forceManaged = false)
		{
			// При форсе managed-таблица нужна целиком (Invoke всегда идёт managed) ⇒ заполняем managed для всех.
			buildManaged |= forceManaged;

			// Дети контекста ILogicNode — TypeId[], упорядоченные по плотному ordinal'у TypeId<ILogicNode>.
			var children = IndexedTypes.GetContextChildren(typeof(ILogicNode));
			var count = children.Length;

			var registry = new NodeFunctionRegistry
			{
				// Аллоцируем при count>0 ВСЕГДА: у Managed-нод должен быть managed-слот даже при buildManaged:false.
				_managed = (count > 0) ? new ExecuteFn[count] : Array.Empty<ExecuteFn>(),
				_forceManaged = forceManaged,
				_isCreated = true,
			};
#if UNITY_5_3_OR_NEWER
			registry._burst = (count > 0) ? new UnsafeArray<FunctionPointer<ExecuteFn>>(default, count) : default;
#endif

			if (count == 0)
				return registry;

			// MethodInfo адаптеров — один раз вне цикла (open generic definitions). Гард: метод обязан существовать
			// и быть единственным (overload сломал бы AmbiguousMatch / переименование — null). Дешёвая страховка на
			// интерим-reflection-пути (M10-кодоген уберёт reflection целиком).
			var getManagedMethod = typeof(NodeInvoker).GetMethod(nameof(NodeInvoker.GetManaged));
			if (getManagedMethod == null)
				throw new InvalidOperationException("[NodeFunctionRegistry] NodeInvoker.GetManaged не найден (рефактор адаптера?).");
#if UNITY_5_3_OR_NEWER
			var compileMethod = typeof(NodeInvoker).GetMethod(nameof(NodeInvoker.Compile));
			if (compileMethod == null)
				throw new InvalidOperationException("[NodeFunctionRegistry] NodeInvoker.Compile не найден (рефактор адаптера?).");
#endif

			for (var ordinal = 0; ordinal < count; ordinal++)
			{
				var type = IndexedTypes.GetType(children[ordinal]);
				// Capability logic-тела (M6-D): managed-тело нельзя Burst-компилировать ⇒ Burst-skip. Boxing default-инстанса
				// — на сборке (раз/тип, managed-путь), не на горячем пути. Дефолт DIM = Unmanaged.
				var isManaged = ((ILogicNode)Activator.CreateInstance(type)).RuntimeType == RuntimeType.Managed;
#if UNITY_5_3_OR_NEWER
				// Burst компилим только для Unmanaged: CompileFunctionPointer на managed-теле упал бы. Managed → _burst[ordinal]=default.
				if (!isManaged)
					registry._burst[ordinal] = (FunctionPointer<ExecuteFn>)compileMethod.MakeGenericMethod(type).Invoke(null, null);
#endif
				// Managed-делегат: для Managed-нод — всегда (единственный путь), для Unmanaged — по флагу buildManaged.
				if (buildManaged || isManaged)
					registry._managed[ordinal] = (ExecuteFn)getManagedMethod.MakeGenericMethod(type).Invoke(null, null);
			}

			return registry;
		}

		/// <summary>
		/// Сборка из <b>уже построенной</b> таблицы (тесты + будущий генерируемый initializer M10). Реестр <b>владеет</b>
		/// собственной копией Burst-таблицы (off-allocator) ⇒ освобождается в <see cref="Dispose"/>.
		/// </summary>
		public static NodeFunctionRegistry Create(ExecuteFn[] managed
#if UNITY_5_3_OR_NEWER
			, FunctionPointer<ExecuteFn>[] burst = null
#endif
			, bool forceManaged = false
		)
		{
			var registry = new NodeFunctionRegistry
			{
				_managed = managed ?? Array.Empty<ExecuteFn>(),
				_forceManaged = forceManaged,
				_isCreated = true,
			};
#if UNITY_5_3_OR_NEWER
			if (burst != null && burst.Length > 0)
			{
				registry._burst = new UnsafeArray<FunctionPointer<ExecuteFn>>(default, burst.Length);
				for (var i = 0; i < burst.Length; i++)
					registry._burst[i] = burst[i];
			}
#endif
			return registry;
		}

		/// <summary>Адаптер ноды managed-путём по ordinal'у <c>TypeId&lt;ILogicNode&gt;</c> (универсальный путь / .NET).</summary>
		public readonly ExecuteFn GetManaged(int ordinal)
		{
			return _managed[ordinal];
		}

		/// <summary>Managed-таблица делегатов целиком — передаётся в managed-диспетчер (<see cref="NodeInvoker.InvokeManaged"/>)
		/// как аргумент, симметрично <see cref="BurstTable"/>.</summary>
		public readonly ExecuteFn[] ManagedTable => _managed;

#if UNITY_5_3_OR_NEWER
		/// <summary>Адаптер ноды Burst-путём (нативный fn-pointer) по ordinal'у <c>TypeId&lt;ILogicNode&gt;</c>.</summary>
		public readonly FunctionPointer<ExecuteFn> GetBurst(int ordinal)
		{
			return _burst[ordinal];
		}

		/// <summary>Off-allocator Burst-таблица целиком — передаётся в Burst-диспетчер (M6-F) как аргумент/в контекст.</summary>
		public readonly UnsafeArray<FunctionPointer<ExecuteFn>> BurstTable => _burst;
#endif

		/// <summary>
		/// <b>Выбор бэкенда</b> (M6-D, развилка 5) для ноды с per-node <paramref name="runtimeType"/> (из
		/// <see cref="NodeHeader.runtimeType"/>). Под Unity: managed, если <see cref="ForceManaged"/> или
		/// нода <see cref="RuntimeType.Managed"/>; иначе Burst. В чистом .NET — <b>всегда</b> managed
		/// (Burst-таблица вырезана <c>#if</c>). Решение принимает managed-glue (<see cref="NodeInvoker.Invoke"/>)
		/// один раз на ноду и зовёт нужную таблицу-специфичную точку (<see cref="NodeInvoker.InvokeBurst"/>/
		/// <see cref="NodeInvoker.InvokeManaged"/>) — внутри самих точек развилки уже нет. Чередование Burst↔Managed
		/// pass (wave) — M7.
		/// </summary>
		public readonly bool UseManaged(RuntimeType runtimeType)
		{
#if UNITY_5_3_OR_NEWER
			return _forceManaged || runtimeType == RuntimeType.Managed;
#else
			return true;
#endif
		}

		/// <summary>Освобождает off-allocator Burst-таблицу. Вызывает владелец. Идемпотентно.</summary>
		public void Dispose()
		{
			_managed = Array.Empty<ExecuteFn>();
#if UNITY_5_3_OR_NEWER
			if (_burst.IsCreated)
				_burst.Dispose();
			_burst = default;
#endif
			_isCreated = false;
		}
	}
}
