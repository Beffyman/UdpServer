using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beffyman.UdpServer.Demo.Contracts;
using Beffyman.UdpServer.Demo.Services;

namespace Beffyman.UdpServer.Demo.Handlers
{
	public class StartTimerHandler : UdpHandler<StartTimerMessage>
	{
		private readonly ICounterService _counterService;

		public StartTimerHandler(ICounterService counterService)
		{
			_counterService = counterService;
		}

		public override ValueTask HandleAsync(in StartTimerMessage request)
		{
			_counterService.Start(request.ExpectedMessages);
			return new ValueTask(Task.CompletedTask);
		}
	}
}
