using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beffyman.UdpServer.Demo.Contracts;
using Beffyman.UdpServer.Demo.Services;

namespace Beffyman.UdpServer.Demo.Handlers
{
	public class StopTimerHandler : UdpHandler<StopTimerMessage>
	{
		private readonly ICounterService _counterService;

		public StopTimerHandler(ICounterService counterService)
		{
			_counterService = counterService;
		}

		public override ValueTask HandleAsync(in StopTimerMessage request)
		{
			_counterService.Stop();
			return new ValueTask(Task.CompletedTask);
		}
	}
}
