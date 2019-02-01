using System;
using System.Collections.Generic;
using System.Text;

namespace Beffyman.UdpServer.Performance.Contracts
{
	public readonly struct SmallMessage
	{
		public readonly int Id;

		public SmallMessage(int id)
		{
			Id = id;
		}
	}
}
