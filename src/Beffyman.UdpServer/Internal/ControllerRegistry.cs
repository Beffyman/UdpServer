using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Beffyman.UdpServer.Internal
{
	public class ControllerRegistry
	{
		public IEnumerable<Type> Handlers { get; private set; }

		public ControllerRegistry(IEnumerable<Type> handlers)
		{
			Handlers = handlers;
		}
	}
}
