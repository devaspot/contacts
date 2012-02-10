//-------------------------------------------------------------------------- 
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: EnumItems.cs
//			
//  Description: Implements the ISyncMgrEnumItems interface to allow SyncMgr
//    to enumerate available synchronization items.
//    FileSyncHandler has only one synchronization item.
//
//-------------------------------------------------------------------------- 

using Microsoft.SyncCenter;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Synrc
{
	/// <summary>
	/// Implements the ISyncMgrEnumItems interface to allow SyncMgr to enumerate available 
	/// synchronization items. FileSyncHandler has only one synchronization item.
	/// </summary>
	public class SynrcSyncMgrEnumItems : ISyncMgrEnumItems
	{
		private Guid itemGuid = new Guid("EC42B65E-4192-4653-8C8F-703A8ED1D28E");
		private int returnedItemCount = 0;

		/// <summary>
		/// Sets the last time that the synchronization operation was performed.
		/// This is shown in the Synchronization Manager dialog.
		/// </summary>
		public DateTime LastUpdated
		{
			set
			{
				lastUpdated = value;
			}
		}
		private DateTime lastUpdated = DateTime.MinValue;

		/// <summary>
		/// Sets the Item name displayed in the Synchronization Manager dialog.
		/// </summary>
		public string SyncItemName
		{
			set
			{
				syncItemName = value;
			}
		}
		private string syncItemName = "";

		/// <summary>
		/// Initializes a new instance of the EnumItems class.
		/// </summary>
		public SynrcSyncMgrEnumItems()
		{
		}

		#region ISyncMgrEnumItems Members

		/// <summary>
		/// Enumerates the next celt elements in the enumerator's list, 
		/// returning them in syncMgrItem along with the actual number of enumerated elements 
		/// in pceltFetched.
		/// </summary>
		/// <param name="celt">[in] Number of items in the array.</param>
		/// <param name="syncMgrItem">[out] Address of array containing items.</param>
		/// <param name="pceltFetched">[out] Address of variable containing actual number of items.</param>
		/// <returns>S_OK if there are items in the syncMgrItem array; S_FALSE otherwise. 
		/// SyncMgr will call this method until it returns S_FALSE.</returns>
		/// 
		public int Next(int celt, out SyncMgrItem syncMgrItem, out int pceltFetched)
		{
			if (returnedItemCount > 0)
			{
				// No more items to return.
				syncMgrItem = new SyncMgrItem();
				pceltFetched = 0;
				return 1;  // S_FALSE
			}

			syncMgrItem = new SyncMgrItem();
			syncMgrItem.cbSize = Marshal.SizeOf(typeof(SyncMgrItem));
			syncMgrItem.dwFlags = 1;  // SYNCMGRITEM_HASPROPERTIES - The item has a properties dialog.

			syncMgrItem.ItemID = itemGuid;
			syncMgrItem.dwItemState = 1;  // SYNCMGRITEMSTATE_CHECKED - Defaults to Checked.
			syncMgrItem.wszItemName = syncItemName.PadRight(128,'\0').ToCharArray();
			// Replace this with an application-appropriate icon as desired.
			syncMgrItem.hIcon = SystemIcons.Information.Handle; 

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
					syncMgrItem.ftLastUpdate.dwHighDateTime = (int) (filetime >> 32);
					syncMgrItem.ftLastUpdate.dwLowDateTime = (int) (filetime & UInt32.MaxValue);
				}
				syncMgrItem.dwFlags |= 8;  // SYNCMGRITEM_LASTUPDATETIME - Indicates lastUpdateTime Field is valid.
			}

			pceltFetched = 1;
			returnedItemCount = 1;
			return 0;  // S_OK
		}

		/// <summary>
		/// Instructs the enumerator to skip the next celt elements in the 
		/// enumeration so the next call to ISyncMgrEnumItems::Next does not return those 
		/// elements.
		/// </summary>
		/// <param name="celt">[in] Number of items to skip.</param>
		public int Skip(int celt)
		{
			returnedItemCount += celt;
			return 0;
		}

		/// <summary>
		/// Instructs the enumerator to position itself at the beginning 
		/// of the list of elements. 
		/// </summary>
		public int Reset()
		{
			returnedItemCount = 0;
			return 0;
		}

		/// <summary>
		/// Creates another items enumerator with the same state as the 
		/// current enumerator to iterate over the same list.  This method makes it 
		/// possible to record a point in the enumeration sequence in order to return to 
		/// that point at a later time.
		/// </summary>
		/// <param name="ppenum">[out] Address of a variable that receives the ISyncMgrEnumItems interface pointer.</param>
		/// <remarks>Not called in this implementation, returns E_NOTIMPL.</remarks>
		public int Clone(out ISyncMgrEnumItems ppenum)
		{
			// Not called by SyncMgr in this implementation.
			ppenum = null;
			unchecked
			{
				return (int) 0x80004001;  // E_NOTIMPL;
			}
		}

		#endregion
	}
}