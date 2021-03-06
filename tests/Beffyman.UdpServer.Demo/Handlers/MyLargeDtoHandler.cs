﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beffyman.UdpServer.Demo.Contracts;
using Beffyman.UdpServer.Demo.Services;
using Microsoft.Extensions.Logging;

namespace Beffyman.UdpServer.Demo.Handlers
{
	public class MyLargeDtoHandler : UdpHandler<MyLargeDto>
	{
		private readonly ILogger _logger;
		private readonly ICounterService _counter;

		public MyLargeDtoHandler(ILogger<MyLargeDtoHandler> logger, ICounterService counter)
		{
			_logger = logger;
			_counter = counter;
		}

		public override Task HandleAsync(MyLargeDto request)
		{
			//_logger.LogTrace(request.ToString());
			_counter.Count(Bytes);
			return Task.CompletedTask;
		}
	}
}
