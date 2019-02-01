using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Beffyman.UdpServer.Internal.Memory;
using Microsoft.Extensions.Logging;

namespace Beffyman.UdpServer.Internal
{
	internal static class Extensions
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetHandleInformation(IntPtr hObject, HANDLE_FLAGS dwMask, HANDLE_FLAGS dwFlags);

		[Flags]
		private enum HANDLE_FLAGS : uint
		{
			None = 0,
			INHERIT = 1,
			PROTECT_FROM_CLOSE = 2
		}

		internal static void DisableHandleInheritance(Socket socket)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				SetHandleInformation(socket.Handle, HANDLE_FLAGS.INHERIT, 0);
			}
		}

		internal static MemoryPool<byte> GetMemoryPoolFactory()
		{
			return new SlabMemoryPool();
		}

		public static ArraySegment<byte> GetArray(this in Memory<byte> memory)
		{
			return ((ReadOnlyMemory<byte>)memory).GetArray();
		}

		public static ArraySegment<byte> GetArray(this in ReadOnlyMemory<byte> memory)
		{
			if (!MemoryMarshal.TryGetArray(memory, out var result))
			{
				ThrowBufferExpected();
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		internal static void ThrowBufferExpected()
		{
			throw new InvalidOperationException("Buffer backed by array was expected");
		}

		internal enum ExceptionArgument
		{
			size,
			offset,
			length,
			MemoryPoolBlock,
			MemoryPool
		}

		private static string GetArgumentName(ExceptionArgument argument)
		{
			return argument.ToString();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void ThrowArgumentOutOfRangeException(int sourceLength, int offset)
		{

			if ((uint)offset > (uint)sourceLength)
			{
				// Offset is negative or less than array length
				throw new ArgumentOutOfRangeException(GetArgumentName(ExceptionArgument.offset));
			}

			// The third parameter (not passed) length must be out of range
			throw new ArgumentOutOfRangeException(GetArgumentName(ExceptionArgument.length));
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		internal static void ThrowArgumentOutOfRangeException_BufferRequestTooLarge(int blockSize)
		{
			throw new ArgumentOutOfRangeException(GetArgumentName(ExceptionArgument.size), $"Cannot allocate more than {blockSize.ToString()} bytes in a single buffer");
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void ThrowObjectDisposedException(ExceptionArgument argument)
		{
			throw new ObjectDisposedException(GetArgumentName(argument));
		}

		public static void LogTrace(this ILogger logger, Exception ex)
		{
			logger.LogTrace(ex, string.Empty, null);
		}
		public static void LogError(this ILogger logger, Exception ex)
		{
			logger.LogError(ex, ex.Message, null);
		}
	}
}
