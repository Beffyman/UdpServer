using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Beffyman.UdpContracts;
using Beffyman.UdpContracts.Serializers.MessagePack;
using Beffyman.UdpServer.Internal.HandlerMapping;
using Beffyman.UdpServer.Internal.Udp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Beffyman.UdpServer.Test.Server
{
	public class HandlerMapperTests : IDisposable
	{
		private readonly IServiceProvider _provider;
		private readonly ILogger<HandlerMapper> _logger;
		private readonly HandlerMapper _controllerMapper;
		private readonly IUdpConfiguration _configuration;
		private readonly IUdpSenderFactory _senderFactory;


		private readonly IDictionary<LogLevel, ICollection<string>> _events;

		public HandlerMapperTests()
		{
			var configuration = new Mock<IUdpConfiguration>();
			configuration.Setup(x => x.SendPort).Returns(6200);
			configuration.Setup(x => x.SendBufferSize).Returns(250000);
			_configuration = configuration.Object;

			var senderFactory = new Mock<IUdpSenderFactory>();
			senderFactory.Setup(x => x.GetSender(It.IsAny<IPAddress>())).Returns((IUdpSender)default);
			_senderFactory = senderFactory.Object;

			_provider = new ServiceCollection().BuildServiceProvider();

			_events = new Dictionary<LogLevel, ICollection<string>>();

			var mockedLogger = new Mock<ILogger<HandlerMapper>>(MockBehavior.Strict);
			mockedLogger.Setup(x => x.Log<object>(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()))
						.Callback((LogLevel logLevel, EventId id, object state, Exception ex, Func<object, Exception, string> formatter) =>
						{
							AddEvent(logLevel, formatter(state, ex));
						});


			_logger = mockedLogger.Object;

			var handlerTypes = new List<Type>();

			_controllerMapper = new HandlerMapper(UdpMessagePackSerializer.Instance, _provider, _logger, _senderFactory, new HandlerRegistry(handlerTypes));
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
			var bytes = new byte[17] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 0, 0, 0, 0, 0, 0, 0 };

			var buffer = Beffyman.UdpServer.Internal.Extensions.WriteAddressToSpan(9, IPAddress.Loopback, bytes);


			await _controllerMapper.HandleAsync(bytes);

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

			var array = new byte[dgram.Length + 8];

			dgram.Data.CopyTo(array);

			var buffer = Beffyman.UdpServer.Internal.Extensions.WriteAddressToSpan(dgram.Length, IPAddress.Loopback, array);


			await _controllerMapper.HandleAsync(array);

			Assert.Equal($"No {nameof(UdpHandler<object>)} setup for type {message.Type}, ignoring.", _events[LogLevel.Warning].Single());
		}



		#region Dtos

		public class ErrorMessageDto
		{
			public int Id { get; set; }
			public string Value { get; set; }

		}

		#endregion
	}
}
