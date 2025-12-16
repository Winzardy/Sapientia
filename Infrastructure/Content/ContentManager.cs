using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Sapientia;

namespace Content
{
	using Management;

	// ReSharper disable once ClassNeverInstantiated.Global
	public sealed class ContentManager : StaticProvider<ContentResolver>
	{
#if UNITY_EDITOR

		public static IContentEditorResolver editorResolver;
		private static IContentEditorResolver resolver
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _instance ?? editorResolver;
		}
#else
		private static ContentResolver resolver
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _instance;
		}
#endif
		internal static bool IsInitialized => resolver != null;

		/// <summary>
		///
		/// </summary>
		/// <param name="importer"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Task PopulateAsync(IContentImporter importer, CancellationToken token = default)
			=> resolver.PopulateAsync(importer, token);

		/// <inheritdoc cref="ContentResolver.Any{T}"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Any<T>() => resolver.Any<T>();

		/// <inheritdoc cref="ContentResolver.Contains{T}()"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<T>(in SerializableGuid guid) => resolver.Contains<T>(in guid);

		/// <inheritdoc cref="ContentResolver.Contains{T}"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<T>(string id) => resolver.Contains<T>(id);

		/// <inheritdoc cref="ContentResolver.Contains{T}(int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<T>(int index) => resolver.Contains<T>(index);

		/// <inheritdoc cref="ContentResolver.Contains{T}()"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<T>() => resolver.Contains<T>();

		/// <inheritdoc cref="ContentResolver.GetEntry{T}(in SerializableGuid)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UniqueContentEntry<T> GetEntry<T>(in SerializableGuid guid) => resolver.GetEntry<T>(in guid);

		/// <inheritdoc cref="ContentResolver.GetEntry{T}(string)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UniqueContentEntry<T> GetEntry<T>(string id) => resolver.GetEntry<T>(id);

		/// <inheritdoc cref="ContentResolver.GetEntry{T}(int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UniqueContentEntry<T> GetEntry<T>(int index) => resolver.GetEntry<T>(index);

		/// <inheritdoc cref="ContentResolver.GetEntry{T}()"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SingleContentEntry<T> GetEntry<T>() => resolver.GetEntry<T>();

		/// <inheritdoc cref="ContentResolver.GetEntry{T}(in SerializableGuid)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetEntry<T>(out UniqueContentEntry<T> entry, in SerializableGuid guid)
			=> entry = resolver.GetEntry<T>(in guid);

		/// <inheritdoc cref="ContentResolver.GetEntry{T}(int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetEntry<T>(out UniqueContentEntry<T> entry, int index)
			=> entry = resolver.GetEntry<T>(index);

		/// <inheritdoc cref="ContentResolver.GetEntry{T}(string)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetEntry<T>(out UniqueContentEntry<T> entry, string id)
			=> entry = resolver.GetEntry<T>(id);

		/// <inheritdoc cref="ContentResolver.GetEntry{T}()"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetEntry<T>(out SingleContentEntry<T> entry) => entry = resolver.GetEntry<T>();

		/// <inheritdoc cref="ContentResolver.TryGetEntry{T}(in SerializableGuid, out UniqueContentEntry{T})"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetEntry<T>(in SerializableGuid guid, out UniqueContentEntry<T> entry) =>
			resolver.TryGetEntry(in guid, out entry);

		/// <inheritdoc cref="ContentResolver.TryGetEntry{T}(string, out UniqueContentEntry{T})"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetEntry<T>(string id, out UniqueContentEntry<T> entry) => resolver.TryGetEntry(id, out entry);

		/// <inheritdoc cref="ContentResolver.TryGetEntry{T}(int, out UniqueContentEntry{T})"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetEntry<T>(int index, out UniqueContentEntry<T> entry) => resolver.TryGetEntry(index, out entry);

		/// <inheritdoc cref="ContentResolver.TryGetEntry{T}(out SingleContentEntry{T})"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetEntry<T>(out SingleContentEntry<T> entry) => resolver.TryGetEntry(out entry);

		/// <inheritdoc cref="ContentResolver.Get{T}(in SerializableGuid)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Get<T>(in SerializableGuid guid) => ref resolver.Get<T>(in guid);

		/// <inheritdoc cref="ContentResolver.Get{T}(string)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Get<T>(string id) => ref resolver.Get<T>(id);

		/// <inheritdoc cref="ContentResolver.Get{T}(int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Get<T>(int index) => ref resolver.Get<T>(index);

		/// <inheritdoc cref="ContentResolver.Get{T}()"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Get<T>() => ref resolver.Get<T>();

		/// <inheritdoc cref="ContentResolver.Get{T}(in SerializableGuid)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Get<T>(in SerializableGuid guid, out T value) => value = resolver.Get<T>(in guid);

		/// <inheritdoc cref="ContentResolver.Get{T}(string)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Get<T>(string id, out T value) => value = resolver.Get<T>(id);

		/// <inheritdoc cref="ContentResolver.Get{T}(int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Get<T>(int index, out T value) => value = resolver.Get<T>(index);

		/// <inheritdoc cref="ContentResolver.Get{T}()"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Get<T>(out T value) => value = resolver.Get<T>();

		/// <inheritdoc cref="ContentResolver.Get{T}(in SerializableGuid)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Get<T>(out ContentReference<T> reference, in SerializableGuid guid)
			=> reference = resolver.GetEntry<T>(in guid);

		/// <inheritdoc cref="ContentResolver.Get{T}(int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Get<T>(out ContentReference<T> reference, int index)
			=> reference = resolver.GetEntry<T>(index);

		/// <inheritdoc cref="ContentResolver.Get{T}(string)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Get<T>(out ContentReference<T> reference, string id)
			=> reference = resolver.GetEntry<T>(id);

		/// <inheritdoc cref="ContentResolver.Get{T}()"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Get<T>(out ContentReference<T> reference)
			=> reference = resolver.GetEntry<T>();

		/// <inheritdoc cref="ContentResolver.GetAll{T}"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<ContentReference<T>> GetAll<T>() => resolver.GetAll<T>();

		/// <inheritdoc cref="ContentResolver.GetAll{T}"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<IContentEntry<T>> GetAllEntries<T>() => resolver.GetAllEntries<T>();

		/// <inheritdoc cref="ContentResolver.ToId{T}(in SerializableGuid)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToId<T>(in SerializableGuid guid) => resolver != null ? resolver.ToId<T>(in guid) : guid.ToString();

		/// <inheritdoc cref="ContentResolver.ToGuid{T}(string)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly SerializableGuid ToGuid<T>(string id)
		{
#if UNITY_EDITOR
			try
			{
				return ref resolver.ToGuid<T>(id);
			}
			catch (System.Exception e)
			{
				ContentDebug.LogWarning(e.Message);
			}

			return ref SerializableGuid.Empty;
#endif
			return ref resolver.ToGuid<T>(id);
		}

		/// <inheritdoc cref="ContentResolver.ToLabel{T}(in SerializableGuid, bool)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToLabel<T>(in SerializableGuid guid, bool verbose = false)
			=> resolver.ToLabel<T>(in guid, verbose);

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
