using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BloomHarvester.Parse;
using NUnit.Framework;


namespace BloomHarvesterTests.Parse
{
	class UpdateOperationTests
	{
		[Test]
		public void UpdateOperation_AddsString_DeserializesCorrectly()
		{
			// Setup
			var updateOp = new UpdateOperation();
			updateOp.UpdateFieldWithString("field1", "str1");

			// System under test
			string resultJson = updateOp.ToJson();

			// Verify
			string expectedJson = "{\"field1\":\"str1\"}";
			Assert.AreEqual(expectedJson, resultJson);
		}

		[Test]
		public void UpdateOperation_AddsInts_DeserializesCorrectly()
		{
			// Setup
			var updateOp = new UpdateOperation();
			updateOp.UpdateFieldWithNumber("field1", "19");

			// System under test
			string resultJson = updateOp.ToJson();

			// Verify
			string expectedJson = "{\"field1\":19}";
			Assert.AreEqual(expectedJson, resultJson);
		}
	}
}
