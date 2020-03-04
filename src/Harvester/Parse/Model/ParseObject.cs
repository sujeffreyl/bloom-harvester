using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BloomHarvester.Parse.Model
{
	// Contains common fields to every object in a Parse class
	[JsonObject]
	public abstract class ParseObject
	{
		[JsonProperty("objectId")]
		public string ObjectId { get; set;  }
		//Date createdAt
		//Date updatedAt
		//ACL  ACL

		public override bool Equals(object other)
		{
			return (other is ParseObject) && this.ObjectId == ((ParseObject)other).ObjectId;
		}

		public static bool SafeEquals(object a, object b)
		{
			if (a == null)
			{
				return b == null;
			}
			else
			{
				return a.Equals(b);
			}
		}

		// Returns the class name (like a table name) of the class on the Parse server that this object corresponds to
		internal abstract string GetParseClassName();

		/// <summary>
		/// Serialize the current object to JSON
		/// </summary>
		/// <returns>the JSON string representation of this object</returns>
		internal virtual string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}

		/// <summary>
		/// Utility function to convert a list to JSON
		/// </summary>
		/// <param name="list"></param>
		/// <returns>the JSON string representation</returns>
		internal static string ToJson(IEnumerable<string> list)
		{
			string json = "[]";
			if (list != null && list.Any())
			{
				json = $"[\"{String.Join("\", \"", list)}\"]";
			}

			return json;
		}
	}
}
