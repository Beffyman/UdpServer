using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Beffyman.UdpContracts.Serializers;
using Beffyman.UdpServer.Internal.HandlerMapping;
using Beffyman.UdpServer.Internal.Udp;

namespace Beffyman.UdpServer
{
	public abstract class UdpHandler<T>
	{
		protected int Bytes { get; private set; }
		protected IUdpSender Sender { get; private set; }

		internal void SetInfo(in HandlerInfo info)
		{
			Bytes = info.Bytes;
			Sender = info.Sender;
		}


		public abstract Task HandleAsync(T request);

		internal Task Handle(HandlerInfo info, ISerializer serializer)
		{
			SetInfo(info);

			return HandleAsync(serializer.Deserialize<T>(info.Data));
		}

	}
}
