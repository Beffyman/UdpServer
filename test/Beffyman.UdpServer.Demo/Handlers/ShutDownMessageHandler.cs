﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beffyman.UdpServer.Demo.Contracts;
using Microsoft.Extensions.Hosting;

namespace Beffyman.UdpServer.Demo.Handlers
{
	public sealed class ShutDownMessageHandler : UdpHandler<ShutdownMessage>
	{
		private readonly IApplicationLifetime _applicationLifetime;

		public ShutDownMessageHandler(IApplicationLifetime applicationLifetime)
		{
			_applicationLifetime = applicationLifetime;
		}

		public override ValueTask HandleAsync(in ShutdownMessage request)
		{
			_applicationLifetime.StopApplication();

			return new ValueTask(Task.CompletedTask);
		}
	}
}
