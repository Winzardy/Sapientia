#nullable enable
using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Sapientia.Collections;
using Sapientia.Data;
using Sapientia.Transport;

namespace Sapientia.Tcp
{
	public class ConnectionHandler : AsyncClass
	{
		public event Action<int> ConnectionFailedEvent;
		public event Action<Socket, int> ConnectionDeclinedEvent;
		public event Action<int> ConnectionDisconnectedEvent;

		private readonly SparseSet<Connection_Tcp> _connections;
		private readonly SimpleList<Connection_Tcp> _existingConnections;
		public readonly ReadOnlySimpleList<Connection_Tcp> existingConnections;

		private volatile int _nextConnectionId;

		private CircularBuffer<ConnectionReference> _newConnectionBuffer;
		private volatile int _newConnectionsCount;

		public int NewConnectionsCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _newConnectionsCount;
		}

		internal ConnectionHandler(int connectionsCapacity, int messageCapacity) : base()
		{
			_connections = new SparseSet<Connection_Tcp>(connectionsCapacity);
			_existingConnections = new SimpleList<Connection_Tcp>(connectionsCapacity);
			existingConnections = new ReadOnlySimpleList<Connection_Tcp>(_existingConnections);
			_nextConnectionId = 0;

			_newConnectionBuffer = new CircularBuffer<ConnectionReference>(connectionsCapacity);

			var connections = _connections.GetValueArray();

			for (var i = 0; i < connections.Length; i++)
			{
				connections[i] = new Connection_Tcp(messageCapacity);
			}
		}

		public void Disconnect(ConnectionReference connectionReference)
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
			if (_newConnectionsCount > 0 && TrySetBusy())
			{
				while (!_newConnectionBuffer.IsEmpty)
				{
					connectionReference = _newConnectionBuffer.RemoveFirst();
					var connection = _connections.GetValue(connectionReference.index);

					if (connectionReference.id == connection.ConnectionReference.id)
					{
						_existingConnections.AddWithoutExpand(connection);
						Interlocked.Decrement(ref _newConnectionsCount);

						SetFree();
						return true;
					}
				}

				SetFree();
			}

			connectionReference = default;
			return false;
		}

		internal void AcceptConnection(Socket? connectionSocket, int customId)
		{
			if (connectionSocket == null)
			{
				ConnectionFailedEvent?.Invoke(customId);
				return;
			}

			using var scope = GetBusyScope();

			if (_connections.IsFull)
			{
				// Overloaded
				ConnectionDeclinedEvent?.Invoke(connectionSocket, customId);
				connectionSocket.Close();

				return;
			}

			var index = _connections.AllocateValueIndex();
			var connection = _connections.GetValue(index);
			var connectionReference = new ConnectionReference(index, _nextConnectionId, customId);

			connection.Start(connectionSocket, connectionReference);

			_newConnectionBuffer.AddLast(connectionReference);

			Interlocked.Increment(ref _nextConnectionId);
			Interlocked.Increment(ref _newConnectionsCount);
		}

		internal void Update()
		{
			if (!TrySetBusy())
				return;

			DisconnectClosedConnections();

			SetFree();
		}

		private void DisconnectClosedConnections()
		{
			for (var i = 0; i < _existingConnections.Count;)
			{
				var connection = _existingConnections[i];

				if (connection.State == Connection_Tcp.ConnectionState.Disconnecting)
				{
					var connectionReference = connection.ConnectionReference;

					connection.Disconnect();
					_existingConnections.RemoveAtSwapBack(i);
					_connections.ReleaseValueIndex(connectionReference.index);

					ConnectionDisconnectedEvent?.Invoke(connectionReference.customId);
				}
				else
				{
					i++;
				}
			}
		}

		internal void Dispose()
		{
			using var scope = GetBusyScope();

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