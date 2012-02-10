// 
//  Copyright (c) Synrc Research Center.  All rights reserved. 
// 
//  Creating a Custom Synchronization Manager Handler
//  http://msdn.microsoft.com/en-us/library/aa480674.aspx

using Microsoft.SyncCenter;
using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

namespace Synrc
{

	[
	ComVisible(true),
	Guid("2A15A14B-7C61-4f3b-B93E-152E8452B096"), 
    ClassInterface(ClassInterfaceType.AutoDual)
	]
    public class SynrcSyncMgrHandler : ISyncMgrSynchronize
    {
        public static Guid SyncHandlerId
        {
            get
            {
                GuidAttribute guidAttribute = (GuidAttribute)Attribute.GetCustomAttribute(typeof(SynrcSyncMgrHandler), typeof(GuidAttribute));
                return new Guid(guidAttribute.Value);
            }
        }

		string syncHandlerName = "Synrc Sync Contacts";
		bool abortTheUpdate = false;
		Guid itemId;
		SynrcSyncMgrEnumItems enumItems = null;
        private ISyncMgrSynchronizeCallback syncMgrSynchronizeCallback = null;

        public SynrcSyncMgrHandler()
        {
		}

        #region ISyncMgrSynchronize Members

        public int Initialize(int dwReserved, int dwSyncMgrFlags, int cbCookie, IntPtr lpCookie)
        {
            return 0;  // S_OK
        }

        public int GetHandlerInfo(out SyncMgrHandlerInfo ppSyncMgrHandlerInfo)
        {
            ppSyncMgrHandlerInfo = new SyncMgrHandlerInfo();
            ppSyncMgrHandlerInfo.cbSize = Marshal.SizeOf(typeof(SyncMgrHandlerInfo));
            ppSyncMgrHandlerInfo.wszHandlerName = syncHandlerName.PadRight(32,'\0').ToCharArray();
            ppSyncMgrHandlerInfo.SyncMgrHandlerFlags = (int) SyncMgrHandlerFlags.HasProperties;
			ppSyncMgrHandlerInfo.hIcon = Resources.favicon.Handle;
            return 0;
        }

        public int EnumSyncMgrItems(out ISyncMgrEnumItems ppSyncMgrEnumItems)
        {
			enumItems = new SynrcSyncMgrEnumItems();
            ppSyncMgrEnumItems = enumItems;
            return 0;
        }

        public int GetItemObject(ref Guid ItemID, ref Guid riid, out IntPtr ppv)
        {
            ppv = IntPtr.Zero;
            unchecked
            {
                return (int) 0x80004001;  // E_NOTIMPL
            }
        }

        public int ShowProperties(IntPtr hWndParent, ref Guid ItemID)
        {
            if (syncMgrSynchronizeCallback == null)
            {
                unchecked
                {
                    return (int) 0x80004005;  // E_FAIL
                }
            }

			string name = "Unknown";
			if (enumItems != null)
			foreach(SyncItem g in enumItems.items)
			{
				if (g.Guid.Equals(ItemID)) {
					name = g.Name;
					break;
				}
			}

			NativeWindow nw = NativeWindow.FromHandle(hWndParent);
			if (name == "Google Contacts")
			{
				GooglePropertiesForm propertiesForm = new GooglePropertiesForm();
				if (DialogResult.OK == propertiesForm.ShowDialog((IWin32Window)nw))
				{
					// save
				}
			}
			else if (name == "Outlook PIM")
			{
				GooglePropertiesForm propertiesForm = new GooglePropertiesForm();
				if (DialogResult.OK == propertiesForm.ShowDialog((IWin32Window)nw))
				{
					// save
				}
			}

            syncMgrSynchronizeCallback.ShowPropertiesCompleted(0);
            return 0;
        }

        public int SetProgressCallback(ISyncMgrSynchronizeCallback lpCallBack)
        {
            syncMgrSynchronizeCallback = lpCallBack;
            return 0;
        }

        public int PrepareForSync(int cbNumItems, [MarshalAs(UnmanagedType.LPArray, SizeConst=1)] Guid[] pItemIDs, IntPtr hWndParent, int dwReserved)
        {
            if (syncMgrSynchronizeCallback == null)
            {
                unchecked
                {
                    return (int) 0x80004005;  // E_FAIL
                }
            }
            
            itemId = pItemIDs[0];
            syncMgrSynchronizeCallback.PrepareForSyncCompleted(0);
            return 0;
        }

		public void ProgressHandler(string processMessage, int position, int max)
		{
		}

        public int Synchronize(IntPtr hWndParent)
        {
            bool succeeded = true;

            if (syncMgrSynchronizeCallback == null)
            {
                unchecked
                {
                    return (int) 0x80004005;  // E_FAIL
                }
            }

            
            // CALCULATE WHAT AND HOW MUCH TO SYNC

            SyncMgrProgressItem syncMgrProgressItem = new SyncMgrProgressItem();
            syncMgrProgressItem.cbSize = Marshal.SizeOf(typeof(SyncMgrProgressItem));
            syncMgrProgressItem.mask = (int) 
                (StatusType.StatusText 
                | StatusType.StatusType
                | StatusType.ProgValue
                | StatusType.MaxValue);
            syncMgrProgressItem.dwStatusType = (int) SyncMgrStatus.Updating;
            syncMgrProgressItem.iMaxValue = 59; // FETCH1.COUNT + FETCH2.COUNT

            int progvalue = 1;
            IList<string> items = new List<string>();


            foreach (string clientFilename in items)
            {
                syncMgrProgressItem.lpcStatusText = "Deleting file " + clientFilename;
                syncMgrProgressItem.iProgValue = progvalue++;
                int callbackRetval = syncMgrSynchronizeCallback.Progress(ref itemId, ref syncMgrProgressItem);
                if (callbackRetval != 0)
                {
                    // The user has canceled the update; send "skipped" progress.
                    syncMgrProgressItem.dwStatusType = (int) SyncMgrStatus.Skipped;
                    syncMgrSynchronizeCallback.Progress(ref itemId, ref syncMgrProgressItem);

                    unchecked
                    {
                        return (int) 0x80004005;  // E_FAIL
                    }
                }

                Thread.Sleep(500);
            }

            // SET LAST UPDATED

            int retval = 0;  // S_OK
            if (succeeded)
            {
                syncMgrProgressItem.dwStatusType = (int) SyncMgrStatus.Succeeded;
            }
            else
            {
                syncMgrProgressItem.dwStatusType = (int) SyncMgrStatus.Failed;
                unchecked
                {
                    retval = (int) 0x80004005;  // E_FAIL
                }
            }
            syncMgrSynchronizeCallback.Progress(ref itemId, ref syncMgrProgressItem);
            
            if (succeeded)
            {
                syncMgrSynchronizeCallback.SynchronizeCompleted(0);
            }

            return retval;
        }

        public int SetItemStatus(ref Guid pItemID, int dwSyncMgrStatus)
        {
            if (dwSyncMgrStatus == (int) SyncMgrStatus.Skipped)
            {
                abortTheUpdate = true;
            }
            return 0;
        }

        public int ShowError(IntPtr hWndParent, ref Guid errorID)
        {
            if (syncMgrSynchronizeCallback == null)
            {
                unchecked
                {
                    return (int) 0x80004005;  // E_FAIL
                }
            }

            if (0 != syncMgrSynchronizeCallback.EnableModeless(1 /* TRUE */))
            {
                // We weren't given permission to show the dialog.
                unchecked
                {
                    return (int) 0x80004005;  // E_FAIL
                }
            }

            // Specific information about the error could be added here.
            NativeWindow nw = NativeWindow.FromHandle(hWndParent);
            MessageBox.Show((IWin32Window) nw, "The FileSyncHandler was unable to copy all of the files.");
            syncMgrSynchronizeCallback.EnableModeless(0 /* FALSE */);

            Guid[] errorIds = new Guid[1];
            errorIds[0] = errorID;
            syncMgrSynchronizeCallback.ShowErrorCompleted(0, 1, errorIds);

            return 0;
        }

        #endregion

        private void CallbackLogError(string message)
        {
            SyncMgrLogErrorInfo syncMgrLogErrorInfo = new SyncMgrLogErrorInfo();
            syncMgrLogErrorInfo.cbSize = Marshal.SizeOf(typeof(SyncMgrLogErrorInfo));
            syncMgrLogErrorInfo.mask = (int) (SyncMgrErrorInfoMask.ItemId
                | SyncMgrErrorInfoMask.ErrorId
                | SyncMgrErrorInfoMask.ErrorFlags);
            syncMgrLogErrorInfo.ItemID = itemId;
            syncMgrLogErrorInfo.ErrorID = Guid.NewGuid();
            syncMgrLogErrorInfo.dwSyncMgrErrorFlags = (int) SyncmgrErrorFlags.EnableJumpText;

            syncMgrSynchronizeCallback.LogError(
                (int) SyncMgrLogLevel.Error,
                message,
                ref syncMgrLogErrorInfo);
        }

	}
}