using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beffyman.UdpServer.Demo.Contracts;
using Beffyman.UdpServer.Demo.Services;
using Microsoft.Extensions.Logging;

namespace Beffyman.UdpServer.Demo.Controllers
{
	public class MyDtoHandler : UdpHandler<MyDto>
	{
		private readonly ILogger _logger;
		private readonly ICounterService _counter;

		public MyDtoHandler(ILogger<MyDtoHandler> logger, ICounterService counter)
		{
			_logger = logger;
			_counter = counter;
		}

		public override Task HandleAsync(MyDto request)
		{
			_logger.LogTrace(request.ToString());
			_counter.Count();
			return Task.CompletedTask;
		}
	}
}
