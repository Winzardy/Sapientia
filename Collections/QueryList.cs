using System;
using Sapientia.Data;

namespace Sapientia.Collections
{
	public class QueryList<T> : AsyncClass, IDisposable where T: IQuery
	{
		private const int DEFAULT_CAPACITY = 8;

		private readonly SimpleList<IQuery> _queries;

		public QueryList(int capacity = DEFAULT_CAPACITY)
		{
			_queries = new(DEFAULT_CAPACITY);
		}

		public void ExecuteOrAddQuery<T1>(T1 query) where T1: T
		{
			if (query.TryExecute())
				return;
			AddQuery(query);
		}

		public void AddQuery<T1>(T1 query) where T1: T
		{
			using var scope = GetBusyScope();
			_queries.AddWithExpand(query);
		}

		public void ExecuteQueries()
		{
			using var scope = GetBusyScope();
			for (var i = _queries.Count - 1; i >= 0; i--)
			{
				var query = _queries[i];
				if (query.TryExecute())
				{
					_queries[i] = default;
					_queries.RemoveAt(i);
				}
			}
		}

		public void Clear()
		{
			using var scope = GetBusyScope();
			_queries.Clear();
		}

		public void Dispose()
		{
			using var scope = GetBusyScope();
			_queries.Dispose();
		}
	}

	public interface IQuery
	{
		public bool TryExecute();
	}
}