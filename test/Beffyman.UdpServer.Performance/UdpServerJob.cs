using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Hosting;
using Beffyman.UdpContracts;
using Beffyman.UdpServer.Performance.Contracts;
using Beffyman.UdpContracts.Serializers.MessagePack;
using Beffyman.UdpContracts.Serializers.Utf8Json;
using Beffyman.UdpContracts.Serializers.NewtonsoftJson;
using Beffyman.UdpServer.Internal.HandlerMapping;
using Beffyman.UdpServer.Performance.Handlers;
using Beffyman.UdpContracts.Serializers;

namespace Beffyman.UdpServer.Performance
{
	/*
	 * Need to make a performance test for a bunch of messages coming into the server
	 */
	[MemoryDiagnoser]
	public class UdpServerJob
	{
		private HandlerMapping _mapping;

		private ISerializer _serializer;
		private Datagram _dgram;
		private SmallMessageHandler _handler;
		private HandlerInfo _handlerInfo;

		[GlobalSetup]
		public void Setup()
		{
			_mapping = new HandlerMapping(typeof(SmallMessageHandler));
			_serializer = UdpMessagePackSerializer.Instance;
			_dgram = UdpMessage.Create(new SmallMessage(10), _serializer).ToDgram(_serializer);
			_handler = new SmallMessageHandler();
			_handlerInfo = new HandlerInfo(_dgram, IPAddress.Loopback);
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			_mapping = null;
		}

		#region HandlerMapping



		[Benchmark]
		public ValueTask HandlerInvoke()
		{
			//We have an allocation from the deserialize
			return _mapping.HandleAsync(_handler, _handlerInfo, _serializer);
		}

		[Benchmark]
		public Delegate HandlerMapping()
		{
			return new HandlerMapping(typeof(SmallMessageHandler)).HandleAsync;
		}


		#endregion HandlerMapping

		#region Serialization Alloc Benchmarks

		[Benchmark]
		public ReadOnlyMemory<byte> DgramAlloc_MessagePack()
		{
			return UdpMessage.Create(new SmallMessage(10), UdpMessagePackSerializer.Instance).ToDgram(UdpMessagePackSerializer.Instance).Data;
		}

		[Benchmark]
		public UdpMessage UdpMessageAlloc_MessagePack()
		{
			return UdpMessage.Create(new SmallMessage(10), UdpMessagePackSerializer.Instance);
		}

		[Benchmark]
		public ReadOnlyMemory<byte> DgramAlloc_Utf8Json()
		{
			return UdpMessage.Create(new SmallMessage(10), UdpUtf8JsonSerializer.Instance).ToDgram(UdpUtf8JsonSerializer.Instance).Data;
		}

		[Benchmark]
		public UdpMessage UdpMessageAlloc_Utf8Json()
		{
			return UdpMessage.Create(new SmallMessage(10), UdpUtf8JsonSerializer.Instance);
		}

		[Benchmark]
		public ReadOnlyMemory<byte> DgramAlloc_Newtonsoft()
		{
			return UdpMessage.Create(new SmallMessage(10), UdpNewtonsoftJsonSerializer.Instance).ToDgram(UdpNewtonsoftJsonSerializer.Instance).Data;
		}

		[Benchmark]
		public UdpMessage UdpMessageAlloc_Newtonsoft()
		{
			return UdpMessage.Create(new SmallMessage(10), UdpNewtonsoftJsonSerializer.Instance);
		}

		#endregion Serialization Alloc Benchmarks
	}
}
