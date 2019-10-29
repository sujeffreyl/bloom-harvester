using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BloomHarvester.Parse.Model
{
	[JsonObject(MemberSerialization = MemberSerialization.OptOut)]
	public class Language : ParseObject
	{
		public string isoCode;
		public string ethnologueCode;

		internal override string GetParseClassName()
		{
			return "language";
		}
	}
}
