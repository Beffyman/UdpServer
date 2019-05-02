using System;
using System.Collections.Generic;
using System.Text;
using Beffyman.UdpContracts;
using Beffyman.UdpContracts.Serializers.NewtonsoftJson;
using Xunit;

namespace Beffyman.UdpServer.Test.Serializers
{
	public class NewtonsoftJsonSerializerTests : BaseSerializerTests
	{
		[Fact]
		public void UdpMessageSerialization()
		{
			Base_ClassUdpMessageSerialization(UdpNewtonsoftJsonSerializer.Instance);
		}

		[Fact]
		public void StructSerializationFailure()
		{
			Base_StructUdpMessageSerialization(UdpNewtonsoftJsonSerializer.Instance);
		}

		[Fact]
		public void EmptyStructSerializationFailure()
		{
			Base_EmptyStructUdpMessageSerialization(UdpNewtonsoftJsonSerializer.Instance);
		}

	}
}
