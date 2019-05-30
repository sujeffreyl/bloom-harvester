using System;
using System.Collections.Generic;
using System.Text;
using BloomHarvester.Parse.Model;
using NUnit.Framework;

namespace BloomHarvesterTests.Parse.Model
{
	[TestFixture]
	public class PublishedBookTests
	{
		[Test]
		public void GetJson_NullBook_NoException()
		{
			var obj = new PublishedBook();

			string json = obj.ToJson();

			string expectedJson = "{ \"book\": {}, \"warnings\": [] }";
			Assert.AreEqual(expectedJson, json);
		}

		[Test]
		public void GetJson_BookButNoWarnings_CorrectJson()
		{
			var obj = new PublishedBook();
			var book = new Book()
			{
				ObjectId = "123"
			};
			obj.Book = book;

			string json = obj.ToJson();

			string expectedJson = "{ \"book\": {\"__type\":\"Pointer\",\"className\":\"books\",\"objectId\":\"123\"}, \"warnings\": [] }";
			Assert.AreEqual(expectedJson, json);
		}

		[Test]
		public void GetJson_BookAndWarnings_CorrectJson()
		{
			var obj = new PublishedBook();
			var book = new Book()
			{
				ObjectId = "123",
			};
			obj.Book = book;
			obj.Warnings = new List<string>
			{
				"warning1",
				"warning2"
			};

			string json = obj.ToJson();

			string expectedJson = "{ \"book\": {\"__type\":\"Pointer\",\"className\":\"books\",\"objectId\":\"123\"}, \"warnings\": [\"warning1\", \"warning2\"] }";
			Assert.AreEqual(expectedJson, json);
		}
	}
}
