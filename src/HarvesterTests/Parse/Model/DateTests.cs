using System;
using System.Collections.Generic;
using System.Text;
using BloomHarvester.Parse.Model;
using Newtonsoft.Json;
using NUnit.Framework;

namespace BloomHarvesterTests.Parse.Model
{
	[TestFixture]
	public class DateTests
	{
		[Test]
		public void GetJson_UtcInput_CorrectJson()
		{
			var parseDate = new Date(new DateTime(2013, 1, 2, 14, 3, 4, 5, DateTimeKind.Utc));

			string resultJson = JsonConvert.SerializeObject(parseDate);

			string expectedJson = "{\"__type\":\"Date\",\"iso\":\"2013-01-02T14:03:04.005Z\"}";
			Assert.AreEqual(expectedJson, resultJson);
		}
	}
}
