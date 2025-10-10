using System;

namespace Sapientia
{
	public interface IIdentifiable
	{
		public string Id { get; }
	}

	public interface IIdentifierSource<out T> : IIdentifierSource
		where T : IIdentifiable
	{
		public T Source { get; }
		string IIdentifiable.Id => Source.Id;
	}

	public interface IIdentifierSource : IIdentifiable
	{
	}

	public interface IExternallyIdentifiable : IIdentifiable
	{
		public void SetId(string id);
	}
}
