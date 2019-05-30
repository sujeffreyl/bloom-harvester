using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BloomHarvester.Parse.Model
{
	[JsonObject]
	public class Date
	{
		// Constructor
		public Date(DateTime dateTime)
		{
			this.UtcTime = dateTime;
		}

		// Fields and Properties
		public string __type = "Date";
		public string iso;

		private DateTime _utcTime;
		[JsonIgnore]
		public DateTime UtcTime
		{
			get
			{
				return _utcTime;
			}

			set
			{
				_utcTime = value.ToUniversalTime();

				iso = _utcTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK");
			}
		}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
