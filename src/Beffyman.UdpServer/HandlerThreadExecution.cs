using System;
using System.Collections.Generic;
using System.Text;

namespace Beffyman.UdpServer
{
	public enum HandlerThreadExecution
	{
		Dedicated,
		ThreadPool,
		Inline,
	}
}
