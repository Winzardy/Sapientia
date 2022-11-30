using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Sapientia.Collections;
using Sapientia.Extensions;
using Sapientia.Transport;

namespace Sapientia.Tcp
{
	public class ConnectionHandler
	{
		public enum State
		{
			Free = 0,
			Busy = 1,
		}

		private const int EXCHANGE_INTERVAL = 10;

		private readonly SparseSet<Connection_Tcp> _connections;
		private readonly SimpleList<Connection_Tcp> _existingConnections;
		public readonly ReadOnlySimpleList<Connection_Tcp> existingConnections;

		private volatile int _nextConnectionId;

		private CircularBuffer<ConnectionReference> _newConnectionBuffer;
		private volatile int _newConnectionsCount;

		private volatile int _state;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public State GetState() => _state.ToEnum<State>();

		public int NewConnectionsCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _newConnectionsCount;
		}

		internal ConnectionHandler(int connectionsCapacity, int messageCapacity)
		{
			_connections = new SparseSet<Connection_Tcp>(connectionsCapacity);
			_existingConnections = new SimpleList<Connection_Tcp>(connectionsCapacity);
			existingConnections = new ReadOnlySimpleList<Connection_Tcp>(_existingConnections);
			_nextConnectionId = 0;

			_newConnectionBuffer = new CircularBuffer<ConnectionReference>(connectionsCapacity);
			_state = (int)State.Free;

			var connections = _connections.GetValueArray();

			for (var i = 0; i < connections.Length; i++)
			{
				connections[i] = new Connection_Tcp(messageCapacity);
			}
		}

		public void CloseConnection(ConnectionReference connectionReference)
		{
			if (TryGetConnection(connectionReference, out var connection))
				return;

			connection.Close();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetConnection(ConnectionReference connectionReference, out Connection_Tcp connection)
		{
			connection = _connections.GetValue(connectionReference.index);
			return connection.ConnectionReference == connectionReference;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Connection_Tcp ForceGetConnection(int index)
		{
			return _connections.GetValue(index);
		}

		public bool TryReceiveNewConnection(out ConnectionReference connectionReference)
		{
			if (_newConnectionsCount > 0 &&
			    _state.Interlocked_CompareExchangeIntEnum(State.Busy, State.Free) == State.Free)
			{
				while (!_newConnectionBuffer.IsEmpty)
				{
					connectionReference = _newConnectionBuffer.RemoveFirst();
					var connection = _connections.GetValue(connectionReference.index);

					if (connectionReference.id == connection.ConnectionReference.id)
					{
						_existingConnections.Add(connection);
						Interlocked.Decrement(ref _newConnectionsCount);

						_state = (int)State.Free;
						return true;
					}
				}

				_state = (int)State.Free;
			}

			connectionReference = default;
			return false;
		}

		internal void AcceptConnection(Socket connectionSocket)
		{
			while (_state.Interlocked_CompareExchangeIntEnum(State.Busy, State.Free) == State.Busy)
			{
				Thread.Sleep(EXCHANGE_INTERVAL);
			}

			if (_connections.IsFull)
			{
				// Overloaded
				connectionSocket.Close();
				return;
			}

			var index = _connections.AllocateValueIndex();
			var connection = _connections.GetValue(index);
			var connectionReference = new ConnectionReference(index, _nextConnectionId);

			connection.Start(connectionSocket, connectionReference);

			_newConnectionBuffer.AddLast(connectionReference);

			Interlocked.Increment(ref _nextConnectionId);
			Interlocked.Increment(ref _newConnectionsCount);
			_state = (int)State.Free;
		}

		internal void Update()
		{
			if (_state.Interlocked_CompareExchangeIntEnum(State.Busy, State.Free) == State.Busy)
				return;

			DisconnectClosedConnections();

			_state = (int)State.Free;
		}

		private void DisconnectClosedConnections()
		{
			for (var i = 0; i < _existingConnections.Count;)
			{
				var connection = _existingConnections[i];

				if (connection.State == Connection_Tcp.ConnectionState.Disconnecting)
				{
					connection.Disconnect();
					_existingConnections.RemoveAtSwapBack(i);
					_connections.ReleaseValueIndex(connection.ConnectionReference.index);
				}
				else
				{
					i++;
				}
			}
		}

		internal void Dispose()
		{
			for (var i = 0; i < _existingConnections.Count; i++)
			{
				_existingConnections[i].Disconnect();
			}

			_connections.Dispose();
			_existingConnections.Dispose();
			_newConnectionBuffer.Dispose();
		}
	}
}