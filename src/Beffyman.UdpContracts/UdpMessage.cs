using System;
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

		public static UdpMessage Create<T>(in T obj, in ISerializer serializer)
		{
			return new UdpMessage(typeof(T).FullName, serializer.Serialize(obj).ToArray());
		}

		public T GetData<T>(in ISerializer serializer)
		{
			return serializer.Deserialize<T>(Data);
		}


		public object GetData(in Type type, in ISerializer serializer)
		{
			return serializer.Deserialize(Data, type);
		}

		public Datagram ToDgram(in ISerializer serializer)
		{
			var dgram = serializer.Serialize(this);

			return new Datagram(dgram);
		}
	}
}
