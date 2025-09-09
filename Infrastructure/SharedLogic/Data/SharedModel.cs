namespace SharedLogic
{
	public abstract class SharedModel<TData>
	{
		public void Load(in TData data)
		{
			OnLoad(in data);
		}

		protected abstract void OnLoad(in TData data);

		public void Save(ref TData data)
		{
			OnSave(ref data);
		}

		protected abstract void OnSave(ref TData data);
	}
}
