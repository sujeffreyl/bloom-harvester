using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BloomHarvester.Parse.Model
{
	// Represents a Parse pointer
	[JsonObject]
	public class Pointer<T> where T : ParseObject
	{
		[JsonProperty("__type")]
		public string Type;

		[JsonProperty("className")]
		public string ClassName;

		[JsonProperty("objectId")]
		public string ObjectId;

		[JsonIgnore]
		public T Value { get; set; }

		public Pointer()
		{

		}

		public Pointer(T value)
		{
			this.Value = value;
		}

		internal string GetJson()
		{
			if (this.Value == null)
			{
				return "{}";
			}
			
			return "{ \"__type\": \"Pointer\", \"className\": \"books\", \"objectId\": \"" + this.Value.ObjectId + "\" }"; ;
		}
	}
}
