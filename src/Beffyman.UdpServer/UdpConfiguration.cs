using System;
using Beffyman.UdpContracts.Serializers;

namespace Beffyman.UdpServer
{
	public interface IUdpConfiguration
	{
		/// <summary>
		/// Port the udp socket will listen on. Default value is 6100
		/// </summary>
		ushort Port { get; set; }
		/// <summary>
		/// Buffer for incoming messages, if it is too small, the incoming requests will be dropped which will show up as packet loss. Default Value is 2.5e+7
		/// </summary>
		int ReceiveBufferSize { get; set; }

		///// <summary>
		///// Buffer for outgoing messages, if it is too small, the outgoing requests will be dropped
		///// </summary>
		//int SendBufferSize { get; set; }

		/// <summary>
		/// How many sockets are listening on the port. Default is Math.Min(Environment.ProcessorCount, 16)
		/// </summary>
		int IOQueueCount { get; set; }

		/// <summary>
		/// Serializer to use for the conversion of the UdpMessage to a byte[].  Default is <see cref="UdpMessagePackSerializer"/>
		/// </summary>
		Type Serializer { get; set; }

		IUdpConfiguration UseSerializer<T>() where T : ISerializer;

	}

	internal sealed class UdpConfiguration : IUdpConfiguration
	{
		public ushort Port { get; set; } = 6100;
		public int ReceiveBufferSize { get; set; } = (int)2.5e+7;
		//public int SendBufferSize { get; set; } = 131071;
		public int IOQueueCount { get; set; } = Math.Min(Environment.ProcessorCount, 16);
		public Type Serializer { get; set; }

		public IUdpConfiguration UseSerializer<T>() where T : ISerializer
		{
			Serializer = typeof(T);

			return this;
		}
	}
}
