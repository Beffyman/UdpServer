using System.Runtime.InteropServices;
using Beffyman.UdpContracts.Serializers;

namespace Beffyman.UdpContracts
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct UdpMessage
	{
		public readonly string Type;
		public readonly byte[] Data;

		public UdpMessage(string type, byte[] data)
		{
			Type = type;
			Data = data;
		}

		public static UdpMessage Create<T>(T obj, ISerializer serializer)
		{
			return new UdpMessage(typeof(T).FullName, serializer.Serialize(obj).ToArray());
		}

		public Datagram ToDgram(ISerializer serializer)
		{
			var dgram = serializer.Serialize(this);

			return new Datagram(dgram);
		}
	}
}
