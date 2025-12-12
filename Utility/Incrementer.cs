using System;

namespace Sapientia.Utility
{
	public class Incrementer
	{
		private int _initialValue;
		private int _value;
		private Func<int, int> _incrementFunc;

		public int current { get { return _value; } }

		public Incrementer(int initialValue = 0)
		{
			_initialValue = initialValue;
			Reset();
		}

		public Incrementer(Func<int, int> function, int initialValue = 0)
		{
			_incrementFunc = function;
			_initialValue = initialValue;
			Reset();
		}

		public void Set(int value)
		{
			_value = value;
		}

		public int Get()
		{
			int currentValue = _value;

			_value = _incrementFunc != null ?
				_incrementFunc.Invoke(_value) :
				_value + 1;

			return currentValue;
		}

		public ushort GetShort()
		{
			return (ushort)Get();
		}

		public string GetString()
		{
			return Get().ToString();
		}

		public void Reset()
		{
			_value = _initialValue;
		}
	}
}
