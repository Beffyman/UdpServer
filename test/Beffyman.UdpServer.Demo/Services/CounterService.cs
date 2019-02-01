using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Beffyman.UdpServer.Demo.Services
{
	public interface ICounterService
	{
		void Start(int expected);
		void Stop();
		void Count();
	}

	public class CounterService : ICounterService
	{
		private readonly Stopwatch _timer;
		private readonly ILogger _logger;
		private int _count;
		private int _expected;

		public CounterService(ILogger<CounterService> logger)
		{
			_timer = new Stopwatch();
			_logger = logger;
		}

		public void Start(int expected)
		{
			_expected = expected;
			_timer.Start();
		}

		public void Count()
		{
			_count++;
		}

		public void Stop()
		{
			_timer.Stop();

			var dropped = 1 - (_count / (double)_expected);
			string droppedStr = null;

			if (dropped == double.NegativeInfinity)
			{
				droppedStr = "0";
			}
			else
			{
				droppedStr = dropped.ToString();
			}

			_logger.LogInformation($"Elapsed time is {_timer.Elapsed.TotalSeconds.ToString()} for {_count.ToString()} messages, {droppedStr}% dropped");
			_count = 0;
		}
	}
}
