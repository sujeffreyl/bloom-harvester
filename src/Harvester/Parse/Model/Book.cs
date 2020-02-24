using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BloomHarvester.Parse.Model
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Book : ParseObject
    {
		public const string kHarvestStateField = "harvestState";
		public const string kHarvesterIdField = "harvesterId";
		public const string kHarvesterMajorVersionField = "harvesterMajorVersion";
		public const string kHarvesterMinorVersionField = "harvesterMinorVersion";
		public const string kHarvestLogField = "harvestLog";
	    public const string kShowField = "show";


		// There are many more properties from the book table that we could add when they are needed.
		// When adding a new property, you probably also need to add it to the list of keys selected in ParseClient.cs

		#region Harvester-specific properties
		[JsonProperty("harvestState")]
		public string HarvestState;

		/// <summary>
		/// Represents the major version number of the last Harvester instance that attempted to process this book.
		/// If the major version changes, then we will redo processing even of books that succeeded.
		/// </summary>
		[JsonProperty("harvesterMajorVersion")]
		public int HarvesterMajorVersion;

		/// <summary>
		/// Represents the minor version number of the last Harvester instance that attempted to process this book.
		/// If the minor version is updated, then we will redo processing of books that failed
		/// </summary>
		[JsonProperty("harvesterMinorVersion")]
		public int HarvesterMinorVersion;

		/// <summary>
		/// The timestamp of the last time Harvester started processing this book
		/// </summary>
		[JsonProperty("harvestStartedAt")]
		public Date HarvestStartedAt;

		[JsonProperty("harvestLog")]
		public List<string> HarvestLogEntries;
		#endregion


		#region Non-harvester related properties of the book
		[JsonProperty("baseUrl")]
		public string BaseUrl;

		[JsonProperty("title")]
		public string Title;

		[JsonProperty("inCirculation")]
		public bool? IsInCirculation;

		[JsonProperty("langPointers")]
		public Language[] Languages;

		[JsonProperty("uploader")]
		public User Uploader;

		/// <summary>
	    /// A json object used to limit what the Library shows the user for each book.
	    /// For example:
	    /// "show": {
	    ///   "epub": {
	    ///     "harvester": true,
		///     "user": true,
		///     "librarian": true,
	    ///   },
		///   "pdf": {
		///     "harvester": true,
		///     "user": false,
		///   },
		///   "bloomReader": {
		///     "harvester": true,
		///     "librarian": false,
		///   },
		///   "readOnline": {
		///     "harvester": true,
		///   }
		/// }
	    /// </summary>
	    /// <remarks>
	    /// Only the harvester values should be changed by this code!
	    /// </remarks>
	    [JsonProperty("show")]
	    public dynamic Show { get; set; }

		#endregion


		// Returns the class name (like a table name) of the class on the Parse server that this object corresponds to
		internal override string GetParseClassName()
        {
            return GetStaticParseClassName();
        }

		internal static string GetStaticParseClassName()
		{
			return "books";
		}

		/// <summary>
		/// Set the harvester evaluation for the given artifact.
		/// Call this method only if harvester created and uploaded the given artifact.
		/// </summary>
	    internal void SetHarvesterEvaluation(string artifact, bool enabled)
	    {
			if (Show == null)
			{
				var jsonString = $"{{ \"{artifact}\": {{ \"harvester\": {enabled.ToString().ToLowerInvariant()} }} }}";
				Show = JsonConvert.DeserializeObject(jsonString);
				return;
			}
		    var setting = JsonConvert.DeserializeObject($"{{ \"harvester\": {enabled.ToString().ToLowerInvariant()} }}");
			switch (artifact)
			{
				case "epub":
					if (Show.epub == null)
						Show.epub = setting;
					else
						Show.epub.harvester = enabled;
					break;
				case "pdf":
					if (Show.pdf == null)
						Show.pdf = setting;
					else
						Show.pdf.harvester = enabled;
					break;
				case "bloomReader":
					if (Show.bloomReader == null)
						Show.bloomReader = setting;
					else
						Show.bloomReader.harvester = enabled;
					break;
				case "readOnline":
					if (Show.readOnline == null)
						Show.readOnline = setting;
					else
						Show.readOnline.harvester = enabled;
					break;
			}

		}

		// Prints out some diagnostic info about the book (for debugging a failed book)
		// environment should be the environment of the BOOK not the Harvester. (i.e., it should probably be _parseDbEnvironment)
		internal string GetBookDiagnosticInfo(EnvironmentSetting environment)
		{
			string diagnosticInfo =
				$"BookId: {this.ObjectId}\n" +
				$"URL: {this.GetDetailLink(environment) ?? "No URL"}\n" +
				$"Title: {this.Title}";

			return diagnosticInfo;
		}

		// Returns the link to the book detail page on Bloom Library
		// If the book's ObjectId is null/etc, this method returns null as well.
		public string GetDetailLink(EnvironmentSetting environment)
		{
			if (String.IsNullOrWhiteSpace(this.ObjectId))
			{
				return null;
			}

			string subdomain;
			switch (environment)
			{
				case EnvironmentSetting.Prod:
					subdomain = "";
					break;
				case EnvironmentSetting.Dev:
				default:
					subdomain = environment.ToString().ToLowerInvariant() + '.';
					break;
			}
			string anchorReference = $"https://{subdomain}bloomlibrary.org/browse/detail/{this.ObjectId}";
			return anchorReference;
		}
	}
}
