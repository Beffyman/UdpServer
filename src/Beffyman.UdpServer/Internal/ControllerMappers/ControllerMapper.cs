using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using Beffyman.UdpContracts.Serializers;
using Beffyman.UdpContracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Buffers;

namespace Beffyman.UdpServer.Internal.ControllerMappers
{
	internal sealed class ControllerMapper
	{
		private readonly ISerializer _serializer;
		private readonly ILogger _logger;
		private readonly IServiceProvider _provider;
		private readonly ControllerRegistry _controllerRegistry;

		private readonly IDictionary<string, ControllerHandlerMapping> _mappings;

		public ControllerMapper(ISerializer serializer,
			IServiceProvider provider,
			ILogger<ControllerMapper> logger,
			ControllerRegistry controllerRegistry)
		{
			_serializer = serializer;
			_logger = logger;
			_provider = provider;
			_controllerRegistry = controllerRegistry;

			_mappings = _controllerRegistry.Handlers.Select(x => new ControllerHandlerMapping(x)).ToDictionary(x => x.MessageType, y => y);
		}

		public async Task HandleAsync(ReadOnlyMemory<byte> buffer)
		{
			UdpMessage message;

			try
			{
				message = _serializer.Deserialize<UdpMessage>(buffer);
			}
			catch (Exception ex)
			{
				_logger.LogTrace(ex, $"Failed to deserialize incoming bytes to a {nameof(UdpMessage)}", null);
				return;
			}

			if (!_mappings.ContainsKey(message.Type))
			{
				_logger.LogWarning($"No {nameof(UdpHandler<object>)} setup for type {message.Type}, ignoring.", null);
				return;
			}

			using (var scope = _provider.CreateScope())
			{
				var mapping = _mappings[message.Type];
				var controller = scope.ServiceProvider.GetRequiredService(mapping.ControllerType);

				await mapping.Handle(controller, message.Data, _serializer);
			}

		}

	}
}
