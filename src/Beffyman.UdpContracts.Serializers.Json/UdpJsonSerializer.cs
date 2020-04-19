using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beffyman.UdpContracts.Serializers.Json
{
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
			if (sequence.IsEmpty)
			{
				return Activator.CreateInstance(type);
			}

			return JsonSerializer.Deserialize(sequence.Span, type, _settings);
		}

		public T Deserialize<T>(in ReadOnlyMemory<byte> sequence)
		{
			if (sequence.IsEmpty)
			{
				return default(T);
			}

			return JsonSerializer.Deserialize<T>(sequence.Span, _settings);
		}

		public ReadOnlyMemory<byte> Serialize<T>(T obj)
		{
			return JsonSerializer.SerializeToUtf8Bytes(obj, _settings);
		}
	}
}
