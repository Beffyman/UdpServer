using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Beffyman.UdpContracts;
using Beffyman.UdpServer.Internal.Udp;

namespace Beffyman.UdpServer.Internal.HandlerMapping
{
	internal class HandlerInfo
	{
		public readonly int Bytes;
		public readonly ReadOnlyMemory<byte> Data;
		public readonly IUdpSenderFactory SenderFactory;
		public readonly IPAddress Address;

		public HandlerInfo(int bytes, in ReadOnlyMemory<byte> data, IUdpSenderFactory senderFactory, IPAddress address)
		{
			Bytes = bytes;
			Data = data;
			SenderFactory = senderFactory;
			Address = address;
		}

		public HandlerInfo(in Datagram dgram, IUdpSenderFactory senderFactory, IPAddress address)
		{
			Bytes = dgram.Length;
			Data = dgram.Data;
			SenderFactory = senderFactory;
			Address = address;
		}

	}
}
