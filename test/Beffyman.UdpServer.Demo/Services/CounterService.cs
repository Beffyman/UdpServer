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
		void Count(int bytes);
	}

	public class CounterService : ICounterService
	{
		private readonly Stopwatch _timer;
		private readonly ILogger _logger;
		private int _count;
		private int _size;
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

		public void Count(int bytes)
		{
			_size += bytes;
			_count++;
		}

		public void Stop()
		{
			_timer.Stop();

			double dropped = (1.0d - (_count / (double)_expected)) * 100;
			string droppedStr;
			if (dropped == double.NegativeInfinity)
			{
				droppedStr = "0";
			}
			else
			{
				droppedStr = dropped.ToString();
			}

			_logger.LogInformation(
$@"---------------------------------------------------------------------
	Expected {_expected.ToString()} messages.
	Handled {_count.ToString()} messages.
	Elapsed time is {_timer.Elapsed.TotalSeconds.ToString()}.
	{droppedStr}% messages dropped.
	{Math.Round(_count / _timer.Elapsed.TotalSeconds, 2)} Messages/sec
	{_size} bytes
	{Math.Round((_size / 1024) / _timer.Elapsed.TotalSeconds, 2)} megabytes/sec
---------------------------------------------------------------------");
			_count = 0;
			_size = 0;
			_expected = 0;
		}
	}
}
