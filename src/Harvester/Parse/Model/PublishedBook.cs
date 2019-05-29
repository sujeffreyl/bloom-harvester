using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BloomHarvester.Parse.Model
{
	[JsonObject]
	public class PublishedBook : ParseObject
	{
		#region Fields and Properties
		[JsonProperty("book")]
		public Pointer<Book> BookPointer { get; set; }

		// This property is a shorthand to set BookPointer's value.
		[JsonIgnore]
		internal Book BookValue
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

		internal string GetJson()
		{
			string pointerJson = "{}";
			if (this.BookPointer != null)
			{
				pointerJson = this.BookPointer.GetJson();
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
