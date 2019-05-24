using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Beffyman.UdpContracts.Serializers;

namespace Beffyman.UdpServer
{
	public interface IUdpSender : IDisposable
	{
		IPAddress Address { get; }

		Task<int> SendAsync<T>(T message, ISerializer serializer);
	}
}
