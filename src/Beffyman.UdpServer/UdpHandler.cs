using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beffyman.UdpContracts.Serializers;

namespace Beffyman.UdpServer
{
	public abstract class UdpHandler<T>
	{
		public abstract Task HandleAsync(T request);

		internal Task Handle(ReadOnlyMemory<byte> data, ISerializer serializer)
		{
			return HandleAsync(serializer.Deserialize<T>(data));
		}

	}
}
