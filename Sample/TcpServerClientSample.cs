using System;
using System.Net;
using System.Threading;
using Sapientia.Extensions;
using Sapientia.Serializers;
using Sapientia.Tcp;
using Sapientia.Transport;

namespace Sapientia.Sample
{
	public class TcpServerClientSample
	{
		public static void Execute()
		{
			const string ADDRESS = "127.0.0.1";
			const int PORT = 10097;

			const int MAX_CONNECTIONS = 10;

			const int MESSAGE_DATA_CAPACITY = 1024;

			Console.WriteLine("Begin");

			var serverEndPoint = new IPEndPoint(IPAddress.Parse(ADDRESS), PORT);

			DoServer();

			DoClient();
			DoClient();
			DoClient();

			Console.ReadLine();

			Console.WriteLine("End");

			void DoServer()
			{
				var server = new TransportHandler_Tcp(MAX_CONNECTIONS, 10, MESSAGE_DATA_CAPACITY);
				server.SetupServer(serverEndPoint);
				server.Start();

				var serverThread = new Thread(() =>
				{
					Console.WriteLine("Test thread was started");

					while (server.GetState().HasNotIntFlag(TransportHandler_Tcp.State.Disposed))
					{
						server.connectionHandler.TryReceiveNewConnection(out _);

						server.receivingHandler.BeginRead();

						while (server.receivingHandler.TryRead(out var remoteMessage))
						{
							var reader = remoteMessage.Reader;

							var message = reader.Pop_String();
							Console.WriteLine(message);

							var sender = server.sendingHandler.CreateMessageSender();
							sender.Reader.Push_String($"Hello Client {remoteMessage.connectionReference.id}!");
							sender.Send(remoteMessage.connectionReference);

							if (!server.sendingHandler.TryApplySendStack())
								Console.WriteLine("wtf?");
						}
						server.receivingHandler.EndRead();

						Thread.Sleep(10);
					}

					Console.WriteLine("Test thread was closed");
				});
				serverThread.Start();
			}

			void DoClient()
			{
				var client = new TransportHandler_Tcp(MAX_CONNECTIONS, 10, MESSAGE_DATA_CAPACITY);
				client.SetupClient();
				client.Start();

				client.Connect(serverEndPoint);

				ConnectionReference connectionReference;
				while (!client.connectionHandler.TryReceiveNewConnection(out connectionReference))
				{
					Thread.Sleep(10);
				}

				Console.WriteLine("CONNECTION RECEIVED");
				var sender = client.sendingHandler.CreateMessageSender();
				sender.Reader.Push_String($"Hello Test {connectionReference.id}!");
				sender.Send(connectionReference);

				if (!client.sendingHandler.TryApplySendStack())
				{
					Console.WriteLine("wtf");
				}

				var clientThread = new Thread(() =>
				{
					Console.WriteLine("Client thread was started");

					while (client.GetState().HasNotIntFlag(TransportHandler_Tcp.State.Disposed))
					{
						client.receivingHandler.BeginRead();

						if (client.receivingHandler.TryRead(out var remoteMessage))
						{
							var reader = remoteMessage.Reader;

							var message = reader.Pop_String();
							Console.WriteLine(message);

							client.receivingHandler.EndRead();
							break;
						}

						client.receivingHandler.EndRead();

						Thread.Sleep(10);
					}

					client.connectionHandler.Disconnect(connectionReference);
					client.Dispose();
					Console.WriteLine("Client thread was closed");
				});
				clientThread.Start();
			}
		}
	}
}