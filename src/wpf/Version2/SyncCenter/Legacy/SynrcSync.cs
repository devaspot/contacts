//-------------------------------------------------------------------------- 
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: FileSync.cs
//          
//  Description: Wraps the functionality for calling the FileSync
//    synchronization handler either directly or through SyncMgr.
//
//-------------------------------------------------------------------------- 

using Microsoft.SyncCenter;
using System;

namespace Synrc
{
    /// <summary>
    /// Wraps the functionality for calling the FileSync synchronization handler either
    /// directly or through SyncMgr.
    /// </summary>
    public class SynrcSync
    {
        /// <summary>
        /// Delegate for the syncStatus event.
        /// </summary>
        /// <param name="state">A SyncState value.</param>
        /// <param name="text">The status description.</param>
        /// <param name="progressValue">An integer that indicates the progress value.</param>
        /// <param name="maxValue">An integer that indicates the maximum progress value.</param>
        /// <param name="cancelUpdate">A ref value that allows the form to tell FileSync to cancel the synchronization.</param>
        public delegate void SyncStatusDelegate(SyncState state, string text, int progressValue, int maxValue, ref bool cancelUpdate);

        /// <summary>
        /// Methods registered with this delegate are called when new status is available.
        /// </summary>
        public SyncStatusDelegate syncStatus;
        
        /// <summary>
        /// Delegate for the syncError event.
        /// </summary>
        /// <param name="level">A SyncErrorLevel value.</param>
        /// <param name="description">The error description.</param>
        public delegate void SyncErrorDelegate(SyncErrorLevel level, string description);

        /// <summary>
        /// Methods registered with this delegate are called when a synchronization error is detected.
        /// </summary>
        public SyncErrorDelegate syncError;
        
        /// <summary>
        /// Gets or sets the parent handle of the FileSync object.
        /// </summary>
        public IntPtr ParentHandle
        {
            get
            {
                return parentHandle;
            }
            set
            {
                parentHandle = value;
            }
        }
        private IntPtr parentHandle = IntPtr.Zero;

        internal static void RegisterWithSyncMgr()
        {
            Guid syncmgrClsid = new Guid("6295DF27-35EE-11D1-8707-00C04FD93327");
            Type syncmgrType = Type.GetTypeFromCLSID(syncmgrClsid);
            ISyncMgrRegister smr = (ISyncMgrRegister)Activator.CreateInstance(syncmgrType);

            Guid fileSyncHandlerId = SynrcSyncMgrHandler.SyncHandlerId;
            //FileSyncConfig config = FileSyncConfig.GetConfig();
            int hresult = smr.RegisterSyncMgrHandler(ref fileSyncHandlerId, "Outlook Provider", 0);
            if (hresult != 0)
            {
                throw new Exception("Failed to register with HRESULT = " + hresult);
            }
        }

        /// <summary>
        /// Unregisters FileSyncHandler with SyncMgr.
        /// </summary>
        /// <exception cref="System.Exception">Thrown when the handler can't be unregistered.</exception>
		internal static void UnRegisterWithSyncMgr()
        {
            Guid syncmgrClsid = new Guid("6295DF27-35EE-11D1-8707-00C04FD93327");
            Type syncmgrType = Type.GetTypeFromCLSID(syncmgrClsid);
            ISyncMgrRegister smr = (ISyncMgrRegister)Activator.CreateInstance(syncmgrType);

            Guid fileSyncHandlerId = SynrcSyncMgrHandler.SyncHandlerId;
            int hresult = smr.UnregisterSyncMgrHandler(ref fileSyncHandlerId, 0);
            if (hresult != 0)
            {
                throw new Exception("Failed to register with HRESULT = " + hresult);
            }
        }

        /// <summary>
        /// Shows the Synchronization Manager dialog.
        /// </summary>
        public static void ShowSyncMgrGui()
        {
            Guid syncmgrClsid = new Guid("6295DF27-35EE-11D1-8707-00C04FD93327");
            Type syncmgrType = Type.GetTypeFromCLSID(syncmgrClsid);
            ISyncMgrSynchronizeInvoke smsi = (ISyncMgrSynchronizeInvoke)Activator.CreateInstance(syncmgrType);

            smsi.UpdateAll();
        }
        internal bool updateWasCanceled = false;
		internal bool UpdateWasCanceled { get { return updateWasCanceled; } }
		internal bool invokeDirectly = false;

        internal  int Sync()
        {
            int retval = 0;

            if (invokeDirectly)
            {
                retval = SyncDirect();
            }
            else
            {
                SyncThroughSyncMgr();
            }

            return retval;
        }
    
        //
        // SyncDirect
        //
        // Perform the file synchronization directly (without Synchronization Manager).
        //
        // Exceptions:
        //  System.Exception - Thrown when FileSync returns an HRESULT other than S_OK.
        //
        private int SyncDirect()
        {
            int retval = 0;  // S_OK
            
			//SynrcSyncMgrHandler fsh = new SynrcSyncMgrHandler();

			//retval = fsh.Initialize(0, (int) (SyncMgrFlag.Invoke | SyncMgrFlag.MayBotherUser), 0, IntPtr.Zero);
			//if (retval != 0)
			//{
			//    throw new Exception("HRESULT from Initialize was " + retval);
			//}

			//SyncMgrSynchronizeCallback smsc = new SyncMgrSynchronizeCallback();
			//smsc.FileSync = this;
			//retval = fsh.SetProgressCallback(smsc);
			//if (retval != 0)
			//{
			//    throw new Exception("HRESULT from SyncMgrSynchronizeCallback was " + retval);
			//}

			//Guid[] itemIds = new Guid[1];
			//itemIds[0] = SynrcSyncMgrHandler.SyncHandlerId;
			//retval = fsh.PrepareForSync(1, itemIds, parentHandle, 0);
			//if (retval != 0)
			//{
			//    throw new Exception("HRESULT from PrepareForSync was " + retval);
			//}

			//retval = fsh.Synchronize(parentHandle);
			//if (retval != 0)
			//{
			//    // The handler won't call SynchronizeCompleted, 
			//    // so we indicate completion here.
			//    if (syncStatus != null)
			//    {
			//        syncStatus(SyncState.Failed, 
			//            "Synchronize failed with HRESULT of 0x" + retval.ToString("x"),
			//            0, 0,
			//            ref updateWasCanceled);
			//    }
			//}

            return retval;
        }

        //
        // SyncThroughSyncMgr
        //
        // Perform the file synchronization using Synchronization Manager.
        //
        public void SyncThroughSyncMgr()
        {
            Guid syncmgrClsid = new Guid("6295DF27-35EE-11D1-8707-00C04FD93327");
            Type syncmgrType = Type.GetTypeFromCLSID(syncmgrClsid);
            ISyncMgrSynchronizeInvoke smsi = (ISyncMgrSynchronizeInvoke)Activator.CreateInstance(syncmgrType);

            Guid fileSyncHandlerId = SynrcSyncMgrHandler.SyncHandlerId;
            smsi.UpdateItems(
                0x02,  // SYNCMGRINVOKE_STARTSYNC - Immediately start the synchronization without displaying the Choice dialog. 
                ref fileSyncHandlerId,
                0,  // cbCookie - [in] Size in bytes of lpCookie data.
                IntPtr.Zero);  // lpCookie - [in] Pointer to the private token that SyncMgr uses to identify the application. This token is passed in the ISyncMgrSynchronize::Initialize method. 
        }

        //
        // StatusUpdate
        //
        // Communicates synchronization status.
        //
        // Parameters:
        //  state - A SyncState value.
        //  text - The status description.
        //  progressValue - An integer that indicates the progress value.
        //  maxValue - An integer that indicates the maximum progress value.
        //
        internal void StatusUpdate(SyncState state, string text, int progressValue, int maxValue)
        {
			//if (syncStatus != null)
			//{
			//    syncStatus(state, text, progressValue, maxValue, ref updateWasCanceled);
			//}
        }

        //
        // LogError
        //
        // Logs information, a warning, or an error message into the error tab on the
        //  Synchronization Manager status dialog box.
        //
        // Parameters:
        //  level - The error level from the SYNCMGRLOGLEVEL enumeration.
        //  description - The error text to be displayed in the error tab.
        //
        internal void LogError(SyncErrorLevel level, string description)
        {
			//if (syncError != null)
			//{
			//    syncError(level, description);
			//}
        }
    }

    /// <summary>
    /// Defines values for the set of synchronization states.
    /// </summary>
    public enum SyncState
    {
        /// <summary>
        /// No state is available, normally because no synchronization has yet run.
        /// </summary>
        NotAvailable,
        /// <summary>
        /// Synchronization is currently in progress.
        /// </summary>
        Updating,
        /// <summary>
        /// The last synchronization succeeded.
        /// </summary>
        Succeeded,
        /// <summary>
        /// The last synchronization failed.
        /// </summary>
        Failed
    }

    /// <summary>
    /// Enumeration of synchronization error levels.
    /// </summary>
    public enum SyncErrorLevel
    {
        /// <summary>
        /// Information only.
        /// </summary>
        Information,

        /// <summary>
        /// A non-serious error that can usually be ignored at run time.
        /// </summary>
        Warning,

        /// <summary>
        /// A serious error that cannot be ignored.
        /// </summary>
        Error
    }
}