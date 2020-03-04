﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BloomHarvester.Parse.Model
{
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	public class Language : ParseObject
	{
		[JsonProperty("isoCode")]
		public string IsoCode { get; set; }

		[JsonProperty("ethnologueCode")]
		public string EthnologueCode { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		public override bool Equals(object obj)
		{
			if (!(obj is Language))
				return false;

			var other = (Language)obj;
			return base.Equals(other)
				&& this.IsoCode == other.IsoCode
				&& this.EthnologueCode == other.EthnologueCode
				&& this.Name == other.Name;
		}

		internal override string GetParseClassName()
		{
			return "language";
		}
	}
}
