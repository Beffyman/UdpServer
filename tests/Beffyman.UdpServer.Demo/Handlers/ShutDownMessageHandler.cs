using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beffyman.UdpServer.Demo.Contracts;
using Microsoft.Extensions.Hosting;

namespace Beffyman.UdpServer.Demo.Handlers
{
	public sealed class ShutDownMessageHandler : UdpHandler<ShutdownMessage>
	{
		private readonly IHostApplicationLifetime _applicationLifetime;

		public ShutDownMessageHandler(IHostApplicationLifetime applicationLifetime)
		{
			_applicationLifetime = applicationLifetime;
		}

		public override Task HandleAsync(ShutdownMessage request)
		{
			_applicationLifetime.StopApplication();

			return Task.CompletedTask;
		}
	}
}
