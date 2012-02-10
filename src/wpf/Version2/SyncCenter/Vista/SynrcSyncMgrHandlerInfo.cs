using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SyncCenter;
using System.Runtime.InteropServices.ComTypes;

namespace Synrc
{
	public class SynrcSyncMgrHandlerInfo : ISyncMgrHandlerInfo
	{
		#region ISyncMgrHandlerInfo Members

		public int GetComment(out string comment)
		{
			comment = "Comment";
			return 0;
		}

		public int GetLastSyncTime(out FILETIME pftLastSync)
		{
			pftLastSync = new FILETIME();
			pftLastSync.dwHighDateTime = 0;
			pftLastSync.dwLowDateTime = 0;
			return 0;
		}

		public int GetType(out SYNCMGR_HANDLER_TYPE pftLastSync)
		{
			pftLastSync = SYNCMGR_HANDLER_TYPE.Application;
			return 0;
		}

		public int GetTypeLabel(out string ppszTypeLabel)
		{
			ppszTypeLabel = "Type Label";
			return 0;
		}

		public int IsActive()
		{
			return 1;
		}

		public int IsConnected()
		{
			return 1;
		}

		public int IsEnabled()
		{
			return 1;
		}

		#endregion
	}
}
