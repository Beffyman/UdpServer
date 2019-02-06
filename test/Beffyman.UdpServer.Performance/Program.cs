using System;
using BenchmarkDotNet.Running;

namespace Beffyman.UdpServer.Performance
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			try
			{
				var summary = BenchmarkRunner.Run<UdpServerJob>();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}


	}
}
