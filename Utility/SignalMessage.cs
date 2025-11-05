namespace Sapientia.Utility
{
	public interface ISignalReceiver
	{
		public bool Signal(string name) => false;
	}

	public struct SignalMessage
	{
		public string name;

		public SignalMessage(string name)
		{
			this.name = name;
		}
	}

	public struct SignalMessage<TArgs>
	{
		public string name;
		public TArgs args;

		public SignalMessage(string name, in TArgs args)
		{
			this.name = name;
			this.args = args;
		}
	}
}
