using System;

namespace Beffyman.UdpContracts.Serializers
{
	public interface ISerializer
	{
		object Deserialize(in ReadOnlyMemory<byte> sequence, in Type type);
		T Deserialize<T>(in ReadOnlyMemory<byte> sequence);
		ReadOnlyMemory<byte> Serialize<T>(in T obj);
	}
}
