using System;
using System.Collections.Generic;
using System.Linq;
using BloomHarvester;
using NUnit.Framework;

namespace BloomHarvesterTests
{
	[TestFixture]
	class FontCheckerTests
	{
		[Test]
		public void GetMissingFonts_BookContainsEmptyStringFont_NotMarkedAsMissing()
		{
			IEnumerable<string> fontNamesUsedInBook = new string[] { "Arial", "" }; // Assumes that Arial is on the machine running the test

			List<string> missingFontsResult = FontChecker.GetMissingFonts(fontNamesUsedInBook);

			Assert.AreEqual(0, missingFontsResult.Count, "No missing fonts were expected.");
		}
	}
}
