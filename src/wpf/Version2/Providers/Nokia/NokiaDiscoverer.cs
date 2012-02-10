//
//	Copyright (c) 2009 Synrc Research Center
//

using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace Synrc
{

	public class NokiaPhone
	{
		public string Name;
		public string SN;
	}

	public class NokiaDiscoverer
	{
		IGUICallbacks window = null;
		public NokiaContactsProvider nokiaProvider = null;

		private CONADefinitions.DeviceNotifyCallbackDelegate pDeviceCallBack;

		private int m_hDMHandle = 0;
		public int error = 0;

		public bool Connected = false;

		public IList<NokiaPhone> nokiaDevices = new List<NokiaPhone>();

		public NokiaDiscoverer(IGUICallbacks owner, NokiaContactsProvider provider)
		{
			window = owner;
			nokiaProvider = provider;
			nokiaProvider.pCANotifyCallBack = CANotifyCallBack;
			nokiaProvider.pCAOperationCallback = CAOperationCallback;
			nokiaProvider.pDeviceCallBack = DeviceNotifyCallback;
			
			int iResult = CONADeviceManagement.DMAPI_Initialize(CONADeviceManagement.DMAPI_VERSION_32, 0);
			iResult = DAContentAccess.CAAPI_Initialize(DAContentAccess.CAAPI_VERSION_30, 0);
			iResult = CONADeviceManagement.CONAOpenDM(ref m_hDMHandle);
			if (iResult != PCCSErrors.CONA_OK) error = 1;
			pDeviceCallBack = DeviceNotifyCallback;
			iResult = CONADeviceManagement.CONARegisterNotifyCallback(m_hDMHandle, CONADefinitions.API_REGISTER, pDeviceCallBack);
			if (iResult != PCCSErrors.CONA_OK) error = 2;

			//window.syncButton.DropDown.Opened +=new RoutedEventHandler(DropDown_Opened);

		}

		//void DropDown_Opened(object sender, RoutedEventArgs e)
		//{
		//	DiscoveryTask();
		//}

		public void DiscoveryTask()
		{
			int iRet = CONADeviceManagement.CONARefreshDeviceList(m_hDMHandle, 0);
			if (iRet != Synrc.PCCSErrors.CONA_OK) error = 3;
			CONADefinitions.CONAPI_DEVICE[] pDevices;
            int iDeviceCount = 0;
            iRet = CONADeviceManagement.CONAGetDeviceCount(m_hDMHandle, ref iDeviceCount);
			if (iRet != PCCSErrors.CONA_OK) error = 4;

			nokiaDevices.Clear();

			if (iRet == PCCSErrors.CONA_OK & iDeviceCount > 0)
			{
				pDevices = null;
				pDevices = new CONADefinitions.CONAPI_DEVICE[iDeviceCount];

				IntPtr buffer = Marshal.AllocHGlobal(iDeviceCount * Marshal.SizeOf(typeof(CONADefinitions.CONAPI_DEVICE)));
				iRet = CONADeviceManagement.CONAGetDevices(m_hDMHandle, ref iDeviceCount, buffer);

				if (iRet != Synrc.PCCSErrors.CONA_OK)
				{
					error = 5;
				}
				else
				{
					int iDeviceIndex;
					for (iDeviceIndex = 0; iDeviceIndex < iDeviceCount; iDeviceIndex++)
					{
						Int64 iPtr = buffer.ToInt64() + iDeviceIndex * Marshal.SizeOf(typeof(CONADefinitions.CONAPI_DEVICE));
						IntPtr ptr = new IntPtr(iPtr);
						pDevices[iDeviceIndex] = (CONADefinitions.CONAPI_DEVICE)Marshal.PtrToStructure(ptr, typeof(CONADefinitions.CONAPI_DEVICE));
						string deviceName = pDevices[iDeviceIndex].pstrFriendlyName;
						string serialNumber = pDevices[iDeviceIndex].pstrSerialNumber;
						nokiaDevices.Add(new NokiaPhone { Name = deviceName, SN = serialNumber});
					}

					CONADeviceManagement.CONAFreeDeviceStructure(iDeviceCount, buffer);
					if (iRet != PCCSErrors.CONA_OK) error = 6;
				}
			}

			window.UpdateApplicationMenu();
		}

		//public void UpdateApplicationMenu()
		//{
		//    IDictionary<string, MenuItem> toDelete = new Dictionary<string,MenuItem>();
		//    foreach (MenuItem mi in window.syncButton.DropDown.Items)
		//    {
		//        if (mi.Tag as NokiaPhone!= null)
		//        {
		//            toDelete[mi.Header.ToString()] = mi;
		//        }
		//    }

		//    foreach (MenuItem mi in toDelete.Values)
		//    {
		//        window.syncButton.DropDown.Items.Remove(mi);
		//    }

		//    foreach (NokiaPhone phone in nokiaDevices)
		//    {
		//        MenuItem mi = new MenuItem();
		//        mi.Header = phone.Name;
		//        mi.Tag = phone;
		//        mi.Click += new System.Windows.RoutedEventHandler(mi_Click);
		//        window.syncButton.DropDown.Items.Add(mi);
		//    }
		//}

		//void mi_Click(object sender, System.Windows.RoutedEventArgs e)
		//{
		//    NokiaPhone phone = (sender as MenuItem).Tag as NokiaPhone;
		//    if (phone == null) return;
		//    nokiaProvider.Id = phone.SN;
		//    nokiaProvider.Name = phone.Name;
		//    window.SyncNokia(this, new RoutedEventArgs());

		//}

		// Callback function for CA notifications
		public int CANotifyCallBack(int hCAHandle, int iReason, int iParam, IntPtr pItemID)
		{
			return PCCSErrors.CONA_OK;
		}
		// Callback function for CA operation notifications
		public int CAOperationCallback(int hOperHandle, int iOperation, int iCurrent, int iTotalAmount, int iStatus, IntPtr pItemID)
		{
			return PCCSErrors.CONA_OK;
		}
		// Callback function for device connection notifications

		public int DeviceNotifyCallback(
			int iStatus,
			[MarshalAs(UnmanagedType.LPWStr)] string pstrSerialNumber)
		{
			// Refresh tree view after next timer tick
			Connected = true;
			return Synrc.PCCSErrors.CONA_OK;
		}
	}
}
