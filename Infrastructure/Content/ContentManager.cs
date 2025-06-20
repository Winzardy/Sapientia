using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia;

namespace Content
{
	using Management;

	// ReSharper disable once ClassNeverInstantiated.Global
	public sealed class ContentManager : StaticProvider<ContentResolver>
	{
#if UNITY_EDITOR
		public static IContentEditorResolver editorResolver;
		private static IContentEditorResolver _resolver
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _instance ?? editorResolver;
		}
#else
		private static ContentResolver _resolver
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _instance;
		}
#endif
		/// <inheritdoc cref="ContentResolver.Any{T}"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Any<T>() => _resolver.Any<T>();

		/// <inheritdoc cref="ContentResolver.Contains{T}()"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<T>(in SerializableGuid guid) => _resolver.Contains<T>(in guid);

		/// <inheritdoc cref="ContentResolver.Contains{T}"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<T>(string id) => _resolver.Contains<T>(id);

		/// <inheritdoc cref="ContentResolver.Contains{T}(int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<T>(int index) => _resolver.Contains<T>(index);

		/// <inheritdoc cref="ContentResolver.Contains{T}()"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<T>() => _resolver.Contains<T>();

		/// <inheritdoc cref="ContentResolver.GetEntry{T}(in SerializableGuid)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UniqueContentEntry<T> GetEntry<T>(in SerializableGuid guid) => _resolver.GetEntry<T>(in guid);

		/// <inheritdoc cref="ContentResolver.GetEntry{T}(string)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UniqueContentEntry<T> GetEntry<T>(string id) => _resolver.GetEntry<T>(id);

		/// <inheritdoc cref="ContentResolver.GetEntry{T}(int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UniqueContentEntry<T> GetEntry<T>(int index) => _resolver.GetEntry<T>(index);

		/// <inheritdoc cref="ContentResolver.GetEntry{T}()"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SingleContentEntry<T> GetEntry<T>() => _resolver.GetEntry<T>();

		/// <inheritdoc cref="ContentResolver.Get{T}(in SerializableGuid)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Get<T>(in SerializableGuid guid) => ref _resolver.Get<T>(in guid);

		/// <inheritdoc cref="ContentResolver.Get{T}(string)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Get<T>(string id) => ref _resolver.Get<T>(id);

		/// <inheritdoc cref="ContentResolver.Get{T}(int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Get<T>(int index) => ref _resolver.Get<T>(index);

		/// <inheritdoc cref="ContentResolver.Get{T}()"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Get<T>() => ref _resolver.Get<T>();

		/// <inheritdoc cref="ContentResolver.Get{T}(in SerializableGuid)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Get<T>(in SerializableGuid guid, out T value) => value = _resolver.Get<T>(in guid);

		/// <inheritdoc cref="ContentResolver.Get{T}(string)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Get<T>(string id, out T value) => value = _resolver.Get<T>(id);

		/// <inheritdoc cref="ContentResolver.Get{T}(int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Get<T>(int index, out T value) => value = _resolver.Get<T>(index);

		/// <inheritdoc cref="ContentResolver.Get{T}()"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Get<T>(out T value) => value = _resolver.Get<T>();

		/// <inheritdoc cref="ContentResolver.GetAll{T}"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<IContentEntry<T>> GetAll<T>() => _resolver.GetAll<T>();

		/// <inheritdoc cref="ContentResolver.ToId{T}(in SerializableGuid)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToId<T>(in SerializableGuid guid) => _resolver != null ? _resolver.ToId<T>(in guid) : guid.ToString();

		/// <inheritdoc cref="ContentResolver.ToGuid{T}(string)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly SerializableGuid ToGuid<T>(string id)
		{
#if UNITY_EDITOR
			try
			{
				return ref _resolver.ToGuid<T>(id);
			}
			catch (System.Exception e)
			{
				ContentDebug.LogWarning(e.Message);
			}

			return ref SerializableGuid.Empty;
#endif
			return ref _resolver.ToGuid<T>(id);
		}

		/// <inheritdoc cref="ContentResolver.ToLabel{T}(in SerializableGuid, bool)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToLabel<T>(in SerializableGuid guid, bool verbose = false)
			=> _resolver.ToLabel<T>(in guid, verbose);

		#region Registation

		/// <summary>
		/// Регистрация уникального контента типа <typeparamref name="T"/> (запись с guid'ом)
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Register<T>(UniqueContentEntry<T> entry) => ContentEntryMap<T>.Register(entry);

		/// <summary>
		/// Удаление уникального контента типа <typeparamref name="T"/> (запись с guid'ом)
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Unregister<T>(UniqueContentEntry<T> entry) => ContentEntryMap<T>.Unregister(entry);

		/// <summary>
		/// Регистрация контента типа <typeparamref name="T"/> (одиночная запись)
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Register<T>(SingleContentEntry<T> entry) => SingleContentEntryShortcut<T>.Register(entry);

		/// <summary>
		/// Удаление контента типа <typeparamref name="T"/> (одиночная запись)
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Unregister<T>() => SingleContentEntryShortcut<T>.Unregister();

		#endregion
	}
}
