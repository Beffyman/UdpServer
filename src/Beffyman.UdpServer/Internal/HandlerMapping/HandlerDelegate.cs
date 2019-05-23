using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beffyman.UdpContracts.Serializers;

namespace Beffyman.UdpServer.Internal.HandlerMapping
{
	internal delegate Task HandlerDelegate(object handler, HandlerInfo messageInfo, ISerializer serializer);
}
