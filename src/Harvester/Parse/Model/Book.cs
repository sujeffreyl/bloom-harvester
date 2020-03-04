using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace BloomHarvester.Parse.Model
{
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	public class Book : ParseObject
	{
		public const string kHarvestStateField = "harvestState";
		public const string kShowField = "show";

		private List<string> _updatedMembers = new List<string>();

		private Book DatabaseVersion { get; set; }

		public Book()
		{
			//UpdateOp = new BookUpdateOperation();
		}

		public Book DeepClone()
		{
			// Dynamic object Show is not easily cloned,
			// so easier for us to do this by serializing/deserializing JSON instead
			string jsonOfThis = JsonConvert.SerializeObject(this);
			var newBook = JsonConvert.DeserializeObject<Book>(jsonOfThis);
			return newBook;
		}

		//internal BookUpdateOperation UpdateOp { get; set; }
		// TODO: DELETE THIS PLACEHOLDER
		// internal List<string> UpdateOp { get; set; }

		// There are many more properties from the book table that we could add when they are needed.
		// When adding a new property, you probably also need to add it to the list of keys selected in ParseClient.cs

		#region Harvester-specific properties
		[JsonProperty(kHarvestStateField)]
		public string HarvestState { get; set; }

		[JsonProperty("harvesterId")]
		public string HarvesterId { get; set; }

		/// <summary>
		/// Represents the major version number of the last Harvester instance that attempted to process this book.
		/// If the major version changes, then we will redo processing even of books that succeeded.
		/// </summary>
		[JsonProperty("harvesterMajorVersion")]
		public int HarvesterMajorVersion { get; set; }

		/// <summary>
		/// Represents the minor version number of the last Harvester instance that attempted to process this book.
		/// If the minor version is updated, then we will redo processing of books that failed
		/// </summary>
		[JsonProperty("harvesterMinorVersion")]
		public int HarvesterMinorVersion { get; set; }


		/// <summary>
		/// The timestamp of the last time Harvester started processing this book
		/// </summary>
		[JsonProperty("harvestStartedAt")]
		public Parse.Model.Date HarvestStartedAt { get; set; }

		[JsonProperty("harvestLog")]
		public List<string> HarvestLogEntries { get; set; }
		#endregion


		#region Non-harvester related properties of the book
		// These properties generally don't have custom setters, since I don't normally expect these to be changed/propagated to the database
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

		[JsonProperty("features")]
		public string[] Features { get; set; }

		private dynamic _show;
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
		[JsonProperty(kShowField)]
		public dynamic Show
		{
			get => _show;
			set
			{
				_show = value;
				_updatedMembers.Add(kShowField);
			}
		}
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

		/// <summary>
		/// Modifies the book with newer metadata
		/// </summary>
		/// <param name="bookFolderPath">The path to find the up-to-date version of the book (which contains the up-to-date meta.json)</param>
		internal void UpdateMetadataIfNeeded(string bookFolderPath)
		{
			var metaData = Bloom.Book.BookMetaData.FromFolder(bookFolderPath);
			UpdateMetadataIfNeeded(metaData);
		}

		/// <summary>
		/// Modifies the book with newer metadata
		/// </summary>
		/// <param name="metaData">The new metadata to update with</param>
		internal void UpdateMetadataIfNeeded(Bloom.Book.BookMetaData metaData)
		{
			if ((this.Features == null && metaData.Features != null)
				|| !this.Features.SequenceEqual(metaData.Features))
			{
				this.Features = metaData.Features;
			}
		}

		public void InitializeDatabaseVersion()
		{
			_updatedMembers.Clear();
			DatabaseVersion = this.DeepClone();
		}

		public void FlushUpdateToDatabase(ParseClient database)
		{
			var pendingUpdates = this.GetPendingUpdates();
			// TODO: Maybe check if there are any worhwhile updates first
			var pendingUpdatesJson = pendingUpdates.ToJson();
			//Console.Error.WriteLine("TODO: Write these updates: " + pendingUpdatesJson);

			database.UpdateObject(this.GetParseClassName(), this.ObjectId, pendingUpdatesJson);

			// TODO: Name should be something different
			InitializeDatabaseVersion();
		}

		/// <summary>
		/// Checks against the old database version to see which fields/properties have been updated and need to be written to the database
		/// Note: Any dynamic objects are probably going to get updated every time. It's hard to compare them.
		/// </summary>
		/// <returns>BookUpdateOperation with the necessary updates</returns>
		internal BookUpdateOperation GetPendingUpdates()
		{
			var updateRequest = new BookUpdateOperation();
			var bookType = typeof(Book);

			List<MemberInfo> fieldsAndProperties =
				bookType.GetFields().Cast<MemberInfo>()
				.Concat(bookType.GetProperties().Cast<MemberInfo>())
				.Where(member => member.Name != "Show")
				.Where(member => member.CustomAttributes.Any())
				.Where(member => member.CustomAttributes.Any(attr => attr.AttributeType.FullName == "Newtonsoft.Json.JsonPropertyAttribute"))
				.ToList();
			
			foreach (var memberInfo in fieldsAndProperties)
			{
				string propertyName = GetMemberJsonName(memberInfo);

				object oldValue;
				object newValue;

				if (memberInfo is FieldInfo)
				{
					var fieldInfo = (FieldInfo)memberInfo;
					oldValue = fieldInfo.GetValue(this.DatabaseVersion);
					newValue = fieldInfo.GetValue(this);
				}
				else
				{
					// We know that everything here is supposed to be either a FieldInfo or PropertyInfo,
					// so if it's not FieldInfo, it should be a propertyInfo
					var propertyInfo = (PropertyInfo)memberInfo;
					oldValue = propertyInfo.GetValue(this.DatabaseVersion);
					newValue = propertyInfo.GetValue(this);
				}

				if (!AreObjectsEqual(oldValue, newValue))
				{
					updateRequest.UpdateFieldWithObject(propertyName, newValue);
				}
			}

			// Now we handle ones that are manually tracked using _updatedMembers.
			// This is designed for dynamic objects, for which the default Equals() function will probably not do what we want (since it's just a ref comparison)
			foreach (var updatedMemberName in _updatedMembers ?? Enumerable.Empty<string>())
			{
				FieldInfo fieldInfo = bookType.GetField(updatedMemberName);
				if (fieldInfo != null)
				{
					string memberName = GetMemberJsonName(fieldInfo);
					object newValue = fieldInfo.GetValue(this);
					updateRequest.UpdateFieldWithObject(memberName, newValue);
				}
				else
				{
					PropertyInfo propertyInfo = bookType.GetProperty(updatedMemberName);
					if (propertyInfo != null)
					{
						string memberName = GetMemberJsonName(propertyInfo);
						object newValue = propertyInfo.GetValue(this);
						updateRequest.UpdateFieldWithObject(memberName, newValue);
					}
				}
			}

			return updateRequest;
		}

		internal static bool AreObjectsEqual(object obj1, object obj2)
		{
			// Careful, oldValue may be null. Make sure to avoid NullReferenceExceptions
			if (obj1 == null)
			{
				return obj2 == null;
			}
			else if (obj2 == null)
			{
				// At this point, we know that obj1 was non-null, so if obj2 is null, we know they're different;
				return false;

				// For all code below here, we know that obj1 and obj2 are both non-null
			}
			else if (obj1.GetType().IsArray)
			{
				var array1 = (object[])obj1;
				var array2 = (object[])obj2;

				if (array1.Length != array2.Length)
				{
					// Different lengths... definitely not equal
					return false;
				}
				else
				{
					// Now we know the lengths are the same. That makes checking this array for equality simpler.
					for (int i = 0; i < array1.Length; ++i)
					{
						if (!array1[i].Equals(array2[i]))
						{
							return false;
						}
					}

					return true;
				}
			}
			else if (obj1 is IList && obj2 is IList)
			{
				var list1 = (IList)obj1;
				var list2 = (IList)obj2;

				if (list1.Count != list2.Count)
				{
					// Different lengths... definitely not equal
					return false;
				}
				else
				{
					// Now we know the lengths are the same. That makes checking this array for equality simpler.
					for (int i = 0; i < list1.Count; ++i)
					{
						if (!list1[i].Equals(list2[i]))
						{
							return false;
						}
					}

					return true;
				}
			}
			else
			{
				// Simple scalars
				return obj1.Equals(obj2);
			}
		}

		private string GetMemberJsonName(MemberInfo memberInfo)
		{
			string name = memberInfo.Name;
			if (memberInfo.CustomAttributes?.FirstOrDefault()?.ConstructorArguments?.Any() == true)
			{
				string jsonMemberName = memberInfo.CustomAttributes.First().ConstructorArguments[0].Value as String;
				if (!String.IsNullOrWhiteSpace(jsonMemberName))
				{
					name = jsonMemberName;
				}
			}

			return name;
		}
	}
}
