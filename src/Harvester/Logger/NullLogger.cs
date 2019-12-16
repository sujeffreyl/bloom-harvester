using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BloomHarvester.Logger
{
	// This class is used when you don't want to actually log anything and just want to send it to /dev/null instead.
	class NullLogger : IMonitorLogger
	{
		public void Dispose()
		{
		}

		public void LogCritical(string messageFormat, params object[] args)
		{
		}

		public void LogError(string messageFormat, params object[] args)
		{
		}

		public void LogInfo(string messageFormat, params object[] args)
		{
		}

		public void LogVerbose(string messageFormat, params object[] args)
		{
		}

		public void LogWarn(string messageFormat, params object[] args)
		{
		}

		public void TrackEvent(string eventName)
		{
		}
	}
}
