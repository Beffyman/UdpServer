using System.Threading.Tasks;
using Beffyman.UdpContracts.Serializers.MessagePack;
using Beffyman.UdpServer.Demo.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Beffyman.UdpServer.Demo
{
	public class Program
	{
		public static Task Main(string[] args)
		{
			return UdpHostBuilder.CreateDefaultBuilder(args,
				udpConfig =>
				{
					udpConfig.ReceivePort = 6002;
					//udpConfig.ThreadExecution = HandlerThreadExecution.Inline;
					udpConfig.UseSerializer<UdpMessagePackSerializer>();
				})
				.UseDefaultLogging()
				.ConfigureServices(services =>
				{
					services.AddSingleton<ICounterService, CounterService>();
				})
				.AddUdpHandlers(typeof(Program).Assembly)
				.Build()
				.RunAsync();
		}
	}
}
