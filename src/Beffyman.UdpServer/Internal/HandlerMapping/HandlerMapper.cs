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
using Beffyman.UdpServer.Internal.Udp;
using System.Net;

namespace Beffyman.UdpServer.Internal.HandlerMapping
{
	internal sealed class HandlerMapper
	{
		private readonly ISerializer _serializer;
		private readonly ILogger _logger;
		private readonly IServiceProvider _provider;
		private readonly HandlerRegistry _controllerRegistry;
		private readonly IUdpSenderFactory _senderFactory;

		private readonly IDictionary<string, HandlerMapping> _mappings;

		public HandlerMapper(ISerializer serializer,
			IServiceProvider provider,
			ILogger<HandlerMapper> logger,
			IUdpSenderFactory senderFactory,
			HandlerRegistry controllerRegistry)
		{
			_serializer = serializer;
			_logger = logger;
			_provider = provider;
			_controllerRegistry = controllerRegistry;
			_senderFactory = senderFactory;

			_mappings = _controllerRegistry.Handlers.Select(x => new HandlerMapping(x)).ToDictionary(x => x.MessageType, y => y);
		}

		public async ValueTask HandleAsync(ReadOnlyMemory<byte> buffer)
		{
			int addressLengthArrayLengthStart;
			int addressLength;

			ReadOnlyMemory<byte> addressSlice;

			IPAddress address;

			ReadOnlyMemory<byte> messageBuffer;


			try
			{
				addressLengthArrayLengthStart = buffer.Length - sizeof(int);
				addressLength = new Int32Converter(buffer.Slice(addressLengthArrayLengthStart, sizeof(int)));

				addressSlice = buffer.Slice(addressLengthArrayLengthStart - addressLength, addressLength);

				address = new IPAddress(addressSlice.ToArray());//allocating here, sigh, no allocation free way

				messageBuffer = buffer.Slice(0, addressLengthArrayLengthStart - addressLength);

				UdpMessage message;

				try
				{
					message = _serializer.Deserialize<UdpMessage>(messageBuffer);
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

				var info = new HandlerInfo(messageBuffer.Length, message.Data, _senderFactory.GetSender(address));

				using (var scope = _provider.CreateScope())
				{
					var mapping = _mappings[message.Type];
					var controller = scope.ServiceProvider.GetRequiredService(mapping.ControllerType);

					await mapping.HandleAsync(controller, info, _serializer);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex);
			}
		}

	}
}
