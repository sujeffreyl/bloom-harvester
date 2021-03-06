﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BloomHarvester.Parse.Model
{
	[JsonObject(MemberSerialization=MemberSerialization.OptIn)]
	public class Book : ParseObject
	{
		// TODO: There are many more properties from the book table that we could add when they are needed.

		[JsonProperty("baseUrl")]
		public string BaseUrl;

		[JsonProperty("warnings")]
		public List<string> Warnings;

		public const string kHarvestStateField = "harvestState";
		public const string kHarvesterIdField = "harvesterId";
		public const string kWarningsField = "warnings";

		// Returns the class name (like a table name) of the class on the Parse server that this object corresponds to
		internal override string GetParseClassName()
		{
			return "books";
		}

		public enum HarvestState
		{
			Unknown,
			New,
			Updated,
			InProgress,
			Done,
			Failed,
		}
	}
}
