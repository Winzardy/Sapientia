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
		public static void Save<TData, TSharedModel>([CanBeNull] Dictionary<string, TSharedModel> idToModel, out TData[] data)
			where TSharedModel : SharedModel<TData>
		{
			data = null;
			if (idToModel.IsNullOrEmpty())
				return;

			data = new TData[idToModel.Count];
			foreach (var (shared, i) in idToModel.Values.WithIndex())
				shared.Save(out data[i]);
		}

		public static void Load<TData, TSharedModel>([NotNull] Dictionary<string, TSharedModel> idToModel, in TData[] data,
			SharedIdSelector<TData> selector)
			where TSharedModel : SharedModel<TData>
		{
			foreach (ref var dataItem in data.AsSpan())
			{
				var id = selector.Invoke(in dataItem);

				if (idToModel.TryGetValue(id, out var model))
					model.Load(in dataItem);
			}
		}
	}

	public delegate string SharedIdSelector<TData>(in TData data);
}
