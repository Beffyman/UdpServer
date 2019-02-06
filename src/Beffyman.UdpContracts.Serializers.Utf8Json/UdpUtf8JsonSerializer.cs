using System;
using System.IO;
using System.Text;
using Utf8Json;

namespace Beffyman.UdpContracts.Serializers.Utf8Json
{
	public class UdpUtf8JsonSerializer : ISerializer
	{
		public static readonly UdpUtf8JsonSerializer Instance = new UdpUtf8JsonSerializer();

		public object Deserialize(in ReadOnlyMemory<byte> sequence, in Type type)
		{
			return JsonSerializer.NonGeneric.Deserialize(type, sequence.ToArray());
		}

		public T Deserialize<T>(in ReadOnlyMemory<byte> sequence)
		{
			return JsonSerializer.Deserialize<T>(sequence.ToArray());
		}

		public ReadOnlyMemory<byte> Serialize<T>(in T obj)
		{
			return JsonSerializer.Serialize<T>(obj);
		}
	}
}
