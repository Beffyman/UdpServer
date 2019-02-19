using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Beffyman.UdpServer.Internal.Udp
{
	public interface IUdpSenderFactory
	{
		IUdpSender GetSender(IPAddress address);
	}

	internal sealed class UdpSenderFactory : IUdpSenderFactory
	{
		private readonly IMemoryCache SenderCache;
		private readonly IUdpConfiguration _configuration;
		private readonly MemoryCacheEntryOptions _options = new MemoryCacheEntryOptions
		{
			SlidingExpiration = TimeSpan.FromMinutes(1)
		};

		public UdpSenderFactory(IUdpConfiguration configuration)
		{
			_configuration = configuration;

			SenderCache = new MemoryCache(new MemoryCacheOptions());
		}


		public IUdpSender GetSender(IPAddress address)
		{
			if (SenderCache.TryGetValue(address, out IUdpSender sender))
			{
				return sender;
			}
			else
			{
				var newSender = new UdpSender(_configuration, address);

				return SenderCache.Set(address, newSender, _options);
			}
		}

	}
}
