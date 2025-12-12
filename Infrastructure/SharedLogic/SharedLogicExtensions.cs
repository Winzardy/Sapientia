using Sapientia;
using Sapientia.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharedLogic
{
	public static class SharedLogicExtensions
	{
		public static TData[] ExtractSaveData<TModel, TData>(this Dictionary<string, TModel> dict) where TModel : SharedModel<TData>
		{
			return dict.Values.ExtractSaveData<TModel, TData>();
		}

		public static TData[] ExtractSaveData<TModel, TData>(this IEnumerable<TModel> models) where TModel : SharedModel<TData>
		{
			return models
				.Select(model =>
				{
					model.Save(out var data);
					return data;
				})
				.ToArray();
		}

		public static List<TModel> ToModelsList<TModel, TData>(this IList<TData> data,
			Func<TData, TModel> converter,
			Action<TModel> onAddition = null)
			where TModel : SharedModel<TData>
		{
			var list = new List<TModel>();
			list.FillModels(data, converter, onAddition);

			return list;
		}

		public static Dictionary<string, TModel> ToModelsDict<TModel, TData>(this IList<TData> data,
			Func<TData, TModel> converter,
			Action<TModel> onAddition = null)
			where TModel : SharedModel<TData>, IIdentifiable
		{
			var dict = new Dictionary<string, TModel>();
			dict.FillModels(data, converter, onAddition);

			return dict;
		}

		public static void FillModels<TModel, TData>(this ICollection<TModel> collection, IList<TData> data,
			Func<TData, TModel> converter,
			Action<TModel> onAddition = null)
			where TModel : SharedModel<TData>
		{
			if (collection == null)
				throw new NullReferenceException(nameof(collection));

			FillModelsInternal(
				data,
				converter,
				collection.Add,
				onAddition);
		}

		public static void FillModels<TModel, TData>(this Dictionary<string, TModel> dict, IList<TData> data,
			Func<TData, TModel> converter,
			Action<TModel> onAddition = null)
			where TModel : SharedModel<TData>, IIdentifiable
		{
			if (dict == null)
				throw new NullReferenceException(nameof(dict));

			FillModelsInternal(
				data,
				converter,
				x => dict.Add(x.Id, x),
				onAddition);
		}

		private static void FillModelsInternal<TModel, TData>(IList<TData> data,
			Func<TData, TModel> converter,
			Action<TModel> addFunc,
			Action<TModel> onAddition = null)
			where TModel : SharedModel<TData>
		{
			if (converter == null)
				throw new NullReferenceException(nameof(converter));

			if (addFunc == null)
				throw new NullReferenceException(nameof(addFunc));

			if (data.IsNullOrEmpty())
				return;

			for (int i = 0; i < data.Count; i++)
			{
				var nextData = data[i];
				if (nextData != null)
				{
					var model = converter.Invoke(nextData);
					if (model != null)
					{
						addFunc.Invoke(model);
						onAddition?.Invoke(model);
					}
				}
			}
		}
	}
}
