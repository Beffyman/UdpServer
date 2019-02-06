using System;
using System.Runtime.InteropServices;

namespace Beffyman.UdpContracts
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct Datagram
	{
		public readonly ReadOnlyMemory<byte> Data;
		public readonly int Length;

		public Datagram(in ReadOnlyMemory<byte> data)
		{
			Data = data;
			Length = data.Length;
		}
	}
}
