using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Beffyman.UdpContracts.Serializers;
using Beffyman.UdpServer.Internal.HandlerMapping;
using Beffyman.UdpServer.Internal.Udp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Beffyman.UdpServer
{
	public static class UdpHostBuilder
	{
		public static IHostBuilder CreateDefaultBuilder(Action<IUdpConfiguration> udpConfiguration)
		{
			return CreateDefaultBuilder(null, udpConfiguration);
		}

		public static IHostBuilder CreateDefaultBuilder(string[] args, Action<IUdpConfiguration> udpConfiguration)
		{
			var udpConfig = new UdpConfiguration();

			if (udpConfiguration != null)
			{
				udpConfiguration.Invoke(udpConfig);
			}

			if (udpConfig.Serializer == null)
			{
				ThrowArgumentNullException($"{nameof(IUdpConfiguration)}.{nameof(IUdpConfiguration.Serializer)}");
			}

			if (!typeof(ISerializer).IsAssignableFrom(udpConfig.Serializer))
			{
				ThrowInvalidSerializerType(udpConfig.Serializer);
			}

			return new HostBuilder()
				.ConfigureHostConfiguration(ConfigureHostConfiguration(args))
				.ConfigureAppConfiguration(ConfigureAppConfiguration(args))
				.ConfigureServices(services =>
				{
					services.AddSingleton<IUdpConfiguration>(udpConfig);
					services.AddSingleton(typeof(ISerializer), udpConfig.Serializer);
				})
				.ConfigureServices(ConfigureServices)
				.UseConsoleLifetime();
		}

		public static IHostBuilder UseDefaultLogging(this IHostBuilder builder)
		{
			return builder.ConfigureLogging(ConfigureLogging);
		}

		private static void ThrowInvalidSerializerType(Type serializerType)
		{
			throw new InvalidOperationException($"{serializerType.FullName} does not implement {nameof(ISerializer)}");
		}

		private static void ThrowArgumentNullException(string name)
		{
			throw new ArgumentNullException($"{name} has not been assigned a value, please use {nameof(IUdpConfiguration.UseSerializer)}");
		}

		private static Action<IConfigurationBuilder> ConfigureHostConfiguration(string[] args)
		{
			return (builder) =>
			{
				builder.SetBasePath(Directory.GetCurrentDirectory());
				builder.AddJsonFile("hostsettings.json", optional: true);
				builder.AddEnvironmentVariables();

				if (args != null)
				{
					builder.AddCommandLine(args);
				}
			};
		}

		private static Action<HostBuilderContext, IConfigurationBuilder> ConfigureAppConfiguration(string[] args)
		{
			return (hostContext, builder) =>
			{
				builder.SetBasePath(Directory.GetCurrentDirectory());
				builder.AddJsonFile("appsettings.json", optional: true);
				builder.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
				builder.AddEnvironmentVariables();

				if (args != null)
				{
					builder.AddCommandLine(args);
				}
			};
		}

		private static void ConfigureServices(IServiceCollection services)
		{
			services.AddOptions()
				.AddSingleton<HandlerMapper>()
				.AddSingleton<UdpTransport>()
				.AddHostedService<UdpHostedService>();
		}


		public static IHostBuilder AddUdpHandlers(this IHostBuilder builder, params Assembly[] handlerAssemblies)
		{
			return builder.ConfigureServices(services =>
			{
				services.AddUdpHandlers(handlerAssemblies);
			});
		}

		private static IServiceCollection AddUdpHandlers(this IServiceCollection services, Assembly[] handlerAssemblies)
		{
			var handlers = GetAllUdpHandlers(handlerAssemblies);
			foreach (var handler in handlers)
			{
				services.AddScoped(handler);
			}

			services.AddSingleton<HandlerRegistry>(new HandlerRegistry(handlers));

			return services;
		}

		internal static IEnumerable<Type> GetAllUdpHandlers(Assembly[] handlerAssemblies)
		{
			return (from x in (handlerAssemblies?.ToList() ?? new List<Assembly> { Assembly.GetEntryAssembly() }).SelectMany(x => x.GetTypes())
					let y = x.BaseType
					where !x.IsAbstract && !x.IsInterface &&
					y != null && y.IsGenericType &&
					y.GetGenericTypeDefinition() == typeof(UdpHandler<>)
					select x)
				   .ToArray();
		}


		private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
		{
			builder.AddConfiguration(context.Configuration.GetSection("Logging"));
			builder.AddConsole();
			builder.AddDebug();
		}
	}
}
