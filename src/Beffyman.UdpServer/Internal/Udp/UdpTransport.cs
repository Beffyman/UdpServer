using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Beffyman.UdpServer.Internal.HandlerMapping;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Beffyman.UdpServer.Internal.Udp
{
	internal sealed class UdpTransport
	{
		private static readonly PipeScheduler[] ThreadPoolSchedulerArray_Inline = new PipeScheduler[] { PipeScheduler.Inline };
		private static readonly PipeScheduler[] ThreadPoolSchedulerArray_Pool = new PipeScheduler[] { PipeScheduler.ThreadPool };

		private readonly IHostApplicationLifetime _appLifetime;
		private readonly IUdpConfiguration _udpConfiguration;
		private readonly HandlerMapper _handlerMapper;
		private readonly ILogger _logger;

		private readonly MemoryPool<byte> _memoryPool;
		private readonly int _numSchedulers;
		private List<UdpConnection> _connections;
		private readonly PipeScheduler[] _schedulers;
		private Exception _listenException;
		private volatile bool _unbinding;

		public UdpTransport(
			IHostApplicationLifetime applicationLifetime,
			IUdpConfiguration udpConfiguration,
			HandlerMapper handlerMapper,
			ILogger<UdpTransport> logger)
		{
			Debug.Assert(applicationLifetime != null);
			Debug.Assert(udpConfiguration != null);
			Debug.Assert(handlerMapper != null);
			Debug.Assert(logger != null);

			_udpConfiguration = udpConfiguration;
			_appLifetime = applicationLifetime;
			_handlerMapper = handlerMapper;
			_logger = logger;

			_memoryPool = Extensions.GetMemoryPoolFactory();

			var configuration = _udpConfiguration.ThreadExecution;

			if (udpConfiguration.IOQueueCount <= 0)
			{
				configuration = HandlerThreadExecution.ThreadPool;
			}

			switch (configuration)
			{
				case HandlerThreadExecution.Inline:
					_numSchedulers = ThreadPoolSchedulerArray_Inline.Length;
					_schedulers = ThreadPoolSchedulerArray_Inline;
					break;
				case HandlerThreadExecution.ThreadPool:
					_numSchedulers = ThreadPoolSchedulerArray_Pool.Length;
					_schedulers = ThreadPoolSchedulerArray_Pool;
					break;
				case HandlerThreadExecution.Dedicated:
					_numSchedulers = _udpConfiguration.IOQueueCount;
					_schedulers = new IOQueue[_numSchedulers];

					for (var i = 0; i < _numSchedulers; i++)
					{
						_schedulers[i] = new IOQueue();
					}
					break;
			}
		}

		public Task BindAsync()
		{
			List<UdpConnection> connections = new List<UdpConnection>();

			IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, _udpConfiguration.ReceivePort);

			for (var schedulerIndex = 0; schedulerIndex < _numSchedulers; schedulerIndex++)
			{
				try
				{
					Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

					Extensions.DisableHandleInheritance(listenSocket);

					listenSocket.ReceiveBufferSize = _udpConfiguration.ReceiveBufferSize;
					//listenSocket.SendBufferSize = _udpConfiguration.SendBufferSize;
					listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
					listenSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);

					// Kestrel expects IPv6Any to bind to both IPv6 and IPv4
					if (endPoint.Address == IPAddress.IPv6Any)
					{
						listenSocket.DualMode = true;
					}

					try
					{
						listenSocket.Bind(endPoint);
					}
					catch (SocketException e) when (e.SocketErrorCode == SocketError.AddressAlreadyInUse)
					{
						ThrowSocketAlreadyInUse(e);
					}

					// If requested port was "0", replace with assigned dynamic port.
					if (endPoint.Port == 0)
					{
						endPoint = (IPEndPoint)listenSocket.LocalEndPoint;
					}

					var connection = new UdpConnection(listenSocket, _memoryPool, _schedulers[schedulerIndex], _udpConfiguration, _logger, _handlerMapper);
					connections.Add(connection);

					// REVIEW: This task should be tracked by the server for graceful shutdown
					// Today it's handled specifically for http but not for arbitrary middleware
					_ = StartListeningAsync(connection);
				}
				catch (SocketException ex) when (!_unbinding)
				{
					_logger.LogError(ex);
					throw;
				}
			}

			_logger.LogInformation($"Started listening to {endPoint.ToString()} with {_numSchedulers.ToString()} listeners.", null);

			_connections = connections;


			return Task.CompletedTask;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void ThrowSocketAlreadyInUse(SocketException sockEx)
		{
			throw new InvalidOperationException(sockEx.Message, sockEx);
		}

		public async Task UnbindAsync()
		{
			_unbinding = true;

			foreach (var connection in _connections)
			{
				connection.Dispose();

				if (_listenException != null)
				{
					var exInfo = ExceptionDispatchInfo.Capture(_listenException);
					_listenException = null;
					exInfo.Throw();
				}
			}

			_logger.LogInformation($"Stopped listening on port {_udpConfiguration.ReceivePort.ToString()}. {_numSchedulers.ToString()} listeners have been closed.", null);

			_unbinding = false;

			_appLifetime.StopApplication();

			await Task.CompletedTask;
		}

		public Task StopAsync()
		{
			_memoryPool.Dispose();
			return Task.CompletedTask;
		}

		private async Task StartListeningAsync(UdpConnection connection)
		{
			try
			{
				var transportTask = connection.StartAsync();

				await transportTask;

				connection.Dispose();
			}
			catch (Exception ex)
			{
				_logger.LogCritical(ex, $"Unexpected exception in {nameof(UdpTransport)}.{nameof(StartListeningAsync)}.", null);
			}
		}
	}
}
