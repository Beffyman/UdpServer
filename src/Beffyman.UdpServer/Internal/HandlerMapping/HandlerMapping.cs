using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Beffyman.UdpContracts.Serializers;

namespace Beffyman.UdpServer.Internal.HandlerMapping
{
	internal sealed class HandlerMapping
	{
		public string MessageType { get; private set; }
		public Type ControllerType { get; private set; }
		public HandlerDelegate HandleAsync { get; private set; }


		public HandlerMapping(Type type)
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

			//(object arg, ReadOnlyMemory<byte> data, ISerializer serializer) => ((UdpHandler<T>)arg).HandleAsync<T>(data, serializer);

			//build out expression
			//var arg = Expression.Parameter(controllerType);
			var arg = Expression.Parameter(typeof(object));
			var info = Expression.Parameter(typeof(HandlerInfo));
			var serializer = Expression.Parameter(typeof(ISerializer));

			var castArg = Expression.Convert(arg, controllerType);

			var handle = Expression.Call(castArg, handleMethod, info, serializer);

			HandleAsync = Expression.Lambda<HandlerDelegate>(handle, new ParameterExpression[] { arg, info, serializer }).Compile();
		}



		private Type GetMessageArgument(Type arg)
		{
			if (arg.BaseType == typeof(object)
				&& arg.IsAbstract
				&& arg.IsGenericType
				&& arg.GetGenericTypeDefinition() == typeof(UdpHandler<>))
			{
				return arg.GetGenericArguments()[0];
			}
			else
			{
				return GetMessageArgument(arg.BaseType);
			}
		}

	}
}
