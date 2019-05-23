using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beffyman.UdpContracts.Serializers.Json
{
	[Obsolete("Does not currently support structs", true)]
	public class UdpJsonSerializer : ISerializer
	{
		public static readonly UdpJsonSerializer Instance = new UdpJsonSerializer();

		private static readonly JsonSerializerOptions _settings = new JsonSerializerOptions
		{
			IgnoreNullValues = false,
			IgnoreReadOnlyProperties = false
		};

		public object Deserialize(in ReadOnlyMemory<byte> sequence, Type type)
		{
			return JsonSerializer.Parse(sequence.Span, type, _settings);
		}

		public T Deserialize<T>(in ReadOnlyMemory<byte> sequence)
		{
			return JsonSerializer.Parse<T>(sequence.Span, _settings);
		}

		public ReadOnlyMemory<byte> Serialize<T>(T obj)
		{
			return JsonSerializer.ToBytes(obj, _settings);
		}
	}
}
