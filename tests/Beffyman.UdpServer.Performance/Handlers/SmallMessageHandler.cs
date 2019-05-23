using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beffyman.UdpServer.Performance.Contracts;

namespace Beffyman.UdpServer.Performance.Handlers
{
	public class SmallMessageHandler : UdpHandler<SmallMessage>
	{
		public override Task HandleAsync(SmallMessage request)
		{
			return Task.CompletedTask;
		}
	}
}
