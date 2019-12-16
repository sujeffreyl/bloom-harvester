using BloomHarvester.Parse;
using BloomHarvester.Parse.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BloomHarvester
{
	internal class HarvestStateBatchUpdater : Harvester
	{
		private HarvestState NewState { get; set; }

		private HarvestStateBatchUpdater(BatchUpdateStateInParseOptions options)
			: base(new HarvesterOptions()
			{
				QueryWhere = options.QueryWhere,
				ParseDBEnvironment = options.ParseDBEnvironment,
				Environment = EnvironmentSetting.Local,
				SuppressLogs = true
			})
		{
			NewState = options.NewState;
		}

		internal static void RunBatchUpdateStates(BatchUpdateStateInParseOptions options)
		{
			using (HarvestStateBatchUpdater updater = new HarvestStateBatchUpdater(options))
			{
				updater.BatchUpdateStates();
			}
		}

		/// <summary>
		/// This function is here to allow setting the harvesterState to specific values for all rows matching the queryWhere condition
		/// </summary>
		private void BatchUpdateStates()
		{
			IEnumerable<Book> bookList = _parseClient.GetBooks(_options.QueryWhere);

			foreach (var book in bookList)
			{
				_logger.LogInfo($"Updating objectId {book.ObjectId} from {book.HarvestState} to {NewState}");
				
				string newStateStr = "";
				if (NewState != HarvestState.Unknown)
				{
					newStateStr = NewState.ToString();
				}

				var updateOp = new BookUpdateOperation();
				updateOp.UpdateFieldWithString(Book.kHarvestStateField, newStateStr);
				_parseClient.UpdateObject(book.GetParseClassName(), book.ObjectId, updateOp.ToJson());
			}
			_parseClient.FlushBatchableOperations();
		}
	}
}
