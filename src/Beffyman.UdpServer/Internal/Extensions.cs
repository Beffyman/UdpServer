using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Beffyman.UdpServer.Internal.Memory;
using Beffyman.UdpServer.Internal.Udp;
using Microsoft.Extensions.Logging;

namespace Beffyman.UdpServer.Internal
{
	internal static class Extensions
	{
		public static int WriteAddressToSpan(in int packetBytes, in IPAddress ipAddress, in Span<byte> buffer)
		{
			int index = packetBytes;
			//Then we convert the sender address into a byte array
			var addressBytes = ipAddress.GetAddressBytes();

			//Then we write the sender address into the buffer
			for (int i = 0; i < addressBytes.Length; i++)
			{
				buffer[index + i] = addressBytes[i];
			}

			//Then we get the byte array for the length of the address bytes
			var lengthBytes = new Int32Converter(addressBytes.Length);

			//And then write the length onto the end of the buffer
			buffer[index + addressBytes.Length + 0] = lengthBytes.Byte1;
			buffer[index + addressBytes.Length + 1] = lengthBytes.Byte2;
			buffer[index + addressBytes.Length + 2] = lengthBytes.Byte3;
			buffer[index + addressBytes.Length + 3] = lengthBytes.Byte4;

			//We always know the last 4 are the length of the address, then we can splice up and get the address and then the rest above it is the data
			return index + addressBytes.Length + 4;
		}



		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetHandleInformation(in IntPtr hObject, HANDLE_FLAGS dwMask, HANDLE_FLAGS dwFlags);

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
		public static void ThrowArgumentOutOfRangeException(in int sourceLength, in int offset)
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
		internal static void ThrowArgumentOutOfRangeException_BufferRequestTooLarge(in int blockSize)
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
