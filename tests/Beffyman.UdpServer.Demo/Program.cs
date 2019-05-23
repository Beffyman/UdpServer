using System.Threading.Tasks;
using Beffyman.UdpContracts.Serializers.Json;
using Beffyman.UdpContracts.Serializers.MessagePack;
using Beffyman.UdpServer.Demo.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Beffyman.UdpServer.Demo
{
	public class Program
	{
		public static Task Main(string[] args)
		{
			return Host.CreateDefaultBuilder()
				.UseConsoleLifetime()
				.ConfigureHostConfiguration(configBuilder =>
				{
					configBuilder.SetBasePath(Directory.GetCurrentDirectory());
					configBuilder.AddJsonFile("hostsettings.json", optional: true);
					configBuilder.AddEnvironmentVariables();

					if (args != null)
					{
						configBuilder.AddCommandLine(args);
					}
				})
				.ConfigureAppConfiguration((hostBuilderContext, configBuilder) =>
				{
					configBuilder.SetBasePath(Directory.GetCurrentDirectory());
					configBuilder.AddJsonFile("appsettings.json", optional: true);
					configBuilder.AddJsonFile($"appsettings.{hostBuilderContext.HostingEnvironment.EnvironmentName}.json", optional: true);
					configBuilder.AddEnvironmentVariables();

					if (args != null)
					{
						configBuilder.AddCommandLine(args);
					}
				})
				.ConfigureLogging((hostBuilderContext, loggingBuilder) =>
				{
					loggingBuilder.AddConfiguration(hostBuilderContext.Configuration.GetSection("Logging"));
					loggingBuilder.AddConsole();
					loggingBuilder.AddDebug();
				})
				.ConfigureServices((hostBuilder, services) =>
				{
					services.AddUdpServer(hostBuilder, udpConfig =>
					{
						udpConfig.ReceivePort = 6002;
						//udpConfig.ThreadExecution = HandlerThreadExecution.Inline;
						udpConfig.UseSerializer<UdpMessagePackSerializer>();
						//udpConfig.UseSerializer<UdpJsonSerializer>();
					});

					services.AddSingleton<ICounterService, CounterService>();
				})
				.Build()
				.RunAsync();
		}
	}
}
