using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Beffyman.UdpServer.Internal.Udp
{
	/// <summary>
	/// https://stackoverflow.com/a/4176752
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	internal readonly struct Int32Converter
	{
		//Neat trick here to assign the int into the stack
		//But also say that the next 4 addresses on the same address that we assigned the int to are a group of 4 bytes
		//This is a performant way to get the bytes of an int without using BitConverter

		[FieldOffset(0)]
		public readonly int Value;
		[FieldOffset(0)]
		public readonly byte Byte1;
		[FieldOffset(1)]
		public readonly byte Byte2;
		[FieldOffset(2)]
		public readonly byte Byte3;
		[FieldOffset(3)]
		public readonly byte Byte4;

		public Int32Converter(int value) : this()
		{
			Value = value;
		}

		public Int32Converter(in ReadOnlyMemory<byte> memory) : this()
		{
			if (BitConverter.IsLittleEndian)
			{
				Byte1 = memory.Span[0];
				Byte2 = memory.Span[1];
				Byte3 = memory.Span[2];
				Byte4 = memory.Span[3];
			}
			else
			{
				Byte1 = memory.Span[3];
				Byte2 = memory.Span[2];
				Byte3 = memory.Span[1];
				Byte4 = memory.Span[0];
			}
		}

		public static implicit operator int(Int32Converter value)
		{
			return value.Value;
		}

		public static implicit operator Int32Converter(int value)
		{
			return new Int32Converter(value);
		}
	}
}
