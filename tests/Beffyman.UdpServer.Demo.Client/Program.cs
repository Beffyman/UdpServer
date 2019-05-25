using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Beffyman.UdpContracts;
using Beffyman.UdpContracts.Serializers.MessagePack;
using Beffyman.UdpContracts.Serializers.Json;
using Beffyman.UdpServer.Demo.Contracts;
using static System.Console;
using Beffyman.UdpContracts.Serializers;
using System.Linq;

namespace Beffyman.UdpServer.Demo.Client
{
	public static class Program
	{

		private static UdpClient _client;
		private static ISerializer Serializer = UdpMessagePackSerializer.Instance;
		//private static ISerializer Serializer = UdpJsonSerializer.Instance;

		public static async Task Main(string[] args)
		{
			try
			{
				CreateClient();

				WriteLine($"Starting Small Messages!");
				//Time estimates are for my local machine (Ryzen 7 2700X 3.7GHz)
				//await SendMessages(3);
				await SendSmallMessages(100);//~0.012 sec
				await SendSmallMessages(1_000);//~0.026 sec
				await SendSmallMessages(10_000);//~0.165 sec
				await SendSmallMessages(100_000);//~1.57 sec
				await SendSmallMessages(1_000_000);//~15 sec
												   //await SendMessages(10_000_000);//~160 sec

				//WriteLine($"Starting Large Messages!");

				//await SendLargeMessages(100);//~0.012 sec
				//await SendLargeMessages(1_000);//~0.026 sec
				//await SendLargeMessages(10_000);//~0.165 sec
				//await SendLargeMessages(100_000);//~1.57 sec
				//await SendLargeMessages(1_000_000);//~15 sec


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

		private static async Task SendSmallMessages(int count)
		{
			WriteLine($"Waiting 5 seconds for {count} small message requests");
			await Task.Delay(5000);

			await _client.SendAsync(new StartTimerMessage(count), Serializer);

			for (int i = 0; i < count; i++)
			{
				var dto = new MyDto()
				{
					Collision = Guid.NewGuid(),
					Id = i,
					Name = $"MyDto{i}",
					When = DateTime.Now
				};

				await _client.SendAsync(dto, Serializer);
			}

			await _client.SendAsync<StopTimerMessage>(Serializer);
		}

		private static async Task SendLargeMessages(int count)
		{
			Random r = new Random();
			var objects = Enumerable.Repeat(new MyLargeDto()
			{
				Data = Enumerable.Repeat(new MyLargePartDto
				{
					Collision = Guid.NewGuid(),
					Id = r.Next(),
					Name = $"MyDto{r.Next()}",
					When = DateTime.Now
				},100).ToArray()
			}, count).ToArray();


			WriteLine($"Waiting 5 seconds for {count} large message requests");
			await Task.Delay(5000);

			await _client.SendAsync(new StartTimerMessage(count), Serializer);

			for (int i = 0; i < count; i++)
			{
				var dto = objects[i];

				await _client.SendAsync(dto, Serializer);
			}

			await _client.SendAsync<StopTimerMessage>(Serializer);
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
			await _client.SendAsync<ShutdownMessage>(Serializer);
		}

	}
}
