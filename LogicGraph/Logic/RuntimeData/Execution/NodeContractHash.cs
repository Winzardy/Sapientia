using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Sapientia.TypeIndexer;
using Unity.Burst;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// <b>«Версия кода»</b> для version gate (M6-E, развилка 7): детерминированный авто-хеш <b>контракта
	/// нод-функций</b>. Блоб (<see cref="CompiledBlueprintHeader"/>) бейкает per-node <c>NodeHeader.typeId</c> =
	/// <b>локальный ordinal</b> <c>TypeId&lt;ILogicNode&gt;</c>. Этот ordinal <b>позиционный</b>: валиден, только
	/// если набор/порядок детей <see cref="ILogicNode"/> в <see cref="IndexedTypes"/> на клиенте тот же, что был
	/// при компиляции — <b>и тело каждой ноды то же</b>. <b>Хеш — верхнеуровневая сущность контроля версий группы
	/// блюпринтов</b>: он живёт в <b>окружении группы</b> (<see cref="CompiledEnvironment"/>), <b>не</b> в каждом
	/// блобе. Гейт сверяет окружение добавляемой группы с окружением стореджа
	/// (<see cref="CompiledEnvironment.IsCompatibleWith"/>) при <see cref="CompiledBlueprintStorage"/> <c>Add</c>
	/// и <b>отклоняет</b> несовместимую группу.
	/// </summary>
	/// <remarks>
	/// <para><b>Что входит:</b> <see cref="FormatVersion"/> (ручная ABI-версия) + на каждый logic-тип (в порядке
	/// ordinal'а) три слоя: <b>имя</b> (<see cref="Type.FullName"/> — структурный: какой тип на какой позиции),
	/// <b>IL тела</b> <c>ILogicNode.Execute</c> (поведенческий: что нода делает) и <b>раскладка данных</b> (поля+размер
	/// структуры — слой данных: static-слайс реинтерпретируется как <c>T</c> через <c>NodeContext.Body&lt;T&gt;</c>,
	/// поэтому смена раскладки = тихая порча данных, даже если <c>Execute</c> не менялся и порядок не сдвигался).
	/// Имя ловит set/order/rename, IL-тело — смену поведения без переименования, раскладка — смену полей/размера
	/// (раньше всё это закрывалось только ручным <see cref="FormatVersion"/>).</para>
	/// <para><b>Ограничения IL-хеша (приняты осознанно):</b> (1) IL содержит метадата-<b>токены</b>, относительные к
	/// сборке ⇒ один исходник в <i>разных</i> сборках (server .NET ↔ client) может дать разный хеш (ложный reject);
	/// (2) Debug/Release и версия компилятора меняют IL; (3) горячий путь — <b>Burst-native</b>, а не этот IL —
	/// дрейф версии Burst хеш не ловит; (4) под <b>IL2CPP</b> <c>GetILAsByteArray()</c> == null ⇒ тело не читается
	/// (фолдится маркер, не падаем) — поэтому хеш считается <b>build-time</b>, не на девайсе; (5) <b>транзитивность</b>:
	/// изменение тела вызванного хелпера IL самого <c>Execute</c> не меняет. Робастная замена — per-node content-digest
	/// из M10-кодогена; здесь — прямой IL по выбору пользователя.</para>
	/// <para><b>FNV-1a 64</b> с фикс-константами — детерминирован в пределах сборки, без <c>Random</c>/wall-clock.
	/// <b>Запрещён <c>string.GetHashCode()</c></b> (рандомизирован per-process). Считается <b>только на managed/
	/// build-стороне</b> (сборка окружения <see cref="CompiledEnvironment.Compile"/>) — никогда на Burst-горячем пути.</para>
	/// </remarks>
	public static class NodeContractHash
	{
		/// <summary>
		/// ABI-версия контракта исполнения (<see cref="ExecuteFn"/>-сигнатура + <see cref="NodeContext"/>-раскладка).
		/// <b>Бампить вручную</b> при изменении контракта, которое не отражается в IL тел нод (напр. раскладка
		/// <see cref="NodeContext"/>). Намеренно source-level (а не <c>sizeof</c>): арх-независимо.
		/// </summary>
		public const ulong FormatVersion = 1;

		// FNV-1a 64 (фикс-константы — детерминизм через процессы/сборки/среды).
		private const ulong FnvOffsetBasis = 14695981039346656037UL;
		private const ulong FnvPrime = 1099511628211UL;

		// Маркеры «тело недоступно» (фолдятся вместо IL, чтобы не падать и оставаться детерминированными).
		private const byte BodyMarkerNotLogicNode = 0x01; // тип не реализует ILogicNode (напр. тестовый плейсхолдер)
		private const byte BodyMarkerNoMethod = 0x02;     // не нашли Execute в interface-map
		private const byte BodyMarkerNoIl = 0x03;         // GetILAsByteArray() == null (IL2CPP-стрип / abstract)
		private const byte LayoutMarkerNoSize = 0x04;     // Marshal.SizeOf бросил (не blittable / открытый generic)

		/// <summary>
		/// Хеш «версии кода» по <b>упорядоченному</b> списку logic-типов (index == ordinal <c>TypeId&lt;ILogicNode&gt;</c>).
		/// На каждый тип фолдится имя (<see cref="Type.FullName"/>) + IL тела <c>ILogicNode.Execute</c> + раскладка
		/// данных (поля/размер). Ядро — тестируется явным списком. Чувствителен к набору/порядку/идентичности типов,
		/// к телам их методов и к раскладке полей.
		/// </summary>
		public static ulong Compute(params Type[] orderedLogicTypes)
		{
			var hash = Fold(FnvOffsetBasis, FormatVersion);

			if (orderedLogicTypes == null)
				return hash;

			foreach (var type in orderedLogicTypes)
			{
				hash = Fold(hash, (byte)0xFF); // разделитель позиции в таблице
				if (type == null)
				{
					hash = Fold(hash, (byte)0x00); // не ожидается, но без NRE
					continue;
				}

				// Структурный слой: идентичность типа на этой позиции (set/order/rename).
				hash = FoldString(hash, type.FullName ?? type.Name);
				// Поведенческий слой: IL тела Execute (изменение поведения без переименования).
				hash = FoldExecuteBody(hash, type);
				// Слой данных: раскладка структуры ноды (поля/размер) — static-слайс реинтерпретируется как T,
				// смена раскладки = тихая порча данных, даже если Execute не менялся.
				hash = FoldDataLayout(hash, type);
			}

			return hash;
		}

		/// <summary>
		/// Локальная «версия кода»: хеш текущей function-table — детей <see cref="ILogicNode"/> из
		/// <see cref="IndexedTypes"/>. В EditMode (<see cref="IndexedTypes"/> не инициализирован, детей нет) →
		/// стабильный seed-хеш пустой таблицы (<c>!= 0</c>). <b>Только build/bake-сторона</b>: сборка окружения
		/// (<see cref="CompiledEnvironment.Compile"/>). В <b>рантайме</b> «версию кода» <b>загружают из конфига</b>
		/// (<see cref="CompiledEnvironment"/>), а не считают рефлексией здесь — сторедж рождается с готовым окружением.
		/// </summary>
		/// <remarks>
		/// <b>Инвариант связки (load-bearing):</b> перебор идёт по <see cref="IndexedTypes.GetContextChildren"/>
		/// в <b>том же порядке</b>, что и <see cref="NodeFunctionRegistry.Build"/> (там <c>ordinal</c> = индекс в этом
		/// же массиве, по нему раздаётся function-table, и именно его бейкает <c>NodeHeader.typeId</c>). Поэтому хеш
		/// представляет <b>реальное dispatch-index-пространство</b>. <b>Не</b> сортировать по значению <c>TypeId</c>.
		/// </remarks>
		public static ulong Local
		{
			get
			{
				var children = IndexedTypes.GetContextChildren(typeof(ILogicNode));
				if (children == null || children.Length == 0)
					return Compute(Array.Empty<Type>());

				var types = new Type[children.Length];
				for (var i = 0; i < children.Length; i++)
					types[i] = IndexedTypes.GetType(children[i]); // index == dispatch ordinal (см. инвариант связки выше)

				return Compute(types);
			}
		}

		private static ulong FoldString(ulong hash, string value)
		{
			for (var i = 0; i < value.Length; i++)
			{
				var c = value[i];
				hash = Fold(hash, (byte)c);
				hash = Fold(hash, (byte)(c >> 8));
			}
			return hash;
		}

		/// <summary>
		/// Фолдит IL тела <c>ILogicNode.Execute</c> типа <paramref name="type"/>. Если тело недоступно (тип не
		/// реализует <see cref="ILogicNode"/>, абстракция, или IL2CPP-стрип) — фолдит стабильный маркер (не падает).
		/// </summary>
		private static ulong FoldExecuteBody(ulong hash, Type type)
		{
			hash = Fold(hash, (byte)0xFE); // разделитель имя↔тело

			if (type.IsInterface || type.IsAbstract || !typeof(ILogicNode).IsAssignableFrom(type))
				return Fold(hash, BodyMarkerNotLogicNode);

			var method = ResolveExecute(type);
			if (method == null)
				return Fold(hash, BodyMarkerNoMethod);

			var il = method.GetMethodBody()?.GetILAsByteArray();
			if (il == null)
				return Fold(hash, BodyMarkerNoIl); // IL2CPP / нет IL — хеш считается build-time, так что норм

			hash = Fold(hash, (ulong)il.Length); // длина — дешёвый дискриминатор
			foreach (var b in il)
				hash = Fold(hash, b);
			return hash;
		}

		/// <summary>
		/// Фолдит <b>раскладку данных</b> структуры ноды: размер (<see cref="Marshal.SizeOf(Type)"/>) + поля
		/// инстанса (имя + <see cref="Type.FullName"/> типа поля) в стабильном порядке (по <c>MetadataToken</c> =
		/// порядок объявления). Ловит add/remove/reorder/retype поля. <b>Не рекурсивно</b> (вложенная структура,
		/// сменившая поля при том же имени/размере — тот же gap, что у IL-транзитивности; закроет M10-кодоген).
		/// </summary>
		private static ulong FoldDataLayout(ulong hash, Type type)
		{
			hash = Fold(hash, (byte)0xFD); // разделитель тело↔layout

			// Размер — грубый дискриминатор (retype/добавление поля со сменой размера, explicit-layout).
			try
			{
				hash = Fold(hash, (ulong)(uint)Marshal.SizeOf(type));
			}
			catch (Exception)
			{
				hash = Fold(hash, LayoutMarkerNoSize); // не blittable / не размечается — маркер, не падаем
			}

			// Поля инстанса (public + non-public) в стабильном порядке: имя + тип. Сортировка по MetadataToken
			// (== порядок объявления, стабилен в пределах сборки) ⇒ reorder полей меняет последовательность фолда.
			var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			Array.Sort(fields, (a, b) => a.MetadataToken.CompareTo(b.MetadataToken));
			foreach (var field in fields)
			{
				hash = Fold(hash, (byte)0xFC); // разделитель поля
				hash = FoldString(hash, field.Name);
				hash = FoldString(hash, field.FieldType.FullName ?? field.FieldType.Name);
			}

			return hash;
		}

		/// <summary>
		/// Резолвит реализацию <c>ILogicNode.Execute</c> у <paramref name="type"/> через interface-map (покрывает и
		/// implicit, и explicit реализацию). <c>null</c>, если не нашли.
		/// </summary>
		private static MethodInfo ResolveExecute(Type type)
		{
			var map = type.GetInterfaceMap(typeof(ILogicNode));
			for (var i = 0; i < map.InterfaceMethods.Length; i++)
			{
				if (map.InterfaceMethods[i].Name == nameof(ILogicNode.Execute))
					return map.TargetMethods[i];
			}
			return null;
		}

		private static ulong Fold(ulong hash, byte b)
		{
			return (hash ^ b) * FnvPrime;
		}

		private static ulong Fold(ulong hash, ulong value)
		{
			// 8 байт LE — порядок фиксирован ради кросс-средового детерминизма.
			for (var i = 0; i < 8; i++)
			{
				hash = Fold(hash, (byte)value);
				value >>= 8;
			}
			return hash;
		}
	}
}
