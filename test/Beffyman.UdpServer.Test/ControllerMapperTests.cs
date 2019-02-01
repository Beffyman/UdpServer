using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beffyman.UdpContracts;
using Beffyman.UdpContracts.Serializers.MessagePack;
using Beffyman.UdpServer.Internal.ControllerMappers;
using Beffyman.UdpServer.Test.Dto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Beffyman.UdpServer.Test
{
	public class ControllerMapperTests : IDisposable
	{
		private readonly IServiceProvider _provider;
		private readonly ILogger<ControllerMapper> _logger;
		private readonly ControllerMapper _controllerMapper;

		private readonly IDictionary<LogLevel, ICollection<string>> _events;

		public ControllerMapperTests()
		{
			_provider = new ServiceCollection().BuildServiceProvider();

			_events = new Dictionary<LogLevel, ICollection<string>>();

			var mockedLogger = new Mock<ILogger<ControllerMapper>>(MockBehavior.Strict);
			mockedLogger.Setup(x => x.Log<object>(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()))
						.Callback((LogLevel logLevel, EventId id, object state, Exception ex, Func<object, Exception, string> formatter) =>
						{
							AddEvent(logLevel, formatter(state, ex));
						});


			_logger = mockedLogger.Object;

			var handlerTypes = new List<Type>();

			_controllerMapper = new ControllerMapper(UdpMessagePackSerializer.Instance, _provider, _logger, new Internal.ControllerRegistry(handlerTypes));
		}

		private void AddEvent(LogLevel level, string msg)
		{
			if (!_events.ContainsKey(level))
			{
				_events.Add(level, new List<string>());
			}

			_events[level].Add(msg);
		}

		public void Dispose()
		{
			_events.Clear();
		}

		[Fact]
		public async Task InvalidByteFormat()
		{
			var buffer = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

			await _controllerMapper.HandleAsync(buffer);

			Assert.Equal($"Failed to deserialize incoming bytes to a {nameof(UdpMessage)}", _events[LogLevel.Trace].Single());
		}

		[Fact]
		public async Task MissingHandler()
		{
			var message = UdpMessage.Create(new ErrorMessageDto
			{
				Id = 5,
				Value = "Hello"
			}, UdpMessagePackSerializer.Instance);

			var dgram = message.ToDgram(UdpMessagePackSerializer.Instance);

			await _controllerMapper.HandleAsync(dgram.Data);

			Assert.Equal($"No {nameof(UdpHandler<object>)} setup for type {message.Type}, ignoring.", _events[LogLevel.Warning].Single());
		}
	}
}
