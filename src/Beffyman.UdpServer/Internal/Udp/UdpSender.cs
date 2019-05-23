using System;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Beffyman.UdpContracts.Serializers;

namespace Beffyman.UdpServer.Internal.Udp
{
	public interface IUdpSender : IDisposable
	{
		IPAddress Address { get; }

		Task<int> SendAsync<T>(T message, ISerializer serializer);
	}

	internal sealed class UdpSender : IUdpSender
	{
		public IPAddress Address { get; private set; }
		private IUdpConfiguration _configuration;
		private readonly UdpClient _client;

		public UdpSender(IUdpConfiguration configuration, IPAddress address)
		{
			_configuration = configuration;
			Address = address;


			var endpoint = new IPEndPoint(Address, _configuration.SendPort);
			_client = new UdpClient();
			_client.Client.SendBufferSize = _configuration.SendBufferSize;
			_client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			_client.Connect(endpoint);
		}

		public void Dispose()
		{
			_client.Dispose();
		}

		public Task<int> SendAsync<T>(T message, ISerializer serializer)
		{
			ReadOnlyMemory<byte> data = serializer.Serialize(message);

			return _client.SendAsync(data.ToArray(), data.Length);
		}
	}
}
