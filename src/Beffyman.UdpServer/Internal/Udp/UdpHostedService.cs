using System.Threading;
using System.Threading.Tasks;
using Beffyman.UdpServer.Internal;
using Beffyman.UdpServer.Internal.Udp;
using Microsoft.Extensions.Hosting;

namespace Beffyman.UdpServer.Internal.Udp
{
	internal sealed class UdpHostedService : IHostedService
	{
		private readonly UdpTransport _transport;

		public UdpHostedService(UdpTransport transport)
		{
			_transport = transport;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			return _transport.BindAsync();
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await _transport.StopAsync();
			await _transport.UnbindAsync();
		}
	}
}
