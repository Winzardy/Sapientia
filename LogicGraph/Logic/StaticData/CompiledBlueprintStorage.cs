using System;
using Sapientia.Collections;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.Memory;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Хранилище («БД») скомпилированных блюпринтов. <b>Ничего не знает об authoring-<c>Blueprint</c> и о
	/// компиляции</b> — на вход приходят уже готовые <see cref="CompiledBlueprintHeader"/> вместе со своими
	/// аренами (<see cref="Add"/>); компиляция — забота вызывающего
	/// (<see cref="CompiledBlueprintHeader.CompileLayout"/>). Адресация версии — по <c>(Id&lt;Blueprint&gt;,
	/// version)</c>, которые сторедж читает из самого <see cref="CompiledBlueprintHeader"/>.
	///
	/// <b>Ничего не удаляется по одной</b>: версии только добавляются (сосуществуют), ресурсы освобождаются
	/// единым <see cref="Dispose"/> на выходе из приложения/PlayMode.
	///
	/// <b>Раскладка (jump-by-id):</b> <see cref="_blueprints"/> индексируется прямо по
	/// <see cref="Id{Blueprint}"/>; <see cref="RootSlot"/> держит текущую версию инлайн (1 jump) и список
	/// старых (<see cref="RootSlot.next"/>). Арены лежат отдельным списком <see cref="_arenas"/> (по батчам);
	/// слот ссылается по индексу (<see cref="Slot.arenaId"/>), а не держит арену. Off-allocator, в снапшот
	/// мира не идёт. <b>Add передаёт владение ареной стореджу</b> (освобождается на <see cref="Dispose"/>).
	/// </summary>
	public struct CompiledBlueprintStorage : IDisposable
	{
		private struct Slot
		{
			public Id version;
			public Id<RawBumpAllocator> arenaId; // индекс в _arenas
			public PtrOffset<CompiledBlueprintHeader> offset;
		}

		private struct RootSlot
		{
			public Slot slot;				// текущая (последняя) версия — инлайн, 1 jump
			public bool HasSlot => slot.version.IsValid;

			public UnsafeList<Slot> next;	// более старые версии (лениво создаётся)

			public bool HasVersion(Id version)
			{
				if (slot.version == version)
					return true;
				if (!next.IsCreated)
					return false;
				var existing = next.GetSpan();
				for (var i = 0; i < existing.Length; i++)
				{
					if (existing[i].version != version)
						continue;
					return true;
				}
				return false;
			}
		}

		// Арены-батчи: каждый Add кладёт сюда свою RawBumpAllocator. Индивидуально не освобождаем.
		private UnsafeList<RawBumpAllocator> _arenas;
		// Индексируется прямо по (int)Id<Blueprint>; растёт при новом id.
		private UnsafeList<RootSlot> _blueprints;

		public bool IsCreated => _arenas.IsCreated;
		// Число арен (по одной на каждый Add/батч). При одном compiled на арену = числу compiled.
		public int Count => _arenas.count;

		public static CompiledBlueprintStorage Create(int blueprintCapacity = 8)
		{
			return new CompiledBlueprintStorage
			{
				_arenas = new UnsafeList<RawBumpAllocator>(blueprintCapacity > 0 ? blueprintCapacity : 1),
				_blueprints = new UnsafeList<RootSlot>(blueprintCapacity > 0 ? blueprintCapacity : 1),
			};
		}

		/// <summary>
		/// Удобная перегрузка для одной скомпилированной ноды-блюпринта: оборачивает <paramref name="offset"/>
		/// в span из одного элемента и зовёт батчевый <see cref="Add(RawBumpAllocator, Span{PtrOffset{CompiledBlueprintHeader}})"/>.
		/// </summary>
		public void Add(RawBumpAllocator arena, PtrOffset<CompiledBlueprintHeader> offset)
		{
			Add(arena, offset.AsSpan());
		}

		/// <summary>
		/// Принимает арену-<b>батч</b>: одна <paramref name="arena"/> с одним или несколькими
		/// <see cref="CompiledBlueprintHeader"/> по <paramref name="offsets"/>. Для каждого <c>(id, version)</c>
		/// читаются из самого блоба; версии с невалидным <c>version</c> пропускаются; дубли по
		/// <c>(id, version)</c> пропускаются. Новая версия блюпринта становится текущей, прежняя уходит в
		/// список старых (продолжает жить). Арена кладётся в <see cref="_arenas"/>, если добавлен хотя бы
		/// один блюпринт; иначе (всё дубли/невалидно) — освобождается. <b>Владение ареной переходит стореджу.</b>
		/// </summary>
		public void Add(RawBumpAllocator arena, Span<PtrOffset<CompiledBlueprintHeader>> offsets)
		{
			var maxBlueprintId = default(Id<Blueprint>);
			foreach (var offset in offsets)
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				if (maxBlueprintId < compiled.blueprintKey.id)
					maxBlueprintId = compiled.blueprintKey.id;
			}

			// +1: индекс = (int)blueprintId доходит до maxBlueprintId, значит нужен count = max + 1.
			_blueprints.EnsureCount(maxBlueprintId + 1);

			var addedBlueprintCount = 0;
			var arenaId = _arenas.count;
			foreach (var offset in offsets)
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				var blueprintId = compiled.blueprintKey.id;
				var version = compiled.blueprintKey.version;

				if (!version.IsValid)
					continue;

				ref var root = ref _blueprints[blueprintId];

				// Дедуп: такая версия уже есть (текущая или старая)? Входную арену освобождаем (владелец — сторедж).
				if (root.HasVersion(version))
					continue;

				var newSlot = new Slot
				{
					version = version,
					arenaId = arenaId,
					offset = offset,
				};

				if (root.HasSlot)
				{
					if (!root.next.IsCreated)
						root.next = new UnsafeList<Slot>();
					root.next.Add(root.slot);
				}
				root.slot = newSlot;

				addedBlueprintCount++;
			}

			if (addedBlueprintCount > 0)
				_arenas.Add(arena);
			else
				arena.Dispose();
		}

		/// <summary>Есть ли в хранилище версия <paramref name="blueprintId"/> (текущая или старая).</summary>
		public bool Has(VersionedId<Blueprint> blueprintId)
		{
			if (!blueprintId.id.IsValid || blueprintId.id >= _blueprints.count)
				return false;

			ref var root = ref _blueprints[blueprintId.id];
			return root.HasVersion(blueprintId.version);
		}

		/// <summary>Доступ к static-данным версии (jump-by-id + walk по старым). Вызывать на существующей
		/// <paramref name="blueprintId"/> (см. <see cref="Has"/>); DEBUG-assert ловит обращение к несуществующей.</summary>
		public ref CompiledBlueprintHeader Get(VersionedId<Blueprint> blueprintId)
		{
			ref var root = ref _blueprints[blueprintId.id];

			// Быстрый путь — текущая версия (1 jump).
			if (root.HasSlot && root.slot.version == blueprintId.version)
				return ref ResolveSlot(root.slot);

			// Иначе — среди старых.
			if (root.next.IsCreated)
			{
				var span = root.next.GetSpan();
				for (var i = 0; i < span.Length; i++)
				{
					if (span[i].version == blueprintId.version)
						return ref ResolveSlot(span[i]);
				}
			}

			E.ASSERT(false, "[CompiledBlueprintStorage] Get на несуществующем blueprintId (VersionedId).");
			// Недостижимо в DEBUG (assert бросает); контракт release — вызывать на существующей версии.
			return ref ResolveSlot(root.slot);
		}

		public void Dispose()
		{
			for (var i = 0; i < _arenas.count; i++)
			{
				ref var arena = ref _arenas[i];
				arena.Dispose();
			}
			_arenas.Dispose();

			for (var i = 0; i < _blueprints.count; i++)
			{
				ref var root = ref _blueprints[i];
				root.next.Dispose();
			}
			_blueprints.Dispose();

			this = default;
		}

		private ref CompiledBlueprintHeader ResolveSlot(Slot slot)
		{
			return ref _arenas[slot.arenaId].Value.GetValue(slot.offset);
		}
	}
}
