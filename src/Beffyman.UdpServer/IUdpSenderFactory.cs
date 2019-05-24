using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Beffyman.UdpServer
{
	public interface IUdpSenderFactory
	{
		IUdpSender GetSender(IPAddress address);
	}
}
