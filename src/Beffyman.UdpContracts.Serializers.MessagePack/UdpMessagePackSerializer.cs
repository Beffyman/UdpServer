﻿using System;
using MessagePack;
using MessagePack.Resolvers;

namespace Beffyman.UdpContracts.Serializers.MessagePack
{
	public class UdpMessagePackSerializer : ISerializer
	{
		public static readonly UdpMessagePackSerializer Instance = new UdpMessagePackSerializer();

		public object Deserialize(in ReadOnlyMemory<byte> sequence, Type type)
		{
			return MessagePackSerializer.Deserialize(type, sequence.ToArray(), ContractlessStandardResolver.Options);
		}

		public T Deserialize<T>(in ReadOnlyMemory<byte> sequence)
		{
			//Grr, I wish it implemented System.Memory overloads
			return MessagePackSerializer.Deserialize<T>(sequence.ToArray(), ContractlessStandardResolver.Options);
		}

		public ReadOnlyMemory<byte> Serialize<T>(T obj)
		{
			return MessagePackSerializer.Serialize(obj, ContractlessStandardResolver.Options);
		}
	}
}
