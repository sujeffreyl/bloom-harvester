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

		[Test]
		public void UtcTime_JsonInput_UtcNotLocal()
		{
			string json = "{\"__type\":\"Date\",\"iso\":\"2013-01-02T14:03:04.005Z\"}";

			Date parseDate = JsonConvert.DeserializeObject<Date>(json);

			var expectedDateTime = new DateTime(2013, 1, 2, 14, 3, 4, 5, DateTimeKind.Utc);
			Assert.AreEqual(expectedDateTime, parseDate.UtcTime);
		}

		[Test]
		public void Equals_SameValues_ReturnsTue()
		{
			var date1 = new Date(new DateTime(2013, 1, 2, 14, 3, 4, 5, DateTimeKind.Utc));
			var date2 = new Date(DateTime.Now);
			date2.UtcTime = new DateTime(2013, 1, 2, 14, 3, 4, 5, DateTimeKind.Utc);

			bool result = date1.Equals(date2);

			Assert.AreEqual(true, result);
		}

		[Test]
		public void Equals_DifferentObjects_ReturnsFalse()
		{
			var date1 = new Date(new DateTime(2013, 1, 2, 14, 3, 4, 5, DateTimeKind.Utc));
			var date2 = new Date(DateTime.Now);

			bool result = date1.Equals(date2);

			Assert.AreEqual(false, result);
		}
	}
}
