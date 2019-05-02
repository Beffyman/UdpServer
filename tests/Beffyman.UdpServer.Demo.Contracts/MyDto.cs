using System;

namespace Beffyman.UdpServer.Demo.Contracts
{
	public class MyDto
	{
		public Guid Collision { get; set; }
		public int Id { get; set; }
		public DateTime When { get; set; }
		public string Name { get; set; }

		public override string ToString()
		{
			return $"{Id.ToString()} - {When.ToShortTimeString()} - {Name} - {Collision.ToString()}";
		}
	}
}
