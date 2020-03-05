using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomHarvester.Parse
{
	internal class BookUpdateOperation : UpdateOperation
	{
		private const string kUpdateSource = "updateSource";
		internal BookUpdateOperation()
		{
			UpdateFieldWithString(kUpdateSource, "bloomHarvester");
		}

		override internal void Clear()
		{
			base.Clear();
			UpdateFieldWithString(kUpdateSource, "bloomHarvester");
		}

		/// <summary>
		/// Returns true if there are any meaningful updates added (i.e., ignores updateSource, which is automatically added)
		/// </summary>
		internal bool Any()
		{
			int expectedMinCount = _updatedFieldValues.ContainsKey(kUpdateSource) ? 1 : 0;
			bool areAny = _updatedFieldValues.Count > expectedMinCount;

			return areAny;
		}
	}
}
