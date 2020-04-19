using System;
using System.Collections.Generic;
using System.Text;
using Beffyman.UdpContracts;
using Beffyman.UdpContracts.Serializers.Json;
using Xunit;

namespace Beffyman.UdpServer.Test.Serializers
{
	//https://github.com/dotnet/runtime/issues/876
	public class SystemJsonSerializerTests : BaseSerializerTests
	{
		//[Fact]
		//public void UdpMessageSerialization()
		//{
		//	Base_ClassUdpMessageSerialization(UdpJsonSerializer.Instance);
		//}

		//[Fact]
		//public void StructSerializationFailure()
		//{
		//	Base_StructUdpMessageSerialization(UdpJsonSerializer.Instance);
		//}


		//[Fact]
		//public void EmptyStructSerializationFailure()
		//{
		//	Base_EmptyStructUdpMessageSerialization(UdpJsonSerializer.Instance);
		//}
	}
}
