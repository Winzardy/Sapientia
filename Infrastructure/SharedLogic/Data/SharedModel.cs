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
}
