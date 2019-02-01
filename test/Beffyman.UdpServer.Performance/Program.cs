using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Beffyman.UdpServer.Performance
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<UdpServerJob>();
		}


	}
}
