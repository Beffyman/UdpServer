using System;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Beffyman.UdpServer.Internal.Udp
{
	public sealed class SocketAwaitableEventArgs : SocketAsyncEventArgs, INotifyCompletion
	{
		private static readonly Action _callbackCompleted = () => { };

		private readonly PipeScheduler _ioScheduler;

		private Action _callback;

		public SocketAwaitableEventArgs(PipeScheduler ioScheduler)
		{
			_ioScheduler = ioScheduler;
		}

		public SocketAwaitableEventArgs GetAwaiter() => this;
		public bool IsCompleted => ReferenceEquals(_callback, _callbackCompleted);

		public int GetResult()
		{
			Debug.Assert(ReferenceEquals(_callback, _callbackCompleted));

			_callback = null;

			if (SocketError != SocketError.Success)
			{
				ThrowSocketException(SocketError);
			}

			//So we get the # of bytes transfered to know the current end of the memory block
			int index = BytesTransferred;
			//Then we convert the sender address into a byte array
			var addressBytes = ReceiveMessageFromPacketInfo.Address.GetAddressBytes();

			//Then we write the sender address into the buffer
			for (int i = 0; i < addressBytes.Length; i++)
			{
				MemoryBuffer.Span[index + i] = addressBytes[i];
			}

			//Then we get the byte array for the length of the address bytes
			var lengthBytes = new Int32Converter(addressBytes.Length);

			//And then write the length onto the end of the buffer
			MemoryBuffer.Span[index + addressBytes.Length + 0] = lengthBytes.Byte1;
			MemoryBuffer.Span[index + addressBytes.Length + 1] = lengthBytes.Byte2;
			MemoryBuffer.Span[index + addressBytes.Length + 2] = lengthBytes.Byte3;
			MemoryBuffer.Span[index + addressBytes.Length + 3] = lengthBytes.Byte4;

			//We always know the last 4 are the length of the address, then we can splice up and get the address and then the rest above it is the data
			return index + addressBytes.Length + 4;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void ThrowSocketException(SocketError e)
		{
			throw new SocketException((int)e);
		}

		public void OnCompleted(Action continuation)
		{
			if (ReferenceEquals(_callback, _callbackCompleted) ||
				ReferenceEquals(Interlocked.CompareExchange(ref _callback, continuation, null), _callbackCompleted))
			{
				Task.Run(continuation);
			}
		}

		public void Complete()
		{
			OnCompleted(this);
		}

		protected override void OnCompleted(SocketAsyncEventArgs _)
		{
			var continuation = Interlocked.Exchange(ref _callback, _callbackCompleted);

			if (continuation != null)
			{
				_ioScheduler.Schedule(state => ((Action)state)(), continuation);
			}
		}
	}
}
