using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Beffyman.UdpServer.Internal.HandlerMapping
{
	internal sealed class HandlerRegistry
	{
		public readonly Type[] Handlers;

		public HandlerRegistry(Type[] handlers)
		{
			Handlers = handlers;
		}
	}
}
