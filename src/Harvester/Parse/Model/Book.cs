using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BloomHarvester.Parse.Model
{
	[JsonObject]
	public class Book : ParseObject
	{
		// TODO: There are many more properties from the book table that we could add when they are needed.

		[JsonProperty("baseUrl")]
		public string BaseUrl;

		// Returns the class name (like a table name) of the class on the Parse server that this object corresponds to
		internal override string GetParseClassName()
		{
			return "books";
		}
	}
}
