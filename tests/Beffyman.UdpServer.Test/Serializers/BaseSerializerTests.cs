using System;
using System.Collections.Generic;
using System.Text;
using Beffyman.UdpContracts;
using Beffyman.UdpContracts.Serializers;
using Xunit;

namespace Beffyman.UdpServer.Test.Serializers
{
	public abstract class BaseSerializerTests
	{

		protected void Base_ClassUdpMessageSerialization(ISerializer serializer)
		{
			var dto = new MyBasicDto();

			UdpMessage message = UdpMessage.Create(dto, serializer);

			ReadOnlyMemory<byte> data = serializer.Serialize(message);

			var nongenericMessage = serializer.Deserialize(data, typeof(UdpMessage));
			var genericMessage = serializer.Deserialize<UdpMessage>(data);

			object nongenericDto = null;

			if (nongenericMessage is UdpMessage ngm)
			{
				nongenericDto = ngm.GetData(typeof(MyBasicDto), serializer);
			}
			else
			{
				Assert.True(false, $"{nameof(nongenericMessage)} was not of expected type {nameof(UdpMessage)}.");
			}

			var genericDto = genericMessage.GetData<MyBasicDto>(serializer);

			AssertExtensions.AreSame(dto, nongenericDto);
			AssertExtensions.AreSame(dto, genericDto);
		}

		protected void Base_StructUdpMessageSerialization(ISerializer serializer)
		{
			var myStruct = new MyBasicStruct(1000);

			UdpMessage message = UdpMessage.Create(myStruct, serializer);

			ReadOnlyMemory<byte> data = serializer.Serialize(message);

			var nongenericMessage = serializer.Deserialize(data, typeof(UdpMessage));
			var genericMessage = serializer.Deserialize<UdpMessage>(data);

			object nongenericDto = null;

			if (nongenericMessage is UdpMessage ngm)
			{
				nongenericDto = ngm.GetData(typeof(MyBasicStruct), serializer);
			}
			else
			{
				Assert.True(false, $"{nameof(nongenericMessage)} was not of expected type {nameof(UdpMessage)}.");
			}

			var genericDto = genericMessage.GetData<MyBasicStruct>(serializer);

			AssertExtensions.AreSame(myStruct, nongenericDto);
			AssertExtensions.AreSame(myStruct, genericDto);
		}

		protected void Base_EmptyStructUdpMessageSerialization(ISerializer serializer)
		{
			var myStruct = new MyEmptyStruct();

			UdpMessage message = UdpMessage.Create(myStruct, serializer);

			ReadOnlyMemory<byte> data = serializer.Serialize(message);

			var nongenericMessage = serializer.Deserialize(data, typeof(UdpMessage));
			var genericMessage = serializer.Deserialize<UdpMessage>(data);

			object nongenericDto = null;

			if (nongenericMessage is UdpMessage ngm)
			{
				nongenericDto = ngm.GetData(typeof(MyEmptyStruct), serializer);
			}
			else
			{
				Assert.True(false, $"{nameof(nongenericMessage)} was not of expected type {nameof(UdpMessage)}.");
			}

			var genericDto = genericMessage.GetData<MyEmptyStruct>(serializer);

			AssertExtensions.AreSame(myStruct, nongenericDto);
			AssertExtensions.AreSame(myStruct, genericDto);
		}


		#region Dtos

		public readonly struct MyEmptyStruct
		{

		}

		public readonly struct MyBasicStruct
		{
			public readonly int Id;

			public MyBasicStruct(int id)
			{
				Id = id;
			}
		}

		public class MyBasicDto
		{
			public int Id1 { get; set; }
			public int Id2 { get; set; }
			public int Id3 { get; set; }

			public string Name1 { get; set; }
			public string Name2 { get; set; }

			public DateTime When1 { get; set; }
			public DateTimeOffset When2 { get; set; }

			public MyBasicDto()
			{
				Fill();
			}

			private void Fill()
			{
				Random r = new Random();

				Id1 = r.Next();
				Id2 = r.Next();
				Id3 = r.Next();

				byte[] _name1 = new byte[r.Next(0, 100)];
				r.NextBytes(_name1);
				Name1 = System.Text.Encoding.UTF8.GetString(_name1);

				byte[] _name2 = new byte[r.Next(0, 100)];
				r.NextBytes(_name2);
				Name2 = System.Text.Encoding.UTF8.GetString(_name1);

				When1 = DateTime.UtcNow.AddDays(r.Next(0, 1000)).Date;
				When2 = DateTimeOffset.UtcNow.AddDays(r.Next(0, 1000)).Date;
			}
		}


		#endregion
	}
}
