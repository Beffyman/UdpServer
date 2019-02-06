using System;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace Beffyman.UdpServer.Internal.Udp
{
	internal sealed class UdpReceiver : IDisposable
	{
		private readonly Socket _socket;
		private readonly SocketAwaitableEventArgs _awaitableEventArgs;

		public UdpReceiver(Socket socket, PipeScheduler scheduler)
		{
			_socket = socket;
			_awaitableEventArgs = new SocketAwaitableEventArgs(scheduler);
		}

		public void Dispose() => _awaitableEventArgs.Dispose();

		public SocketAwaitableEventArgs ReceiveAsync(in Memory<byte> buffer)
		{
			_awaitableEventArgs.SetBuffer(buffer);
			_awaitableEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

			if (!_socket.ReceiveMessageFromAsync(_awaitableEventArgs))
			{
				_awaitableEventArgs.Complete();
			}

			return _awaitableEventArgs;
		}

	}
}
