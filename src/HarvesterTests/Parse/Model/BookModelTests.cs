using System.Collections.Generic;
using BloomHarvester.Parse.Model;
using NUnit.Framework;

namespace BloomHarvesterTests.Parse.Model
{
	[TestFixture]
	public class BookModelTests
	{
		[Test]
		public void BookModel_GetNewBookUpdateOperation_AddsUpdateSource()
		{
			// System under test
			var bookUpdateOp = BookModel.GetNewBookUpdateOperation();
			var result = bookUpdateOp._updatedFieldValues;

			// Verification
			var expectedResult = new Dictionary<string, string>();
			expectedResult.Add("updateSource", "\"bloomHarvester\"");

			CollectionAssert.AreEquivalent(expectedResult, result);
		}
	}
}
