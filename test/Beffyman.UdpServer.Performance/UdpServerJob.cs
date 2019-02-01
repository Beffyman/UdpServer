using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Hosting;
using Beffyman.UdpContracts;
using Beffyman.UdpServer.Performance.Contracts;
using Beffyman.UdpContracts.Serializers.MessagePack;

namespace Beffyman.UdpServer.Performance
{
	/*
	 * Need to make a performance test for a bunch of messages coming into the server
	 */
	public class UdpServerJob
	{
		private IHost _host;

		[GlobalSetup]
		public async Task SetupAsync()
		{
			_host = UdpHostBuilder.CreateDefaultBuilder()
				.AddUdpHandlers(typeof(UdpServerJob).Assembly)
				.Build();

			await _host.StartAsync();
		}

		[GlobalCleanup]
		public async Task CleanupAsync()
		{
			await _host.StopAsync();

			_host.Dispose();
		}

		[Benchmark]
		[MemoryDiagnoser]
		public byte[] SendMessages()
		{
			//var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6100);
			//var client = new UdpClient();
			//client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			//client.Connect(endpoint);

			//

			//client.Send(msg, UdpMessagePackSerializer.Instance);

			var msg = new SmallMessage(10);
			var udpMessage = UdpMessage.Create(msg, UdpMessagePackSerializer.Instance);
			return udpMessage.ToDgram(UdpMessagePackSerializer.Instance).Data.ToArray();
		}
	}
}
