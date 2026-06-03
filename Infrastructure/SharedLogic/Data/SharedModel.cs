using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sapientia.Collections;

namespace SharedLogic
{
	public abstract class SharedModel<TData>
	{
		public void Load(in TData data)
		{
			OnLoad(in data);
		}

		protected abstract void OnLoad(in TData data);

		public void Save(out TData data)
		{
			OnSave(out data);
		}

		protected abstract void OnSave(out TData data);
	}

	public static class SharedModelUtility
	{
		public static void Save<TData, TSharedModel>([CanBeNull] Dictionary<string, TSharedModel> idToModel, out TData[] data,
			SharedDataPredicate<TData> predicate = null)
			where TSharedModel : SharedModel<TData>
		{
			data = null;
			if (idToModel.IsNullOrEmpty())
				return;

			using var list = new SimpleList<TData>();
			foreach (var (shared, i) in idToModel.Values.WithIndex())
			{
				shared.Save(out var saveData);
				if (predicate != null && !predicate(in saveData))
					continue;
				if(EqualityComparer<TData>.Default.Equals(saveData, default))
					continue;
				list.Add(in saveData);
			}

			data = list.ToArray();
		}

		public static void Load<TData, TSharedModel>([NotNull] Dictionary<string, TSharedModel> idToModel, in TData[] data,
			SharedIdSelector<TData> selector)
			where TSharedModel : SharedModel<TData>
		{
			var i = -1;
			foreach (ref var dataItem in data.AsSpan())
			{
				i++;
				var id = selector.Invoke(in dataItem);

				if (id.IsNullOrEmpty())
				{
					SLDebug.LogWarning($"Empty or null id for {typeof(TData).Name} at index {i}...");
					continue;
				}

				if (idToModel.TryGetValue(id, out var model))
					model.Load(in dataItem);
			}
		}
	}

	public delegate string SharedIdSelector<TData>(in TData data);

	public delegate bool SharedDataPredicate<TData>(in TData data);
}
