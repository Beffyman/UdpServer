using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Beffyman.UdpContracts.Serializers;

namespace Beffyman.UdpContracts
{
	public static class UdpMessageUdpClientExtensions
	{
		internal static readonly int MAX_MESSAGE_SIZE = 65507;

		[MethodImpl(MethodImplOptions.NoInlining)]
		internal static void ThrowSizeException(string argName)
		{
			throw new ArgumentOutOfRangeException(argName, $"Size of packet is greater than {MAX_MESSAGE_SIZE.ToString()} which is the largest dgram supported.  Please split up your data.");
		}

		public static Task<int> SendAsync<T>(this UdpClient client, in T message, in ISerializer serializer)
		{
			var msg = UdpMessage.Create<T>(message, serializer);

			var dgram = msg.ToDgram(serializer);

			if (dgram.Length > MAX_MESSAGE_SIZE)
			{
				ThrowSizeException(nameof(message));
			}

			return client.SendAsync(dgram.Data.ToArray(), dgram.Length);
		}

		public static Task<int> SendAsync(this UdpClient client, in UdpMessage message, in ISerializer serializer)
		{
			var dgram = message.ToDgram(serializer);

			if (dgram.Length > MAX_MESSAGE_SIZE)
			{
				ThrowSizeException(nameof(message));
			}

			return client.SendAsync(dgram.Data.ToArray(), dgram.Length);
		}

		public static Task<int> SendAsync(this UdpClient client, in Datagram dgram)
		{
			if (dgram.Length > MAX_MESSAGE_SIZE)
			{
				ThrowSizeException(nameof(dgram));
			}

			return client.SendAsync(dgram.Data.ToArray(), dgram.Length);
		}


		public static Task<int> SendAsync<T>(this UdpClient client, in ISerializer serializer) where T : new()
		{
			var msg = UdpMessage.Create<T>(new T(), serializer);

			var dgram = msg.ToDgram(serializer);

			if (dgram.Length > MAX_MESSAGE_SIZE)
			{
				ThrowSizeException(typeof(T).Name);
			}

			return client.SendAsync(dgram.Data.ToArray(), dgram.Length);
		}


		public static int Send<T>(this UdpClient client, in T message, in ISerializer serializer)
		{
			var msg = UdpMessage.Create<T>(message, serializer);

			var dgram = msg.ToDgram(serializer);

			if (dgram.Length > MAX_MESSAGE_SIZE)
			{
				ThrowSizeException(nameof(message));
			}

			return client.Send(dgram.Data.ToArray(), dgram.Length);
		}

		public static int Send(this UdpClient client, in UdpMessage message, in ISerializer serializer)
		{
			var dgram = message.ToDgram(serializer);

			if (dgram.Length > MAX_MESSAGE_SIZE)
			{
				ThrowSizeException(nameof(message));
			}

			return client.Send(dgram.Data.ToArray(), dgram.Length);
		}

		public static int Send(this UdpClient client, in Datagram dgram)
		{
			if (dgram.Length > MAX_MESSAGE_SIZE)
			{
				ThrowSizeException(nameof(dgram));
			}

			return client.Send(dgram.Data.ToArray(), dgram.Length);
		}


		public static int Send<T>(this UdpClient client, in ISerializer serializer) where T : new()
		{
			var msg = UdpMessage.Create<T>(new T(), serializer);

			var dgram = msg.ToDgram(serializer);

			if (dgram.Length > MAX_MESSAGE_SIZE)
			{
				ThrowSizeException(typeof(T).Name);
			}

			return client.Send(dgram.Data.ToArray(), dgram.Length);
		}
	}
}
