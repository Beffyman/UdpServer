using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Beffyman.UdpContracts.Serializers;

namespace Beffyman.UdpServer.Internal.ControllerMappers
{
	internal sealed class ControllerHandlerMapping
	{
		public string MessageType { get; private set; }
		public Type ControllerType { get; private set; }
		public Func<object, ReadOnlyMemory<byte>, ISerializer, Task> Handle { get; private set; }


		public ControllerHandlerMapping(Type type)
		{
			ControllerType = type;
			GenerateHandler();
		}

		private void GenerateHandler()
		{
			var messageType = GetMessageArgument(ControllerType);

			MessageType = messageType.FullName;

			var controllerType = typeof(UdpHandler<>).MakeGenericType(new Type[] { messageType });

			var handleMethod = controllerType.GetMethod(nameof(UdpHandler<object>.Handle), BindingFlags.NonPublic | BindingFlags.Instance);

			//(object arg, ReadOnlyMemory<byte> data, ISerializer serializer) => ((UdpHandler<T>)arg).Handle<T>(data, serializer);

			//build out expression
			//var arg = Expression.Parameter(controllerType);
			var arg = Expression.Parameter(typeof(object));
			var data = Expression.Parameter(typeof(ReadOnlyMemory<byte>));
			var serializer = Expression.Parameter(typeof(ISerializer));

			var castArg = Expression.Convert(arg, controllerType);

			var handle = Expression.Call(castArg, handleMethod, data, serializer);

			Handle = Expression.Lambda<Func<object, ReadOnlyMemory<byte>, ISerializer, Task>>(handle, new ParameterExpression[] { arg, data, serializer }).Compile();
		}



		private Type GetMessageArgument(Type arg)
		{
			if (arg.BaseType == typeof(Object)
				&& arg.IsAbstract
				&& arg.IsGenericType
				&& arg.GetGenericTypeDefinition() == typeof(UdpHandler<>))
			{
				return arg.GetGenericArguments().Single();
			}
			else
			{
				return GetMessageArgument(arg.BaseType);
			}
		}

	}
}
