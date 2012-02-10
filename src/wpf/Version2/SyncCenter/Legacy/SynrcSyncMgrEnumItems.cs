// 
//  Copyright (c) Synrc Research Center.  All rights reserved. 
// 

using Microsoft.SyncCenter;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Synrc
{
	public class SyncItem
	{
		public Guid Guid;
		public string Name;
		public SyncItem(string name, Guid guid)
		{
			Guid = guid;
			Name = name;
		}
	}

	public class SynrcSyncMgrEnumItems : ISyncMgrEnumItems
	{

		public IList<SyncItem> items = null;
		private int returnedItemCount = 0;
		public DateTime LastUpdated { set { lastUpdated = value; } }
		private DateTime lastUpdated = DateTime.MinValue;

		public SynrcSyncMgrEnumItems()
		{
			items = new List<SyncItem>();
			items.Add(new SyncItem("Outlook PIM", new Guid("A6FA7319-CFA5-4a0e-8F79-89A59BCB9C75")));
			items.Add(new SyncItem("Google Contacts", new Guid("4F25C85E-3389-4de5-89F1-3A5FCE25643D")));
		}

		#region ISyncMgrEnumItems Members

		public int Next(int celt, out SyncMgrItem syncMgrItem, out int pceltFetched)
		{
			// No more items to return.
			syncMgrItem = new SyncMgrItem();

			if (returnedItemCount >= items.Count)
			{
				pceltFetched = 0;
				return 1;  // S_FALSE
			}
			else 
			{
				syncMgrItem.cbSize = Marshal.SizeOf(typeof(SyncMgrItem));
				syncMgrItem.dwFlags = 1;  // SYNCMGRITEM_HASPROPERTIES - The item has a properties dialog.
				syncMgrItem.ItemID = items[returnedItemCount].Guid;
				syncMgrItem.dwItemState = 1;  // SYNCMGRITEMSTATE_CHECKED - Defaults to Checked.
				syncMgrItem.wszItemName = items[returnedItemCount].Name.PadRight(128, '\0').ToCharArray();
				syncMgrItem.hIcon = Resources.favicon.Handle;

				if (lastUpdated == DateTime.MinValue)
				{
					syncMgrItem.ftLastUpdate.dwHighDateTime = 0;
					syncMgrItem.ftLastUpdate.dwLowDateTime = 0;
				}
				else
				{
					long filetime = lastUpdated.ToFileTime();
					unchecked  // Suppress range checking of int results.
					{
						syncMgrItem.ftLastUpdate.dwHighDateTime = (int)(filetime >> 32);
						syncMgrItem.ftLastUpdate.dwLowDateTime = (int)(filetime & UInt32.MaxValue);
					}
					syncMgrItem.dwFlags |= 8;  // SYNCMGRITEM_LASTUPDATETIME - Indicates lastUpdateTime Field is valid.
				}

				returnedItemCount++;
				pceltFetched = returnedItemCount;
			}

			return 0;  // S_OK
		}

		public int Skip(int celt)
		{
			returnedItemCount += celt;
			return 0;
		}

		public int Reset()
		{
			returnedItemCount = 0;
			return 0;
		}

		public int Clone(out ISyncMgrEnumItems ppenum)
		{
			ppenum = null;
			unchecked
			{
				return (int) 0x80004001;  // E_NOTIMPL;
			}
		}

		#endregion
	}
}