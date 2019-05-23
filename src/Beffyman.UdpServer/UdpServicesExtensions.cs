using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Beffyman.UdpContracts.Serializers;
using Beffyman.UdpServer.Internal.HandlerMapping;
using Beffyman.UdpServer.Internal.Udp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Beffyman.UdpServer
{
	public static class UdpServicesExtensions
	{
		public static IServiceCollection AddUdpServer(this IServiceCollection services, HostBuilderContext hostBuilderContext, Action<IUdpConfiguration> udpConfiguration)
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

			return services.AddSingleton<IUdpConfiguration>(udpConfig)
				.AddSingleton(typeof(ISerializer), udpConfig.Serializer)
				.AddOptions()
				.AddSingleton<HandlerMapper>()
				.AddSingleton<UdpTransport>()
				.AddSingleton<IUdpSenderFactory, UdpSenderFactory>()
				.AddHostedService<UdpHostedService>()
				.AddUdpHandlers(null);
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

		internal static Type[] GetAllUdpHandlers(Assembly[] handlerAssemblies)
		{
			return (from x in (handlerAssemblies?.ToList() ?? new List<Assembly> { Assembly.GetEntryAssembly() }).SelectMany(x => x.GetTypes())
					let y = x.BaseType
					where !x.IsAbstract && !x.IsInterface &&
					y != null && y.IsGenericType &&
					y.GetGenericTypeDefinition() == typeof(UdpHandler<>)
					select x)
				   .ToArray();
		}


		private static void ThrowInvalidSerializerType(Type serializerType)
		{
			throw new InvalidOperationException($"{serializerType.FullName} does not implement {nameof(ISerializer)}");
		}

		private static void ThrowArgumentNullException(string name)
		{
			throw new ArgumentNullException($"{name} has not been assigned a value, please use {nameof(IUdpConfiguration.UseSerializer)}");
		}

	}
}
