using System;
using System.Collections.Generic;
using System.Text;
using Beffyman.UdpContracts;
using Beffyman.UdpContracts.Serializers.MessagePack;
using Xunit;

namespace Beffyman.UdpServer.Test.Serializers
{
	public class MessagePackSerializerTests : BaseSerializerTests
	{
		[Fact]
		public void UdpMessageSerialization()
		{
			Base_ClassUdpMessageSerialization(UdpMessagePackSerializer.Instance);
		}

		[Fact]
		public void StructSerializationFailure()
		{
			Base_StructUdpMessageSerialization(UdpMessagePackSerializer.Instance);
		}

		[Fact]
		public void EmptyStructSerializationFailure()
		{
			Base_EmptyStructUdpMessageSerialization(UdpMessagePackSerializer.Instance);

			//Throws an internal exception, so can't use (MessagePack.Internal.MessagePackDynamicObjectResolverException)
			//https://github.com/neuecc/MessagePack-CSharp/blob/master/src/MessagePack/Resolvers/DynamicObjectResolver.cs
			//Assert.ThrowsAny<Exception>(() =>
			//{

			//});
		}

	}
}
