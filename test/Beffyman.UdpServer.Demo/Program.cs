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
					udpConfig.Port = 6002;
					udpConfig.UseSerializer<UdpMessagePackSerializer>();
				})
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
