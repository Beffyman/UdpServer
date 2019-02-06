using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Beffyman.UdpServer.Internal.HandlerMapping;
using Microsoft.Extensions.Logging;

namespace Beffyman.UdpServer.Internal.Udp
{
	/// <summary>
	/// Clone of the SocketConnection from Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets
	/// </summary>
	internal sealed class UdpConnection : IDisposable
	{
		private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		internal readonly Pipe _pipe;

		private PipeWriter ReceiverPipe => _pipe.Writer;
		private PipeReader ReaderPipe => _pipe.Reader;


		private readonly Socket _socket;
		private readonly ILogger _logger;
		private readonly HandlerMapper _handlerMapper;

		private readonly UdpReceiver _receiver;
		private readonly CancellationTokenSource _connectionClosingCts = new CancellationTokenSource();
		private readonly CancellationTokenSource _connectionClosedTokenSource = new CancellationTokenSource();

		private readonly object _shutdownLock = new object();
		private volatile bool _socketDisposed;
		private volatile Exception _shutdownReason;

		internal UdpConnection(Socket socket,
			MemoryPool<byte> memoryPool,
			PipeScheduler scheduler,
			ILogger logger,
			HandlerMapper handlerMapper)
		{
			Debug.Assert(socket != null);
			Debug.Assert(memoryPool != null);
			Debug.Assert(logger != null);
			Debug.Assert(handlerMapper != null);

			_socket = socket;
			MemoryPool = memoryPool;
			Scheduler = scheduler;
			_logger = logger;
			_handlerMapper = handlerMapper;

			// On *nix platforms, Sockets already dispatches to the ThreadPool.
			// Yes, the IOQueues are still used for the PipeSchedulers. This is intentional.
			// https://github.com/aspnet/KestrelHttpServer/issues/2573
			var awaiterScheduler = IsWindows ? Scheduler : PipeScheduler.Inline;

			_pipe = new Pipe(new PipeOptions(MemoryPool, awaiterScheduler, awaiterScheduler, useSynchronizationContext: false));

			_receiver = new UdpReceiver(_socket, awaiterScheduler);
		}

		public MemoryPool<byte> MemoryPool { get; }
		public PipeScheduler Scheduler { get; }

		public async Task StartAsync()
		{
			try
			{
				// Spawn send and receive logic
				var receiveTask = FillPipeAsync();
				var sendTask = ReadPipeAsync();

				// Now wait for both to complete
				await Task.WhenAll(new Task[] { receiveTask, sendTask });

				_receiver.Dispose();
			}
			catch (Exception ex)
			{
				_logger.LogError(0, ex, $"Unexpected exception in {nameof(UdpConnection)}.{nameof(StartAsync)}.", null);
			}
		}


		public void Dispose()
		{
			_socketDisposed = true;
			_socket.Dispose();
			_connectionClosedTokenSource.Dispose();
			_connectionClosingCts.Dispose();
		}

		private async Task FillPipeAsync()
		{
			Exception error = null;

			try
			{
				await ProcessReceives();
			}
			catch (Exception ex)
				when ((ex is SocketException socketEx && IsConnectionAbortError(socketEx.SocketErrorCode)) ||
					   ex is ObjectDisposedException)
			{
				// This exception should always be ignored because _shutdownReason should be set.
				error = ex;

				if (!_socketDisposed)
				{
					// This is unexpected if the socket hasn't been disposed yet.
					_logger.LogError(error);
				}
			}
			catch (Exception ex)
			{
				// This is unexpected.
				error = ex;
				_logger.LogError(error);
			}
			finally
			{
				// If Shutdown() has already bee called, assume that was the reason ProcessReceives() exited.
				ReceiverPipe.Complete(_shutdownReason ?? error);
			}
		}

		private async Task ProcessReceives()
		{
			while (true)
			{
				//// MacOS blocked on https://github.com/dotnet/corefx/issues/31766
				//if (!IsMacOS)
				//{
				//	// Wait for data before allocating a buffer.
				//	await _receiver.WaitForDataAsync();
				//}

				// Ensure we have some reasonable amount of buffer space
				var buffer = ReceiverPipe.GetMemory(_socket.ReceiveBufferSize);

				var bytesReceived = await _receiver.ReceiveAsync(buffer);

				if (bytesReceived == 0)
				{
					// FIN
					_logger.LogTrace("No bytes received", null);
					break;
				}

				ReceiverPipe.Advance(bytesReceived);

				var flushTask = ReceiverPipe.FlushAsync();

				var result = await flushTask;

				if (result.IsCompleted || result.IsCanceled)
				{
					// Pipe consumer is shut down, do we stop writing
					break;
				}
			}
		}

		private async Task ReadPipeAsync()
		{
			Exception shutdownReason = null;
			Exception unexpectedError = null;

			try
			{
				await ProcessMessagesAsync();
			}
			catch (Exception ex)
				when ((ex is SocketException socketEx && IsConnectionAbortError(socketEx.SocketErrorCode)) ||
					   ex is ObjectDisposedException)
			{
				// This should always be ignored since Shutdown() must have already been called by Abort().
				shutdownReason = ex;
			}
			catch (Exception ex)
			{
				shutdownReason = ex;
				unexpectedError = ex;
				_logger.LogTrace(unexpectedError);
			}
			finally
			{
				Shutdown(shutdownReason);

				// Complete the output after disposing the socket
				ReaderPipe.Complete(unexpectedError);

				// Cancel any pending flushes so that the input loop is un-paused
				ReceiverPipe.CancelPendingFlush();
			}
		}

		private async Task ProcessMessagesAsync()
		{
			while (true)
			{
				var result = await ReaderPipe.ReadAsync();

				if (result.IsCanceled)
				{
					break;
				}

				var buffer = result.Buffer;

				var end = buffer.End;

				var isCompleted = result.IsCompleted;

				if (!buffer.IsEmpty)
				{
					//SequenceReader is not allowed in async methods :(
					foreach (var seg in buffer)
					{
						if (!seg.IsEmpty)
						{
							await _handlerMapper.HandleAsync(seg);
						}
					}
				}

				ReaderPipe.AdvanceTo(end);

				if (isCompleted)
				{
					break;
				}
			}
		}

		private void Shutdown(Exception shutdownReason)
		{
			lock (_shutdownLock)
			{
				if (_socketDisposed)
				{
					return;
				}

				// Make sure to close the connection only after the _aborted flag is set.
				// Without this, the RequestsCanBeAbortedMidRead test will sometimes fail when
				// a BadHttpRequestException is thrown instead of a TaskCanceledException.
				_socketDisposed = true;

				// shutdownReason should only be null if the output was completed gracefully, so no one should ever
				// ever observe the nondescript ConnectionAbortedException except for connection middleware attempting
				// to half close the connection which is currently unsupported.
				_shutdownReason = shutdownReason;

				_logger.LogTrace(_shutdownReason.Message, null);

				try
				{
					// Try to gracefully close the socket even for aborts to match libuv behavior.
					_socket.Shutdown(SocketShutdown.Both);
				}
				catch
				{
					// Ignore any errors from Socket.Shutdown() since we're tearing down the connection anyway.
				}

				_socket.Dispose();
			}
		}

		private static bool IsConnectionAbortError(SocketError errorCode)
		{
			// Calling Dispose after ReceiveAsync can cause an "InvalidArgument" error on *nix.
			return errorCode == SocketError.OperationAborted ||
				   errorCode == SocketError.Interrupted ||
				   (errorCode == SocketError.InvalidArgument && !IsWindows);
		}

	}
}
