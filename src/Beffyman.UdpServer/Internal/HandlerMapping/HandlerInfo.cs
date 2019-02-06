using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Beffyman.UdpContracts;

namespace Beffyman.UdpServer.Internal.HandlerMapping
{
	internal readonly struct HandlerInfo
	{
		public readonly int Bytes;
		public readonly ReadOnlyMemory<byte> Data;
		public readonly IPAddress Sender;

		public HandlerInfo(int bytes, in ReadOnlyMemory<byte> data, in IPAddress sender)
		{
			Bytes = bytes;
			Data = data;
			Sender = sender;
		}

		public HandlerInfo(in Datagram dgram, in IPAddress sender)
		{
			Bytes = dgram.Length;
			Data = dgram.Data;
			Sender = sender;
		}

	}
}
