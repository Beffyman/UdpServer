using System;
using System.Collections.Generic;
using System.Text;
using Beffyman.UdpContracts;
using Beffyman.UdpContracts.Serializers.Utf8Json;
using Xunit;

namespace Beffyman.UdpServer.Test.Serializers
{
	public class Utf8JsonSerializerTests : BaseSerializerTests
	{
		[Fact]
		public void UdpMessageSerialization()
		{
			Base_ClassUdpMessageSerialization(UdpUtf8JsonSerializer.Instance);
		}

		[Fact]
		public void StructSerializationFailure()
		{
			Base_StructUdpMessageSerialization(UdpUtf8JsonSerializer.Instance);
		}


		[Fact]
		public void EmptyStructSerializationFailure()
		{
			Base_EmptyStructUdpMessageSerialization(UdpUtf8JsonSerializer.Instance);
		}
	}
}
