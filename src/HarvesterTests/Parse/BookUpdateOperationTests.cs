using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BloomHarvester.Parse;
using NUnit.Framework;
using VSUnitTesting = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BloomHarvesterTests.Parse
{
	class BookUpdateOperationTests
	{
		/// <summary>
		/// A convenience method to get at a private variable more easily
		/// </summary>
		internal static Dictionary<string, string> GetUpdatedFieldValues(BookUpdateOperation bookUpdateOp)
		{
			// Note that the type needs to be of the class that actually defined the field. In this case, that's its base class.
			var updateOpExaminer = new VSUnitTesting.PrivateObject(bookUpdateOp, new VSUnitTesting.PrivateType(typeof(UpdateOperation)));
			return (Dictionary<string, string>)updateOpExaminer.GetFieldOrProperty("_updatedFieldValues");
		}

		[Test]
		public void BookUpdateOperation_Constructor_AddsUpdateSource()
		{
			// System under test
			var bookUpdateOp = new BookUpdateOperation();

			// Verification			
			var result = GetUpdatedFieldValues(bookUpdateOp);

			var expectedResult = new Dictionary<string, string>();
			expectedResult.Add("updateSource", "\"bloomHarvester\"");

			CollectionAssert.AreEquivalent(expectedResult, result);
		}

		[Test]
		public void BookUpdateOperation_Clear_UpdateSourceRemains()
		{
			// Setup
			var bookUpdateOp = new BookUpdateOperation();

			// System under test
			bookUpdateOp.Clear();

			// Verification
			var result = GetUpdatedFieldValues(bookUpdateOp);

			var expectedResult = new Dictionary<string, string>();
			expectedResult.Add("updateSource", "\"bloomHarvester\"");

			CollectionAssert.AreEquivalent(expectedResult, result);
		}

		[Test]
		public void BookUpdateOperation_Any_NoNewEntries_ReturnsFalse()
		{
			var bookUpdateOp = new BookUpdateOperation();

			bool result = bookUpdateOp.Any();

			Assert.AreEqual(false, result);
		}

		[Test]
		public void BookUpdateOperation_Any_YesNewEntries_ReturnsTrue()
		{
			var bookUpdateOp = new BookUpdateOperation();
			bookUpdateOp.UpdateFieldWithString("field", "a");

			bool result = bookUpdateOp.Any();

			Assert.AreEqual(true, result);
		}
	}
}
