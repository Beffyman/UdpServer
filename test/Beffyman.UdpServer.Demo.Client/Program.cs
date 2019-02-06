using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Beffyman.UdpContracts;
using Beffyman.UdpContracts.Serializers.MessagePack;
using Beffyman.UdpServer.Demo.Contracts;
using static System.Console;

namespace Beffyman.UdpServer.Demo.Client
{
	public static class Program
	{

		private static UdpClient _client;

		public static async Task Main(string[] args)
		{
			try
			{
				CreateClient();

				//await SendMessages(3);
				await SendMessages(100);//~0.016 sec
				await SendMessages(1_000);//~0.035 sec
				await SendMessages(10_000);//~0.18 sec
				await SendMessages(100_000);//~1.6 sec
				await SendMessages(1_000_000);//~17 sec
											  //await SendMessages(10_000_000);//~160 sec

				await ReadLineAsync();
				await ShutdownServer();
			}
			catch (Exception)
			{
				Debugger.Break();
			}
		}

		private static async Task ReadLineAsync()
		{
			CancellationTokenSource src = new CancellationTokenSource(TimeSpan.FromSeconds(5));

			var consoleInput = Task.Run(async () =>
			{
				await Console.Out.WriteAsync("Press Enter to Exit, or in 5 seconds it will automatically exit");
				_ = Task.Run(() => ReadLine());

				while (!src.IsCancellationRequested)
				{
					await Task.Delay(500);
				}
			}, src.Token);


			await consoleInput;
		}

		private static void CreateClient()
		{
			var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6002);
			WriteLine($"Sending to {endpoint}");
			_client = new UdpClient();
			_client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			_client.Connect(endpoint);
		}

		private static async Task SendMessages(int count)
		{
			WriteLine($"Waiting 5 seconds for {count} message request");
			await Task.Delay(5000);

			await _client.SendAsync(new StartTimerMessage(count), UdpMessagePackSerializer.Instance);

			for (int i = 0; i < count; i++)
			{
				var dto = new MyDto()
				{
					Collision = Guid.NewGuid(),
					Id = i,
					Name = $"MyDto{i}",
					When = DateTime.Now
				};

				await _client.SendAsync(dto, UdpMessagePackSerializer.Instance);
			}

			await _client.SendAsync<StopTimerMessage>(UdpMessagePackSerializer.Instance);
		}

		private static async Task ErrorMessageTest()
		{
			WriteLine("Waiting 3 seconds for invalid message test");
			await Task.Delay(3000);

			var data = System.Text.Encoding.UTF8.GetBytes("HELLO MR.ERROR");


			await _client.SendAsync(data, data.Length);
		}

		private static async Task ShutdownServer()
		{
			await _client.SendAsync<ShutdownMessage>(UdpMessagePackSerializer.Instance);
		}

	}
}
