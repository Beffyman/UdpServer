using System;

namespace Beffyman.UdpContracts.Serializers
{
	public interface ISerializer
	{
		object Deserialize(in ReadOnlyMemory<byte> sequence, Type type);
		T Deserialize<T>(in ReadOnlyMemory<byte> sequence);
		ReadOnlyMemory<byte> Serialize<T>(T obj);
	}
}
