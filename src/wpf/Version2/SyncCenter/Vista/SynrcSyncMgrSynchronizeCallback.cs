//-------------------------------------------------------------------------- 
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: SynchronizeCallback.cs
//			
//  Description: Implements the ISyncMgrSynchronizeCallback to allow
//    FileSyncHandler to call back into SyncMgr.
//
//-------------------------------------------------------------------------- 

using Microsoft.SyncCenter;
using System.Windows.Forms;

using System;

namespace Synrc
{
	/// <summary>
	/// Implements the ISyncMgrSynchronizeCallback to allow FileSyncHandler
	/// to call back into SyncMgr.
	/// </summary>
	internal class SyncMgrSynchronizeCallback : ISyncMgrSynchronizeCallback
	{
		private SynrcSync fileSync = null;
		/// <summary>
		/// Sets the FileSync object for synchronization.
		/// </summary>
		internal SynrcSync FileSync
		{
			set
			{
				fileSync = value;
			}
		}

		private bool cancelUpdate = false;
		/// <summary>
		///  Sets the cancellation flag.
		/// </summary>
		internal bool CancelUpdate
		{
			set
			{
				cancelUpdate = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the SyncMgrSynchronizeCallback class.
		/// </summary>
		public SyncMgrSynchronizeCallback()
		{
		}

		#region ISyncMgrSynchronizeCallback Members

		/// <summary>
		/// Logs information, a warning, or an error message into the Error tab on
		/// the Synchronization Manager status dialog.
		/// </summary>
		/// <param name="dwErrorLevel">[in] The error level.  Values are taken from the SYNCMGRLOGLEVEL enumeration.</param>
		/// <param name="lpcErrorText">[in] A pointer to error text to be displayed in the error tab.</param>
		/// <param name="lpSyncLogError">[in] A pointer to the SYNCMGRLOGERRORINFO structure that contains additional error information.
		/// Registered applications that do not provide this data can pass NULL.</param>
		public int LogError(int dwErrorLevel, string lpcErrorText, ref SyncMgrLogErrorInfo lpSyncLogError)
		{
			if (fileSync != null)
			{
				SyncErrorLevel level = SyncErrorLevel.Error;
				switch (dwErrorLevel)
				{
					case (int) SyncMgrLogLevel.Information:
						level = SyncErrorLevel.Information;
						break;
					case (int) SyncMgrLogLevel.Warning:
						level = SyncErrorLevel.Warning;
						break;
					case (int) SyncMgrLogLevel.Error:
						level = SyncErrorLevel.Error;
						break;
				}

				fileSync.LogError(level, lpcErrorText);
			}

			return 0;
		}

		/// <summary>
		/// The registered application's handler calls the EstablishConnection method when 
		/// a network connection is required.
		/// </summary>
		/// <param name="lpwszConnection">[in] Identifies the name of the connection to dial.</param>
		/// <param name="dwReserved">[in] Reserved for future use.  Must be set to zero.</param>
		public int EstablishConnection(string lpwszConnection, int dwReserved)
		{
			// Establish the connection here as needed.
			unchecked
			{
				// Return E_FAIL because no connection is currently established.
				return (int) 0x80004005;  // E_FAIL
			}
		}

		/// <summary>
		/// The registered application must call the EnableModeless method before and after any 
		/// dialogs are displayed from within the ISyncMgrSynchronize::PrepareForSync 
		/// and ISyncMgrSynchronize::Synchronize methods.
		/// </summary>
		/// <param name="fEnable">[in] TRUE if the registered application is requesting permission to display a dialog box
		/// or FALSE if the registered application has finished displaying a dialog box.</param>
		public int EnableModeless(int fEnable)
		{
			return 0;
		}

		/// <summary>
		/// The registered application's handler must call the ShowPropertiesCompleted 
		/// method before or after its ShowProperties operation is completed.
		/// </summary>
		/// <param name="hr">[in] indicates whether the ISyncMgrSynchronize::ShowProperties was successful.</param>
		public int ShowPropertiesCompleted(int hr)
		{
			return 0;
		}

		/// <summary>
		/// The registered application's handler calls the DeleteLogError method to delete 
		/// a previously logged ErrorInformation, warning, or error message in the Error 
		/// tab on the Synchronization Manager status dialog.
		/// </summary>
		/// <param name="ErrorID">[in] Identifies LogError to be deleted.  If ErrorID is GUID_NULL,
		/// all errors logged by the instance of the registered application's handler will be deleted.</param>
		/// <param name="dwReserved">[in] Reserved for future use. Must be set to zero.</param>
		public int DeleteLogError(ref Guid ErrorID, int dwReserved)
		{
			return 0;
		}

		/// <summary>
		/// The application must call the SynchronizeCompleted method when its 
		/// ISyncMgrSynchronize::Synchronize method has completed execution.
		/// </summary>
		/// <param name="hr">[in] The returned result from the ISyncMgrSynchronize::Synchronize method.</param>
		public int SynchronizeCompleted(int hr)
		{
			return 0;
		}

		/// <summary>
		/// The registered application's handler must call the ShowErrorCompleted method 
		/// before or after its ISyncMgrSynchronize::PrepareForSync operation has been 
		/// completed.
		/// </summary>
		/// <param name="hr">[in] Indicates whether ISyncMgrSynchronize::ShowError was successful.
		/// This value is S_SYNCMGR_RETRYSYNC if the registered application's handler requires SyncMgr to retry the Synchronization.
		/// When this value is returned to SyncMgr both the ISyncMgrSynchronize::PrepareForSync and ISyncMgrSynchronize::Synchronize methods are called again.</param>
		/// <param name="cbNumItems">[in] Indicates the number of ItemIds in the pItemIDs parameter.
		/// This parameter is ignored unless hrResult is S_SYNCMGR_RETRYSYNC.</param>
		/// <param name="pItemIDs">[in] Pointer to array of ItemIds to pass to ISyncMgrSynchronize::PrepareForSync in the event of a retry.
		/// This parameter is ignored unless hrResult is S_SYNCMGR_RETRYSYNC.</param>
		public int ShowErrorCompleted(int hr, int cbNumItems, Guid[] pItemIDs)
		{
			return 0;
		}

		/// <summary>
		/// The registered application's handler must call the PrepareForSyncCompleted 
		/// method after the ISyncMgrSynchronize::PrepareForSync method has completed 
		/// execution.
		/// </summary>
		/// <param name="hr">[in] The return value of the ISyncMgrSynchronize::PrepareForSync method.
		/// If S_OK is returned, the Synchronization Manager calls ISyncMgrSynchronize::Synchronize for the item.</param>
		public int PrepareForSyncCompleted(int hr)
		{
			return 0;
		}

		/// <summary>
		/// The registered application calls the Progress method to update the progress 
		/// information and determine whether the operation should continue.
		/// </summary>
		/// <param name="pItemID">[in] The item identifier for an item that is being updated.</param>
		/// <param name="lpSyncProgressItem">[in] A pointer to a SYNCMGRPROGRESSITEM structure that contains the updated progress information.</param>
		public int Progress(ref Guid pItemID, ref SyncMgrProgressItem lpSyncProgressItem)
		{
			string statusText = "";
			if (0 != (lpSyncProgressItem.mask & (int) StatusType.StatusText))
			{
				statusText = lpSyncProgressItem.lpcStatusText;
			}

			SyncState syncState = SyncState.NotAvailable;
			if (0 != (lpSyncProgressItem.mask & (int) StatusType.StatusType))
			{
				switch (lpSyncProgressItem.dwStatusType)
				{
					case (int) SyncMgrStatus.Updating:
						syncState = SyncState.Updating;
						break;
					case (int) SyncMgrStatus.Succeeded:
						syncState = SyncState.Succeeded;
						break;
					case (int) SyncMgrStatus.Failed:
						syncState = SyncState.Failed;
						break;
				}
			}

			int progressValue = 0;
			int maxValue = 0;
			if ((0 != (lpSyncProgressItem.mask & (int) StatusType.ProgValue))
				&& (0 != (lpSyncProgressItem.mask & (int) StatusType.MaxValue)))
			{
				progressValue = lpSyncProgressItem.iProgValue;
				maxValue = lpSyncProgressItem.iMaxValue;
			}

			if (fileSync != null)
			{
				fileSync.StatusUpdate(syncState, statusText, progressValue, maxValue);
			}

			int retval = 0;
			if (fileSync.UpdateWasCanceled)
			{
				retval = 0x40203;  // S_SYNCMGR_CANCELITEM
			}
			return retval;
		}

		#endregion
	}
}