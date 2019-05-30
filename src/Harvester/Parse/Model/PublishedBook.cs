using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BloomHarvester.Parse.Model
{
	// TODO: Can probably deprecate this class if we decide not to use it.

	[JsonObject]
	public class PublishedBook : ParseObject
	{
		#region Fields and Properties
		[JsonProperty("book")]
		public Pointer<Book> BookPointer { get; set; }

		// This property is a shorthand to set BookPointer's value.
		[JsonIgnore]
		internal Book Book
		{
			get
			{
				return BookPointer?.Value;
			}
			set
			{
				if (BookPointer == null)
				{
					BookPointer = new Pointer<Book>(value);
				}
				else
				{
					BookPointer.Value = value;
				}
			}
		}

		[JsonProperty("warnings")]
		public List<string> Warnings{ get; set; }
		#endregion

		// Returns the class name (like a table name) of the class on the Parse server that this object corresponds to
		internal override string GetParseClassName()
		{
			return "publishedBooks";
		}

		internal override string ToJson()
		{
			string pointerJson = "{}";
			if (this.BookPointer != null)
			{
				pointerJson = this.BookPointer.ToJson();
			}

			string warningsJson = "[]";
			if (this.Warnings?.Count > 0)
			{
				warningsJson = $"[\"{String.Join("\", \"", this.Warnings)}\"]";
			}
			return "{ \"book\": " + pointerJson + ", \"warnings\": " + warningsJson + " }";
		}
	}
}
