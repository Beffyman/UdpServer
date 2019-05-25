using System;
using System.Collections.Generic;
using System.Text;

namespace Beffyman.UdpServer.Demo.Contracts
{
	public class MyLargeDto
	{
		public MyLargePartDto[] Data { get; set; }
	}

	public class MyLargePartDto
	{
		public Guid Collision { get; set; }
		public int Id { get; set; }
		public DateTime When { get; set; }
		public string Name { get; set; }
	}
}
