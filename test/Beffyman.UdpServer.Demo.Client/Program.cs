using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
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

				await ErrorMessageTest();
				await SendMessages(100);
				await SendMessages(1_000);
				await SendMessages(10_000);
				await SendMessages(100_000);
				await SendMessages(1_000_000);
				WriteLine("Press Enter to Exit");
				ReadLine();
			}
			catch (Exception)
			{
				Debugger.Break();
			}
		}

		private static void CreateClient()
		{
			var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6002);
			_client = new UdpClient();
			_client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			_client.Connect(endpoint);
		}

		private static async Task SendMessages(int count)
		{
			WriteLine($"Waiting 3 seconds for {count} message request");
			await Task.Delay(3000);

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

			await _client.SendAsync(new StopTimerMessage(), UdpMessagePackSerializer.Instance);
		}

		private static async Task ErrorMessageTest()
		{
			WriteLine("Waiting 3 seconds for invalid message test");
			await Task.Delay(3000);

			var data = System.Text.Encoding.UTF8.GetBytes("HELLO MR.ERROR");


			await _client.SendAsync(data, data.Length);
		}

	}
}
