﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Beffyman.UdpServer.Demo.Contracts
{
	public class StartTimerMessage
	{
		public int ExpectedMessages;

		public StartTimerMessage() { }

		public StartTimerMessage(int expected)
		{
			ExpectedMessages = expected;
		}
	}
}
