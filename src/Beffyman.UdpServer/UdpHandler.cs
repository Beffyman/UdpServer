﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Beffyman.UdpContracts.Serializers;
using Beffyman.UdpServer.Internal.HandlerMapping;

namespace Beffyman.UdpServer
{
	public abstract class UdpHandler<T>
	{
		protected int Bytes { get; private set; }
		protected IPAddress Sender { get; private set; }

		internal void SetInfo(in HandlerInfo info)
		{
			Bytes = info.Bytes;
			Sender = info.Sender;
		}


		public abstract ValueTask HandleAsync(in T request);

		internal ValueTask Handle(in HandlerInfo info, ISerializer serializer)
		{
			SetInfo(info);

			return HandleAsync(serializer.Deserialize<T>(info.Data));
		}

	}
}
