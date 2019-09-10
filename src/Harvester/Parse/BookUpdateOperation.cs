using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomHarvester.Parse
{
	internal class BookUpdateOperation : UpdateOperation
	{
		internal BookUpdateOperation()
		{
			UpdateField("updateSource", "\"bloomHarvester\"");
		}
	}
}
