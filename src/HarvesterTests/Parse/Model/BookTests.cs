using System;
using System.Collections.Generic;
using BloomHarvester;
using BloomHarvester.Parse;
using BloomHarvester.Parse.Model;
using Newtonsoft.Json;
using NUnit.Framework;
using VSUnitTesting = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BloomHarvesterTests.Parse.Model
{
	[TestFixture]
	public class BookTests
	{
		[Test]
		public void SetHarvesterEvaluation_InsertsProperly()
		{
			// Setup
			var book = new Book()
			{
				BaseUrl = "www.amazon.com",
				HarvestState = "Done",
				HarvestStartedAt = new Date(new DateTime(2019, 9, 11, 0, 0, 0, DateTimeKind.Utc)),
				HarvesterMajorVersion = 1,
				HarvesterMinorVersion = 0,
				HarvestLogEntries = new List<string>()
			};

			// System under test
			book.SetHarvesterEvaluation("epub", false);
			book.SetHarvesterEvaluation("pdf", true);
			book.SetHarvesterEvaluation("bloomReader", true);
			book.SetHarvesterEvaluation("readOnline", true);

			//Verify
			string bookJson = JsonConvert.SerializeObject(book);
			string expectedJsonFragment = "\"show\":{\"epub\":{\"harvester\":false},\"pdf\":{\"harvester\":true},\"bloomReader\":{\"harvester\":true},\"readOnline\":{\"harvester\":true}}";
			Assert.That(bookJson.Contains(expectedJsonFragment), Is.True);
		}

		[Test]
		public void SetHarvesterEvaluation_UpdatesProperly()
		{
			// Setup
			var book = new Book()
			{
				BaseUrl = "www.amazon.com",
				HarvestState = "Done",
				HarvestStartedAt = new Date(new DateTime(2019, 9, 11, 0, 0, 0, DateTimeKind.Utc)),
				HarvesterMajorVersion = 1,
				HarvesterMinorVersion = 0,
				HarvestLogEntries = new List<string>(),
				Show = JsonConvert.DeserializeObject("{\"epub\":{\"harvester\":false, \"librarian\":true, \"user\":true},\"pdf\":{\"harvester\":false, \"librarian\":false, \"user\":false},\"bloomReader\":{\"harvester\":false, \"librarian\":false},\"readOnline\":{\"harvester\":false, \"user\":true}}")
			};

			// System under test
			book.SetHarvesterEvaluation("epub", true);
			book.SetHarvesterEvaluation("pdf", true);
			book.SetHarvesterEvaluation("bloomReader", true);
			book.SetHarvesterEvaluation("readOnline", true);

			//Verify
			string bookJson = JsonConvert.SerializeObject(book);
			string expectedJsonFragment = "\"show\":{\"epub\":{\"harvester\":true,\"librarian\":true,\"user\":true},\"pdf\":{\"harvester\":true,\"librarian\":false,\"user\":false},\"bloomReader\":{\"harvester\":true,\"librarian\":false},\"readOnline\":{\"harvester\":true,\"user\":true}}";
			Assert.That(bookJson.Contains(expectedJsonFragment), Is.True);
		}

		[Test]
		public void GetBloomLibraryBookDetailLink_Prod_PopulatesLink()
		{
			var book = new Book();
			book.ObjectId = "myObjectId";

			string url = book.GetDetailLink(EnvironmentSetting.Prod);

			Assert.That(url, Is.EqualTo("https://bloomlibrary.org/browse/detail/myObjectId"));
		}

		[Test]
		public void GetBloomLibraryBookDetailLink_Dev_PopulatesLink()
		{
			var book = new Book();
			book.ObjectId = "myObjectId";

			string url = book.GetDetailLink(EnvironmentSetting.Dev);

			Assert.That(url, Is.EqualTo("https://dev.bloomlibrary.org/browse/detail/myObjectId"));
		}

		[TestCase(null)]
		[TestCase("")]
		[TestCase(" ")]
		public void GetBloomLibraryBookDetailLink_BadInput_ErrorReported(string badObjectId)
		{
			var book = new Book();
			book.ObjectId = badObjectId;

			string url = book.GetDetailLink(EnvironmentSetting.Dev);

			Assert.That(url, Is.Null);
		}

		//[Test]
		//public void UpdateMetadata_NotEqual_UpdateOpRegistered()
		//{
		//	// Setup
		//	var book = new Book();
		//	book.Features = new string[] { "talkingBook" };
		//	//book.UpdateOp.Clear();
		//	book.InitializeDatabaseVersion();

		//	var metaData = new Bloom.Book.BookMetaData();
		//	metaData.Features = new string[] { "talkingBook", "talkingBook:en" };

		//	// Test
		//	book.UpdateMetadataIfNeeded(metaData);

		//	// Verification
		//	var updateOp = new VSUnitTesting.PrivateObject(book.UpdateOp, new VSUnitTesting.PrivateType(typeof(UpdateOperation)));
		//	var updateDict = (Dictionary<string, string>)updateOp.GetFieldOrProperty("_updatedFieldValues");

		//	var expectedResult = new Dictionary<string, string>();
		//	expectedResult.Add("features", "[\"talkingBook\", \"talkingBook:en\"]");
		//	expectedResult.Add("updateSource", "\"bloomHarvester\"");	// This key should be present so Parse knows it's not a BloomDesktop upload
		//	CollectionAssert.AreEquivalent(updateDict, expectedResult);
		//}

		[Test]
		public void GetPendingUpdates_ModifyAProperty_JsonUpdated()
		{
			var book = new Book();
			book.HarvestState = HarvestState.New.ToString();	// Will be modified
			book.HarvesterMajorVersion = 2;						// Will stay the same

			book.InitializeDatabaseVersion();

			// System under test
			book.HarvestState = HarvestState.InProgress.ToString();
			var pendingUpdatesResult = book.GetPendingUpdates();

			// Verification
			string resultJson = pendingUpdatesResult.ToJson();
			Assert.AreEqual("{\"updateSource\":\"bloomHarvester\",\"harvestState\":\"InProgress\"}", resultJson);
		}

		[Test]
		public void GetPendingUpdates_ModifyAField_JsonUpdated()
		{
			var book = new Book();
			book.Title = "My Test Book";    // This will stay the same
			book.IsInCirculation = true;	// This will be modified

			book.InitializeDatabaseVersion();

			// System under test
			book.IsInCirculation = false;
			var pendingUpdatesResult = book.GetPendingUpdates();

			// Verification
			string resultJson = pendingUpdatesResult.ToJson();
			Assert.AreEqual("{\"updateSource\":\"bloomHarvester\",\"inCirculation\":false}", resultJson);
		}

		[Test]
		public void GetPendingUpdates_NonNullHarvestStartedAt_NoPendingUpdates()
		{
			var book = new Book();
			book.HarvestStartedAt = new Date(new DateTime(2020, 03, 04));

			book.InitializeDatabaseVersion();

			// System under test
			var pendingUpdatesResult = book.GetPendingUpdates();

			// Verification
			string resultJson = pendingUpdatesResult.ToJson();

			// Show was not modified in this test. It shouldn't appear as an update.
			Assert.IsFalse(resultJson.Contains("harvestStartedAt"));
		}

		[Test]
		public void GetPendingUpdates_NonNullShow_NoPendingUpdates()
		{
			var book = new Book();
			var jsonString = $"{{ \"pdf\": {{ \"harvester\": true }} }}";
			book.Show = JsonConvert.DeserializeObject(jsonString);

			book.InitializeDatabaseVersion();

			// System under test
			var pendingUpdatesResult = book.GetPendingUpdates();

			// Verification
			string resultJson = pendingUpdatesResult.ToJson();

			// Show was not modified in this test. It shouldn't appear as an update.
			Assert.IsFalse(resultJson.Contains("Show"), "Show");
			Assert.IsFalse(resultJson.Contains("show"), "show");
		}

		[TestCase("a", "a", true)]
		[TestCase("a", "b", false)]
		[TestCase(1, 5/5, true)]	// True - evaluate to same
		[TestCase(1, 2, false)]
		[TestCase(1, 1.0f, false)]	// False - different types
		[TestCase("1", 1, false)]   // False - different types
		public void AreObjectsEqual_ScalarInput_ReturnsCorrectResult(object obj1, object obj2, bool expectedResult)
		{
			bool result = Book.AreObjectsEqual(obj1, obj2);
			Assert.AreEqual(expectedResult, result);
		}

		[Test]
		public void AreObjectsEqual_ArraysSameValuesButDifferentInstances_ReturnsTrue()
		{
			var array1 = new string[] { "a", "b", "c" };

			var list2 = new List<string>();
			list2.Add("a");
			list2.Add("b");
			list2.Add("c");
			var array2 = list2.ToArray();

			bool result = Book.AreObjectsEqual(array1, array2);
			Assert.AreEqual(true, result, "Array");

			var list1 = new List<string>(array1);
			result = Book.AreObjectsEqual(list1, list2);
			Assert.AreEqual(true, result, "List");
		}

		[TestCase(new string[] { "a", "b" }, new string[] { "a", "b", "c" }, TestName = "AreObjectsEqual_ArraysDifferentLengths_ReturnsFalse")]
		[TestCase(new string[] { "a", "b" }, new string[] { "a", "b", "c" }, TestName = "AreObjectsEqual_ArraysSameLengthDifferentValue_ReturnsFalse")]
		[TestCase(null, new string[] { }, TestName = "AreObjectsEqual_Arrays1stIsNull_ReturnsFalse")]
		[TestCase(new string[] { }, null, TestName = "AreObjectsEqual_Arrays2ndIsNull_ReturnsFalse")]
		public void AreObjectsEqual_DifferentArrays_ReturnsFalse(object[] array1, object[] array2)
		{
			bool result = Book.AreObjectsEqual(array1, array2);
			Assert.AreEqual(false, result, "Array");

			// Repeat for lists
			var list1 = array1 != null ? new List<object>(array1) : null;
			var list2 = array2 != null ? new List<object>(array2) : null;
			result = Book.AreObjectsEqual(list1, list2);
			Assert.AreEqual(false, result, "List");
		}
	}
}
