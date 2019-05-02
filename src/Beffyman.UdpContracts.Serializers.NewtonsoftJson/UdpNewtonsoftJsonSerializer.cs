using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Beffyman.UdpContracts.Serializers.NewtonsoftJson
{
	public class UdpNewtonsoftJsonSerializer : ISerializer
	{
		public static readonly UdpNewtonsoftJsonSerializer Instance = new UdpNewtonsoftJsonSerializer();

		public object Deserialize(in ReadOnlyMemory<byte> sequence, Type type)
		{
			using (var stream = new MemoryStream(sequence.ToArray()))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
				return JsonSerializer.Create().Deserialize(reader, type);
		}

		public T Deserialize<T>(in ReadOnlyMemory<byte> sequence)
		{
			using (var stream = new MemoryStream(sequence.ToArray()))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			using (JsonReader jsonReader = new JsonTextReader(reader))
				return JsonSerializer.Create().Deserialize<T>(jsonReader);
		}

		public ReadOnlyMemory<byte> Serialize<T>(T obj)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			var jsonWriter = new JsonTextWriter(writer)
			{
				CloseOutput = false
			};

			var serializer = new JsonSerializer();
			serializer.Serialize(jsonWriter, obj);
			jsonWriter.Flush();
			writer.Flush();

			return stream.ToArray();
		}
	}
}
