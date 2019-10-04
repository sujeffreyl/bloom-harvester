using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Bloom;
using Bloom.Api;
using Bloom.Book;
using Newtonsoft.Json;

namespace BloomHarvester
{
	/// <summary>
	/// Analyze a book and extract various information the harvester needs
	/// </summary>
	public class BookAnalyzer
	{
		private HtmlDom _dom;
		public BookAnalyzer(string html, string meta)
		{
			_dom = new HtmlDom(XmlHtmlConverter.GetXmlDomFromHtml(html, false));
			Language1Code = GetBestLangCode(1) ?? "";
			Language2Code = GetBestLangCode(2) ?? "en";
			Language3Code = GetBestLangCode(3) ?? "";

			var metaObj = DynamicJson.Parse(meta);
			if (metaObj.IsDefined("brandingProjectName"))
			{
				this.Branding = metaObj.brandingProjectName;
			}

			var bloomCollectionElement =
				new XElement("Collection",
					new XElement("Language1Iso639Code", new XText(Language1Code)),
					new XElement("Language2Iso639Code", new XText(Language2Code)),
					new XElement("Language3Iso639Code", new XText(Language3Code)),
					new XElement("BrandingProjectName", new XText(Branding??"")));
			var sb = new StringBuilder();
			using (var writer = XmlWriter.Create(sb))
				bloomCollectionElement.WriteTo(writer);
			BloomCollection = sb.ToString();
		}

		/// <summary>
		/// Gets the language code for the specified language number
		/// </summary>
		/// <param name="x">The language number</param>
		/// <returns>The most frequently occurring value of the "lang" attribute</returns>
		private string GetBestLangCode(int x)
		{
			string xpathString = $"//div[contains(@class, 'bloom-editable') and contains(@class, 'bloom-content{x}') and @lang]";
			string langCode = GetBestLangCodeFromNodeList(_dom.SafeSelectNodes(xpathString));
			return langCode;
		}

		/// <summary>
		/// Gets the most-frequently occuring lang code among the nodes in the specified node list
		/// </summary>
		/// <param name="nodeList"></param>
		/// <returns>The most frequently occuring lang code. May return null</returns>
		public static string GetBestLangCodeFromNodeList(XmlNodeList nodeList)
		{
			if (nodeList == null)
			{
				return null;
			}

			// Get the frequency count for each language code
			var frequencyCounts = new Dictionary<string, uint>();
			for (int i = 0; i < nodeList.Count; ++i)
			{
				var xmlNode = nodeList.Item(i);
				var lang = xmlNode.Attributes["lang"]?.Value;

				if (!frequencyCounts.TryGetValue(lang, out uint count))
				{
					count = 0;
				}
				++count;

				frequencyCounts[lang] = count;
			}

			// Now get the language with the maximum count
			string langWithHighestCount = null;
			uint highestCount = 0;
			foreach (var kvp in frequencyCounts)
			{
				uint count = kvp.Value;
				if (count > highestCount)
				{
					highestCount = count;
					langWithHighestCount = kvp.Key;
				}
			}
			
			return langWithHighestCount;
		}

		public static BookAnalyzer FromFolder(string bookFolder)
		{
			var filename = Path.GetFileName(bookFolder);
			var bookPath = Bloom.Book.BookStorage.FindBookHtmlInFolder(bookFolder);						 
			var metaPath = Path.Combine(bookFolder, "meta.json");
			return new BookAnalyzer(File.ReadAllText(bookPath, Encoding.UTF8),
				File.ReadAllText(metaPath, Encoding.UTF8));
		}

		public string WriteBloomCollection(string bookFolder)
		{
			var collectionFolder = Path.GetDirectoryName(bookFolder);
			var result = Path.Combine(collectionFolder, "temp.bloomCollection");
			File.WriteAllText(result, BloomCollection, Encoding.UTF8);
			return result;
		}

		public string Language1Code { get;}
		public string Language2Code { get; }
		public string Language3Code { get; set; }
		public string Branding { get; }
		public string SubscriptionCode { get; }

		/// <summary>
		/// The content appropriate to a skeleton BookCollection file for this book.
		/// </summary>
		public string BloomCollection { get; set; }
	}
}
