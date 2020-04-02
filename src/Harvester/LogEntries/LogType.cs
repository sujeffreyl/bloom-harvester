using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomHarvester.LogEntries
{
	enum LogType
	{
		BloomCLIError
		GetFontsError,
		MissingBaseUrl,
		MissingBloomDigitalIndex,
		MissingFont,
		TimeoutError
	}
}
