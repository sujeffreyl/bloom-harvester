﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BloomHarvester.Parse.Model
{
	// Represents a Parse pointer
	[JsonObject]
	public class Pointer<T> where T : ParseObject
	{
		[JsonProperty(Order=1)]
		public string __type = "Pointer";

		[JsonProperty("className", Order=2)]
		public string ClassName;

		[JsonProperty("objectId", Order=3)]
		public string ObjectId;

		[JsonIgnore]
		public T Value { get; set; }

		public Pointer(T value)
		{
			this.ClassName = value?.GetParseClassName() ?? "";
			this.ObjectId = value?.ObjectId;
			this.Value = value;
		}

		internal string ToJson()
		{
			if (this.Value == null)
			{
				return "{}";
			}

			return "{" + $"\"__type\":\"{__type}\",\"className\":\"{ClassName}\",\"objectId\":\"{ObjectId}\"" + "}";
		}
	}
}
