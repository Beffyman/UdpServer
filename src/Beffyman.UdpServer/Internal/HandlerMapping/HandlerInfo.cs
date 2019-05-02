using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Beffyman.UdpContracts;
using Beffyman.UdpServer.Internal.Udp;

namespace Beffyman.UdpServer.Internal.HandlerMapping
{
	internal readonly struct HandlerInfo
	{
		public readonly int Bytes;
		public readonly ReadOnlyMemory<byte> Data;
		public readonly IUdpSender Sender;

		public HandlerInfo(in int bytes, in ReadOnlyMemory<byte> data, IUdpSender sender)
		{
			Bytes = bytes;
			Data = data;
			Sender = sender;
		}

		public HandlerInfo(in Datagram dgram, IUdpSender sender)
		{
			Bytes = dgram.Length;
			Data = dgram.Data;
			Sender = sender;
		}

	}
}
