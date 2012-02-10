// 
//  Copyright (c) Synrc Research Center.  All rights reserved. 
// 

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
using System.Runtime.InteropServices.ComTypes;

namespace Synrc
{

	[
	ComVisible(true),
	Guid("6B8FF769-13FD-463e-AB36-7A355476706A"), 
    ClassInterface(ClassInterfaceType.AutoDual)
    ]
    public class SynrcSyncMgrHandler : ISyncMgrHandler, ISyncMgrHandlerInfo, ISyncMgrSyncItemContainer
    {
        public static Guid SyncHandlerId
        {
            get
            {
                GuidAttribute guidAttribute = (GuidAttribute)Attribute.GetCustomAttribute(typeof(SynrcSyncMgrHandler), typeof(GuidAttribute));
                return new Guid(guidAttribute.Value);
            }
        }

		public static string SyncHandlerName
		{
			get
			{
				return "Synrc Sync Contacts";
			}
		}

		IList<SYNCDEVICEINFO> Devices = new List<SYNCDEVICEINFO>();

       // private ISyncMgrSynchronizeCallback syncMgrSynchronizeCallback = null;
        private Guid itemId;
        internal bool abortTheUpdate = false;
		Icon icon = null;

        public SynrcSyncMgrHandler()
        {
			Devices.Add(new SYNCDEVICEINFO("Windows Contacts", SynrcSyncMgrHandler.SyncHandlerName, 1));
		}

       	#region ISyncMgrHandler Members

		public int Activate(int fActivate)
		{
			return 0;
		}

		public int Enable(int fActivate)
		{
			return 0;
		}

		public int GetCapabilities(out SYNCMGR_HANDLER_CAPABILITIES pmCapabilities)
		{
			pmCapabilities = SYNCMGR_HANDLER_CAPABILITIES.SYNCMGR_HCM_NONE;
			return 0;

		}

		public int GetHandlerInfo(out ISyncMgrHandlerInfo ppSyncMgrEnumItems)
		{
			ppSyncMgrEnumItems = (ISyncMgrHandlerInfo)this;
			return 0;
		}

		public int GetName([MarshalAs(UnmanagedType.LPWStr)] out string ppszName)
		{
			ppszName = "Synrc Contacts Name";
			return 0;
		}

		public int GetObject(ref Guid rguidObjectID, ref Guid riid, out IntPtr ppv)
		{
			ppv = IntPtr.Zero;
			return Convert.ToInt32(0x80004001);  // E_NOTIMPL
		}

		public int GetPolicies(out SYNCMGR_HANDLER_POLICIES pmPolicies)
		{
			pmPolicies = SYNCMGR_HANDLER_POLICIES.SYNCMGR_HPM_NONE ;
			return 0;
		}

		public int Synchronize([In, MarshalAs(UnmanagedType.LPWStr)] ref string ppszItemIDs, [In] uint cItems, [In] ref IntPtr hwndOwner, [In, MarshalAs(UnmanagedType.Interface)] ISyncMgrSessionCreator pSessionCreator, [In, MarshalAs(UnmanagedType.IUnknown)] object punk)
		{
			return 0;
		}

		#endregion

		#region ISyncMgrHandlerInfo Members

		public int GetComment(out string comment)
		{
			comment = "Comment";
			return 0;
		}

		public int GetLastSyncTime(out System.Runtime.InteropServices.ComTypes.FILETIME pftLastSync)
		{
			pftLastSync = new System.Runtime.InteropServices.ComTypes.FILETIME();
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
			return 0;
		}

		public int IsConnected()
		{
			return 0;
		}

		public int IsEnabled()
		{
			return 0;
		}

		#endregion

		#region ISyncMgrSyncItemContainer Members

		public int GetSyncItem(string pszItemID, out ISyncMgrSyncItem ppItem)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void GetSyncItemCount(out uint pcItems)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void GetSyncItemEnumerator(out IEnumSyncMgrSyncItems ppenum)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}