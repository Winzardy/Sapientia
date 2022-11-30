using System.Net;
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
			const string address = "127.0.0.1";
			const int port = 10097;

			const int maxConnections = 10;

			Console.WriteLine("Begin");

			var serverEndPoint = new IPEndPoint(IPAddress.Parse(address), port);

			DoServer();

			DoClient();
			DoClient();
			DoClient();

			Console.ReadLine();

			Console.WriteLine("End");

			void DoServer()
			{
				var server = new TransportHandler_Tcp(maxConnections, 10, 512);
				server.SetupServer(serverEndPoint);
				server.Start();

				var serverThread = new Thread(() =>
				{
					Console.WriteLine("Server thread was started");

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

					Console.WriteLine("Server thread was closed");
				});
				serverThread.Start();
			}

			void DoClient()
			{
				var client = new TransportHandler_Tcp(maxConnections, 10, 512);
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
				sender.Reader.Push_String($"Hello Server {connectionReference.id}!");
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

					client.connectionHandler.CloseConnection(connectionReference);
					client.Dispose();
					Console.WriteLine("Client thread was closed");
				});
				clientThread.Start();
			}
		}
	}
}