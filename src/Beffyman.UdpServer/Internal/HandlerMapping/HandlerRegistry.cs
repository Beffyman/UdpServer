using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Beffyman.UdpServer.Internal.HandlerMapping
{
	internal sealed class HandlerRegistry
	{
		public IEnumerable<Type> Handlers { get; private set; }

		public HandlerRegistry(IEnumerable<Type> handlers)
		{
			Handlers = handlers;
		}
	}
}
