namespace Sapientia
{
	public abstract class IdentifiableConfig : IExternallyIdentifiable
	{
		private string _id;

		public string Id => _id;

		void IExternallyIdentifiable.SetId(string id)
		{
			_id = id;
		}
	}
}
