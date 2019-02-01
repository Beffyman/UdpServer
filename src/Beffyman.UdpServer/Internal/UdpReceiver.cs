using System;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace Beffyman.UdpServer.Internal
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
#if NETCOREAPP
            _awaitableEventArgs.SetBuffer(buffer);
#elif NETSTANDARD
			var segment = buffer.GetArray();

			_awaitableEventArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);
#else
#error TFMs need to be updated
#endif
			if (!_socket.ReceiveAsync(_awaitableEventArgs))
			{
				_awaitableEventArgs.Complete();
			}

			return _awaitableEventArgs;
		}

	}
}
