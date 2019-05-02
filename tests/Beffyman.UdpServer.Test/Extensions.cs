using System;
using System.Collections.Generic;
using System.Text;
using KellermanSoftware.CompareNetObjects;
using Xunit;

namespace Beffyman.UdpServer.Test
{
	public static class AssertExtensions
	{

		public static void AreSame<T>(T expected, T actual)
		{
			CompareLogic compareLogic = new CompareLogic();
			ComparisonResult result = compareLogic.Compare(expected, actual);

			Assert.True(result.AreEqual, result.DifferencesString);
		}

		public static void AreNotSame<T>(T expected, T actual)
		{
			CompareLogic compareLogic = new CompareLogic();
			ComparisonResult result = compareLogic.Compare(expected, actual);

			Assert.False(result.AreEqual, result.DifferencesString);
		}

	}
}
