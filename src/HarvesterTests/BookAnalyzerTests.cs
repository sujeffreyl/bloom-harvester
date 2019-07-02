using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BloomHarvester;
using NUnit.Framework;

namespace BloomHarvesterTests
{
	[TestFixture]
	public class BookAnalyzerTests
	{
		private BookAnalyzer _threeLanguageAnalyzer;
		private BookAnalyzer _twoLanguageAnalyzer;
		private BookAnalyzer _oneLanguageAnalyzer;

		private XElement _threeLanguageCollection;
		[OneTimeSetUp]
		public void Setup()
		{
			var html = @"
<html>
	<head>
	</head>

	<body>
		<div class='bloom-page numberedPage bloom-combinedPage imageOnBottom A5Portrait' data-page='' id='e0b1da23-9771-416f-afdf-5b92faab21c7' data-pagelineage='5dcd48df-e9ab-4a07-afd4-6a24d0398384'>
	        <div class='pageLabel' lang='en'>
	            Picture On Bottom
	        </div>

	        <div class='marginBox'>
	            <div class='bloom-translationGroup bloom-trailingElement normal-style'>
	                <div aria-describedby='qtip-0' data-hasqtip='true' class='bloom-editable bloom-content1 normal-style' contenteditable='true' lang='xk'>
	                    Normal English<br>
	                </div>

	                <div aria-describedby='qtip-1' data-hasqtip='true' class='bloom-editable bloom-content2 normal-style' contenteditable='true' lang='fr'>
	                    Normal Francais<br>
	                </div>

	                <div aria-describedby='qtip-2' data-hasqtip='true' class='bloom-editable bloom-content3 normal-style' contenteditable='true' lang='de'>
	                    Normal Deutsch<br>
	                </div>

	                <div data-hasqtip='true' id='formatButton' style='top: 331.2333526611328px' class='bloom-ui'><img src='file://C:%5CProgram%20Files%20%28x86%29%5CBloom%5CBloomBrowserUI/bookEdit/img/cogGrey.svg'></div>

	                <div id='format-toolbar' class='bloom-ui' style='opacity:0; display:none;'>
	                    <a class='smallerFontButton' id='smaller'><img src='file://C:%5CProgram%20Files%20%28x86%29%5CBloom%5CBloomBrowserUI/bookEdit/img/FontSizeLetter.svg'></a><a id='bigger' class='largerFontButton'><img src='file://C:%5CProgram%20Files%20%28x86%29%5CBloom%5CBloomBrowserUI/bookEdit/img/FontSizeLetter.svg'></a>
	                </div>

	                <div class='bloom-editable' contenteditable='true' lang='z'></div>
	            </div>

	            <div class='bloom-imageContainer bloom-leadingElement'><img style='width: 303px; height: 298px; margin-left: 52px; margin-top: 0px;' src='placeHolder.png' alt='This picture, placeHolder.png, is missing or was loading too slowly.' height='298' width='303'></div>
	        </div>
	    </div>
	</body>
</html>";
			_threeLanguageAnalyzer = new BookAnalyzer(html);
			var twoLangHtml = html.Replace("bloom-content3 ", "");
			_twoLanguageAnalyzer = new BookAnalyzer(twoLangHtml);
			var oneLangHtml = twoLangHtml.Replace("bloom-content2 ", "");
			_oneLanguageAnalyzer = new BookAnalyzer(oneLangHtml);

			_threeLanguageCollection = XElement.Parse(_threeLanguageAnalyzer.BloomCollection);
		}

		[Test]
		public void Language1Code_InBookWithThreeLanguages()
		{
			Assert.That(_threeLanguageAnalyzer.Language1Code, Is.EqualTo("xk"));
		}

		[Test]
		public void Language1Code_InBookWithTwoLanguages()
		{
			Assert.That(_twoLanguageAnalyzer.Language1Code, Is.EqualTo("xk"));
		}
		[Test]
		public void Language1Code_InBookWithOneLanguage()
		{
			Assert.That(_oneLanguageAnalyzer.Language1Code, Is.EqualTo("xk"));
		}

		[Test]
		public void Language2Code_InBookWithThreeLanguages()
		{
			Assert.That(_threeLanguageAnalyzer.Language2Code, Is.EqualTo("fr"));
		}

		[Test]
		public void Language2Code_InBookWithTwoLanguages()
		{
			Assert.That(_twoLanguageAnalyzer.Language2Code, Is.EqualTo("fr"));
		}
		[Test]
		public void Language2Code_InBookWithOneLanguage_en()
		{
			Assert.That(_oneLanguageAnalyzer.Language2Code, Is.EqualTo("en"));
		}

		[Test]
		public void Language3Code_InBookWithThreeLanguages()
		{
			Assert.That(_threeLanguageAnalyzer.Language3Code, Is.EqualTo("de"));
		}

		[Test]
		public void Language3Code_InBookWithTwoLanguages()
		{
			Assert.That(_twoLanguageAnalyzer.Language3Code, Is.Empty);
		}

		[Test]
		public void Language3Code_InBookWithOneLanguage()
		{
			Assert.That(_oneLanguageAnalyzer.Language3Code, Is.Empty);
		}

		[Test]
		public void BookCollection_HasLanguage1Code()
		{
			Assert.That(_threeLanguageCollection.Element("Language1Iso639Code")?.Value, Is.EqualTo("xk"));
		}

		[Test]
		public void BookCollection_HasLanguage2Code()
		{
			Assert.That(_threeLanguageCollection.Element("Language2Iso639Code")?.Value, Is.EqualTo("fr"));
		}

		[Test]
		public void BookCollection_HasLanguage3Code()
		{
			Assert.That(_threeLanguageCollection.Element("Language3Iso639Code")?.Value, Is.EqualTo("de"));
		}
	}
}
