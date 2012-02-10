//
//	Copyright (c) 2009 Synrc Research Center
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using System.Runtime.InteropServices;
using Microsoft.Communications.Contacts;

namespace Synrc
{
	public class NokiaContactsProvider : ContactsProvider, ISyncSource
	{
		IntPtr m_pContactBuffer = IntPtr.Zero;
		IntPtr buf = IntPtr.Zero;
		DAContentAccessDefinitions.CA_ID_LIST caIDList;
		DAContentAccessDefinitions.CA_FOLDER_INFO folderInfo;
		int m_hContacts = 0;
		int m_hCurrentConnection = 0;

		public Bitmap Image
		{
			get
			{
				return null;
			}
		}
		public DAContentAccessDefinitions.CANotifyCallbackDelegate pCANotifyCallBack;
		public DAContentAccessDefinitions.CAOperationCallbackDelegate pCAOperationCallback;
		public CONADefinitions.DeviceNotifyCallbackDelegate pDeviceCallBack;

		public override string Name { get { return "NOKIA"; } }
		public override string DisplayName { get { return "NOKIA"; } }

		public NokiaContactsProvider(IGUICallbacks host, SyncEngine syncEngine)
		{
			owner = host;
			this.syncEngine = syncEngine;
		}

		public int FetchCount
		{
			get
			{
				int iRet = CheckContactsConnection(Id);
				GetFolderInfo();

				// Set contacts folder target path
				// Read all the contact item IDs from the connected device 
				caIDList = new DAContentAccessDefinitions.CA_ID_LIST();
				caIDList.iSize = Marshal.SizeOf(caIDList);
				IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf(caIDList));
				Marshal.StructureToPtr(caIDList, buf, true);

				// Allow Message On Bluetooth Phone

				iRet = DAContentAccess.CAGetIDList(m_hContacts, folderInfo.iFolderId, 0, buf);
				if (iRet != Synrc.PCCSErrors.CONA_OK)
				{
					Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, PCCAPIUtils.CONAError2String(iRet), null);

					//syncEngine.CancelSync(PCCAPIUtils.CONAError2String(iRet));
					//PCCAPIUtils.ShowErrorMessage("CAGetIDList", iRet);
				}
				caIDList = (DAContentAccessDefinitions.CA_ID_LIST)Marshal.PtrToStructure(buf, typeof(DAContentAccessDefinitions.CA_ID_LIST));

				return caIDList.iUIDCount;
			}
		
		}

		public class PIMAddresses
		{
			public string Postal;
			public string Business;
			public string Private;
			////
			public string City;
			public string Country;
			public string POBox;
			public string PostalCode;
			public string State;
			public string Street;
		}

		public class PIMNumbers
		{
			public IList<string> Telephone = new List<string>();
			public IList<string> Home = new List<string>();
			public IList<string> Work = new List<string>();
			public IList<string> Preffered = new List<string>();
			public IList<string> Car = new List<string>();
			public IList<string> Data = new List<string>();
			public IList<string> Mobile = new List<string>();
			public IList<string> MobileHome = new List<string>();
			public IList<string> MobileWork = new List<string>();
			public IList<string> Fax = new List<string>();
			public IList<string> FaxHome = new List<string>();
			public IList<string> FaxWork = new List<string>();
			public IList<string> Pager = new List<string>();
			public IList<string> Video = new List<string>();
			public IList<string> VideoHome = new List<string>();
			public IList<string> VideoWork = new List<string>();
			public IList<string> VoIP = new List<string>();
			public IList<string> VoIPHome = new List<string>();
			public IList<string> VoIPWork = new List<string>();
		}

		public class PIMPersonalInfo
		{
			public string Name = null;
			public string FirstName = null;
			public string MiddleName = null;
			public string LastName = null;
			public string Title = null;
			public string Suffix = null;
			public string Company = null;
			public string JobTitle = null;
			public string Birthday = null;
			public string Nick = null;
			public string FormalName = null;
			public string GivenNamePronunciation = null;
			public string FamilyNamePronunciation = null;
			public string CompanyNamePronunciation = null;
		}

		public void ReadAddresses(PIMAddresses a, CADataDefinitions.CA_DATA_ITEM pimData)
		{
			CADataDefinitions.CA_DATA_POSTAL_ADDRESS postal;
			postal = (CADataDefinitions.CA_DATA_POSTAL_ADDRESS)Marshal.PtrToStructure(pimData.pCustomData, typeof(CADataDefinitions.CA_DATA_POSTAL_ADDRESS));
						
			if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_POSTAL)
            {
                a.Postal = Marshal.PtrToStringUni(pimData.pCustomData);
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_POSTAL_BUSINESS)
            {
                a.Business = Marshal.PtrToStringUni(pimData.pCustomData);
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_POSTAL_PRIVATE)
            {
				a.Private = Marshal.PtrToStringUni(pimData.pCustomData);
			}

			a.City = postal.pstrCity;
			a.Country = postal.pstrCountry;
			a.POBox = postal.pstrPOBox;
			a.PostalCode = postal.pstrPostalCode;
			a.State = postal.pstrState;
			a.Street = postal.pstrStreet;

		}

		public void ReadNumbers(PIMNumbers n, CADataDefinitions.CA_DATA_ITEM pimData)
		{
            if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_TEL)
            {
				n.Telephone.Add(Marshal.PtrToStringUni(pimData.pCustomData));
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_TEL_HOME)
            {
				n.Home.Add(Marshal.PtrToStringUni(pimData.pCustomData));
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_TEL_WORK)
            {
				n.Work.Add(Marshal.PtrToStringUni(pimData.pCustomData));
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_TEL_PREF)
            {
				n.Preffered.Add(Marshal.PtrToStringUni(pimData.pCustomData));
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_TEL_CAR)
            {
				n.Car.Add(Marshal.PtrToStringUni(pimData.pCustomData));
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_TEL_DATA)
            {
				n.Data.Add(Marshal.PtrToStringUni(pimData.pCustomData));
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_MOBILE)
            {
				n.Mobile.Add(Marshal.PtrToStringUni(pimData.pCustomData));
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_MOBILE_HOME)
            {
				n.MobileHome.Add(Marshal.PtrToStringUni(pimData.pCustomData));
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_MOBILE_WORK)
            {
				n.MobileWork.Add(Marshal.PtrToStringUni(pimData.pCustomData));
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_PAGER)
            {
				n.Pager.Add(Marshal.PtrToStringUni(pimData.pCustomData));
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_FAX)
            {
				n.Fax.Add(Marshal.PtrToStringUni(pimData.pCustomData));
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_FAX_HOME)
            {
				n.FaxHome.Add(Marshal.PtrToStringUni(pimData.pCustomData));
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_FAX_WORK)
            {
				n.FaxWork.Add(Marshal.PtrToStringUni(pimData.pCustomData));
			}
			else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_VIDEO)
			{
				n.Video.Add(Marshal.PtrToStringUni(pimData.pCustomData));
			}
			else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_VIDEO_HOME)
			{
				n.VideoHome.Add(Marshal.PtrToStringUni(pimData.pCustomData));
			}
			else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_VIDEO_WORK)
			{
				n.VideoWork.Add(Marshal.PtrToStringUni(pimData.pCustomData));
			}
			else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_VOIP)
			{
				n.VoIP.Add(Marshal.PtrToStringUni(pimData.pCustomData));
			}
			else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_VOIP_HOME)
			{
				n.VoIPHome.Add(Marshal.PtrToStringUni(pimData.pCustomData));
			}
			else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_VOIP_WORK)
			{
				n.VoIPWork.Add(Marshal.PtrToStringUni(pimData.pCustomData));
			}
			else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_TEL_PREF)
			{
				n.Preffered.Add(Marshal.PtrToStringUni(pimData.pCustomData));
			}
		}

		public void ReadPersonalInfo(PIMPersonalInfo c, CADataDefinitions.CA_DATA_ITEM pimData)
		{
            if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_NAME)
            {
				c.Name = Marshal.PtrToStringUni(pimData.pCustomData);
            } 
			else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_FN)
            {
				c.FirstName = Marshal.PtrToStringUni(pimData.pCustomData);
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_MN)
            {
				c.MiddleName = Marshal.PtrToStringUni(pimData.pCustomData);
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_LN)
            {
				c.LastName = Marshal.PtrToStringUni(pimData.pCustomData);
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_TITLE)
            {
				c.Title = Marshal.PtrToStringUni(pimData.pCustomData);
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_SUFFIX)
            {
				c.Suffix = Marshal.PtrToStringUni(pimData.pCustomData);
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_COMPANY)
            {
				c.Company = Marshal.PtrToStringUni(pimData.pCustomData);
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_JOB_TITLE)
            {
				c.JobTitle = Marshal.PtrToStringUni(pimData.pCustomData);
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_BIRTHDAY)
            {
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_PICTURE)
            {
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_NICKNAME)
            {
				c.Nick = Marshal.PtrToStringUni(pimData.pCustomData);
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_FORMAL_NAME)
            {
				c.FormalName = Marshal.PtrToStringUni(pimData.pCustomData);
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_GIVEN_NAME_PRONUNCIATION)
            {
				c.GivenNamePronunciation = Marshal.PtrToStringUni(pimData.pCustomData);
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_FAMILY_NAME_PRONUNCIATION)
            {
                c.FamilyNamePronunciation = Marshal.PtrToStringUni(pimData.pCustomData);
            }
            else if (pimData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_COMPANY_NAME_PRONUNCIATION)
            {
                c.CompanyNamePronunciation = Marshal.PtrToStringUni(pimData.pCustomData);
            }
		}

		public Contact GetCanonicalContact(PIMPersonalInfo pim, PIMNumbers num, IList<PIMAddresses> adr)
		{
			if (pim == null)
				return null;

			Contact contact = new Contact();

			if (pim.FormalName != null)
			{
				string[] names = pim.FormalName.Split(new char[] { ' ' });

				if (pim.FirstName == null && pim.LastName == null && pim.MiddleName == null)
				{
					if (names.Length > 2)
						pim.FirstName = names[0];
					else if (names.Length == 2)
						pim.FirstName = names[1];
					else if (names.Length > 0)
						pim.FirstName = names[0];

					if (names.Length == 3)
						pim.MiddleName = names[1];
					else if (names.Length > 3)
						pim.MiddleName = names[names.Length - 2];

					if (names.Length >= 3)
						pim.LastName = names[2];
					else if (names.Length > 1)
						pim.LastName = names[0];
				}
			}

			Name name = new Name(
				pim.FormalName,
				pim.GivenNamePronunciation, 
				null,
				pim.Title,
				pim.FirstName,
				pim.MiddleName,
				pim.LastName,
				null,
				pim.Suffix,
				pim.Nick);

			name.FormattedName = pim.LastName;
			if (name.FormattedName != null &&
				name.FormattedName != "")
				name.FormattedName += " ";
			name.FormattedName += pim.FirstName;
			if (name.FormattedName != null &&
				name.FormattedName != "")
				name.FormattedName += " ";
			name.FormattedName += pim.MiddleName;
			name.FormattedName = name.FormattedName.Trim();

			contact.Names.Add(name);

			PhoneNumber phone = null;
			
			if (num.Mobile.Count > 0)
			{
				foreach (string ph in num.Mobile) {
					phone = new PhoneNumber(ph);
					contact.PhoneNumbers.Add(phone, PhoneLabels.Cellular);
				}
			}

			if (num.MobileHome.Count > 0)
			{
				foreach (string ph in num.MobileHome) {
					phone = new PhoneNumber(ph);
					contact.PhoneNumbers.Add(phone, PhoneLabels.Cellular, PropertyLabels.Personal);
				}
			}

			if (num.MobileWork.Count > 0)
			{
				foreach (string ph in num.MobileWork) {
					phone = new PhoneNumber(ph);
					contact.PhoneNumbers.Add(phone, PhoneLabels.Cellular, PropertyLabels.Business);
				}
			}

			if (num.Telephone.Count > 0)
			{
				foreach (string ph in num.Telephone)
				{
					phone = new PhoneNumber(ph);
					contact.PhoneNumbers.Add(phone, PhoneLabels.Voice);
				}
			}

			if (num.Home.Count > 0)
			{
				foreach (string ph in num.Home)
				{
					phone = new PhoneNumber(ph);
					contact.PhoneNumbers.Add(phone, PhoneLabels.Voice, PropertyLabels.Personal);
				}
			}

			if (num.Work.Count > 0)
			{
				foreach (string ph in num.Work)
				{
					phone = new PhoneNumber(ph);
					contact.PhoneNumbers.Add(phone, PhoneLabels.Voice, PropertyLabels.Business);
				}
			}

			if (num.Fax.Count > 0)
			{
				foreach (string ph in num.Fax)
				{
					phone = new PhoneNumber(ph);
					contact.PhoneNumbers.Add(phone, PhoneLabels.Fax);
				}
			}

			if (num.FaxHome.Count > 0)
			{
				foreach (string ph in num.FaxHome)
				{
					phone = new PhoneNumber(ph);
					contact.PhoneNumbers.Add(phone, PhoneLabels.Fax, PropertyLabels.Personal);
				}
			}

			if (num.FaxWork.Count > 0)
			{
				foreach (string ph in num.FaxWork)
				{
					phone = new PhoneNumber(ph);
					contact.PhoneNumbers.Add(phone, PhoneLabels.Fax, PropertyLabels.Business);
				}
			}

			if (num.Pager.Count > 0)
			{
				foreach (string ph in num.Pager)
				{
					phone = new PhoneNumber(ph);
					contact.PhoneNumbers.Add(phone, PhoneLabels.Pager);
				}
			}

			if (num.Video.Count > 0)
			{
				foreach (string ph in num.Video)
				{
					phone = new PhoneNumber(ph);
					contact.PhoneNumbers.Add(phone, PhoneLabels.Video);
				}
			}

			if (num.VideoHome.Count > 0)
			{
				foreach (string ph in num.VideoHome)
				{
					phone = new PhoneNumber(ph);
					contact.PhoneNumbers.Add(phone, PhoneLabels.Video, PropertyLabels.Personal);
				}
			}

			if (num.VideoWork.Count > 0)
			{
				foreach (string ph in num.VideoWork)
				{
					phone = new PhoneNumber(ph);
					contact.PhoneNumbers.Add(phone, PhoneLabels.Video, PropertyLabels.Business);
				}
			}

			foreach (PIMAddresses a in adr)
			{
				PhysicalAddress phys = new PhysicalAddress(
					a.POBox, a.Street, a.City, a.State, a.PostalCode, a.Country,
					null, null);

				if (a.Private != null)
				{
					contact.Addresses.Add(phys, PropertyLabels.Personal);
				}
				else if (a.Business != null)
				{
					contact.Addresses.Add(phys, PropertyLabels.Business);
				}
				else
				{
					contact.Addresses.Add(phys, PropertyLabels.Personal);
				}
			}

			Position pos = new Position(null, null, pim.Company, null, null, pim.JobTitle, null);
			contact.Positions.Add(pos, PropertyLabels.Business);

			DateTime lastChanged = DateTime.Now;
			contact.Dates.Add(lastChanged, new string[] { "LastModificationTime" });
			
			return contact;
		}

		public void FetchTask()
		{
			mans.Clear();
			contactsByFullName.Clear();

			//if (owner != null)
			//	Dispatcher.BeginInvoke(new VoidInt(owner.SetBarLimits),
			//		FetchCount);

			syncEngine.CurrentTotal += FetchCount;

			if (syncEngine.SyncCanceled)
			{
				this.FetchSem.Release();
				return;
			}

			int hOperHandle = 0;
			int iRet = DAContentAccess.CABeginOperation(m_hContacts, 0, ref hOperHandle);
			if (iRet != Synrc.PCCSErrors.CONA_OK)
			{
				PCCAPIUtils.ShowErrorMessage("CABeginOperation", iRet);
				this.FetchSem.Release();
				return;
			}
			
			for (int k = 0; k < caIDList.iUIDCount; k++)
			{
				//if (owner != null && !quiet)
				//	Dispatcher.BeginInvoke(new VoidString(owner.IncBarPosition), "Loading" /*+ " " + this.Name*/);

				if (owner != null)
				{
					Dispatcher.Invoke(new SyncProgressEventHandler(owner.Progress), this,
						"Loading " + "(" + syncEngine.CurrentItemNum + "/" + syncEngine.CurrentTotal + ")",
						syncEngine.CurrentItemNum, syncEngine.CurrentTotal);
					syncEngine.CurrentItemNum++;
				}

				DAContentAccessDefinitions.CA_ITEM_ID UID = GetUidFromBuffer(k, caIDList.pUIDs);
				CADataDefinitions.CA_DATA_CONTACT dataContact = new CADataDefinitions.CA_DATA_CONTACT();
				ReadContact(UID, ref dataContact);
				PIMPersonalInfo pimData = new PIMPersonalInfo();
				PIMNumbers numData = new PIMNumbers();
				IList<PIMAddresses> adrData = new List<PIMAddresses>();

				for (int i = 0; i < dataContact.bPICount; i++)
				{
					Int64 iPtr = dataContact.pPIFields.ToInt64() + i * Marshal.SizeOf(typeof(CADataDefinitions.CA_DATA_ITEM));
					IntPtr ptr = new IntPtr(iPtr);
					CADataDefinitions.CA_DATA_ITEM itemData;
					itemData = (CADataDefinitions.CA_DATA_ITEM)Marshal.PtrToStructure(ptr, typeof(CADataDefinitions.CA_DATA_ITEM));
					if (itemData.iFieldType == CADataDefinitions.CA_FIELD_TYPE_CONTACT_PI)
						ReadPersonalInfo(pimData, itemData);
				}

				for (int i = 0; i < dataContact.bNumberCount; i++)
                {
                    Int64 iPtr = dataContact.pNumberFields.ToInt64() + i * Marshal.SizeOf(typeof(CADataDefinitions.CA_DATA_ITEM));
                    IntPtr ptr = new IntPtr(iPtr);
                    CADataDefinitions.CA_DATA_ITEM itemData;
                    itemData = (CADataDefinitions.CA_DATA_ITEM)Marshal.PtrToStructure(ptr, typeof(CADataDefinitions.CA_DATA_ITEM));
					if (itemData.iFieldType == CADataDefinitions.CA_FIELD_TYPE_CONTACT_NUMBER)
						ReadNumbers(numData, itemData);
                }

				for (int i = 0; i < dataContact.bAddressCount; i++)
				{
					Int64 iPtr = dataContact.pAddressFields.ToInt64() + i * Marshal.SizeOf(typeof(CADataDefinitions.CA_DATA_ITEM));
					IntPtr ptr = new IntPtr(iPtr);
					CADataDefinitions.CA_DATA_ITEM itemData;
					itemData = (CADataDefinitions.CA_DATA_ITEM)Marshal.PtrToStructure(ptr, typeof(CADataDefinitions.CA_DATA_ITEM));
					if (itemData.iFieldType == CADataDefinitions.CA_FIELD_TYPE_CONTACT_ADDRESS)
					{
						PIMAddresses a = new PIMAddresses();
                        if (false)
                        {
                            ReadAddresses(a, itemData);
                            adrData.Add(a);
                        }
                        
					}

					if (itemData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_POSTAL |
						itemData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_POSTAL_BUSINESS |
						itemData.iFieldSubType == CADataDefinitions.CA_FIELD_SUB_TYPE_POSTAL_PRIVATE)
					{
					}
				}

				Contact contact = GetCanonicalContact(pimData, numData, adrData);
				contactsByFullName[contact.FullName] = contact;
				mans.Add(new Man { FullName = contact.FullName, EMail = contact.EMail, Phone = contact.Phone });
				
			}
			iRet = DAContentAccess.CAEndOperation(hOperHandle);
			if (iRet != PCCSErrors.CONA_OK)
			{
				//PCCAPIUtils.ShowErrorMessage("CAEndOperation", iRet);
				//syncEngine.CancelSync(PCCAPIUtils.CONAError2String(iRet));

				Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, PCCAPIUtils.CONAError2String(iRet), null);

				this.FetchSem.Release();
				return;
			}
			
			CloseContactsConnection();

			//iRet = DAContentAccess.CAFreeIdListStructure(buf);
			//if (iRet != PCCSErrors.CONA_OK)
			//{
			//    PCCAPIUtils.ShowErrorMessage("CAFreeIdListStructure", iRet);
			//}
			Marshal.FreeHGlobal(buf);

			this.FetchSem.Release();
		}
		public void UpdateTask()
		{
			SyncSem.WaitOne();

			if (syncEngine.SyncCanceled)
			{
				UpdateSem.Release();
				return;
			}

			//syncEngine.CurrentTotal += MapAdded.Count + MapUpdated.Count;

			UpdateSem.Release();
		}

		public void GetFolderInfo()
		{
			folderInfo = new DAContentAccessDefinitions.CA_FOLDER_INFO();
			folderInfo.iSize = Marshal.SizeOf(folderInfo);
			IntPtr bufItem = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(DAContentAccessDefinitions.CA_FOLDER_INFO)));
			try
			{
				Marshal.StructureToPtr(folderInfo, bufItem, false);
			}
			catch { }
			int iRet = DAContentAccess.CAGetFolderInfo(m_hContacts, bufItem);
			if (iRet == Synrc.PCCSErrors.CONA_OK)
			{
				folderInfo = (DAContentAccessDefinitions.CA_FOLDER_INFO)Marshal.PtrToStructure(bufItem, typeof(DAContentAccessDefinitions.CA_FOLDER_INFO));
			}
			else
			{
				//PCCAPIUtils.ShowErrorMessage("CAGetFolderInfo", iRet);
				//syncEngine.CancelSync(PCCAPIUtils.CONAError2String(iRet));

				Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, PCCAPIUtils.CONAError2String(iRet), null);

			}
			int iResult = DAContentAccess.CAFreeFolderInfoStructure(bufItem);
			if (iResult != Synrc.PCCSErrors.CONA_OK)
			{
				//PCCAPIUtils.ShowErrorMessage("CAFreeFolderInfoStructure", iResult);
				//syncEngine.CancelSync(PCCAPIUtils.CONAError2String(iRet));

				Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, PCCAPIUtils.CONAError2String(iRet), null);
			}
			Marshal.FreeHGlobal(bufItem);
		}

		private int CheckContactsConnection(string strSerialNumber)
		{
			int iRet = Synrc.PCCSErrors.CONA_OK;
			
			// No PIM connection, open it
			IntPtr pstrSerialNumber = Marshal.StringToCoTaskMemUni(strSerialNumber);
			
			// CONAAllocString(strSerialNumber)
			int iMedia = CONADefinitions.API_MEDIA_ALL;
			int iTarget = CADataDefinitions.CA_TARGET_CONTACTS;
			iRet = DAContentAccess.DAOpenCA(pstrSerialNumber, ref iMedia, iTarget, ref m_hContacts);
			if (iRet != Synrc.PCCSErrors.CONA_OK & 
				iRet != Synrc.PCCSErrors.ECONA_NOT_SUPPORTED_DEVICE)
			{
				return iRet;
			}

			Marshal.FreeCoTaskMem(pstrSerialNumber);
			if (m_hContacts != 0)
			{
				int iResult = DAContentAccess.CARegisterNotifyCallback(m_hContacts, PCCSTypeDefinitions.API_REGISTER, pCANotifyCallBack);
				if (iResult != Synrc.PCCSErrors.CONA_OK)
				{
					//PCCAPIUtils.ShowErrorMessage("CARegisterNotifyCallback", iResult);
					//syncEngine.CancelSync(PCCAPIUtils.CONAError2String(iRet));

					Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, PCCAPIUtils.CONAError2String(iRet), null);
				}
			}

			m_hCurrentConnection = m_hContacts;

			return iRet;
		}

		private int CloseContactsConnection()
		{
			int iRet = Synrc.PCCSErrors.CONA_OK;
			if (m_hContacts != 0)
			{
				// Unregister CallBack
				int iResult = DAContentAccess.CARegisterNotifyCallback(m_hContacts, PCCSTypeDefinitions.API_UNREGISTER, pCANotifyCallBack);

				// Close PIM connection
				iRet = DAContentAccess.DACloseCA(m_hContacts);
				if (iRet != Synrc.PCCSErrors.CONA_OK)
				{
					//syncEngine.CancelSync(PCCAPIUtils.CONAError2String(iRet));
					//PCCAPIUtils.ShowErrorMessage("DACloseCA", iRet);

					Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, PCCAPIUtils.CONAError2String(iRet), null);
				}
				m_hContacts = 0;
			}
			return iRet;
		}

		private DAContentAccessDefinitions.CA_ITEM_ID GetUidFromBuffer(int iIndex, IntPtr pUIds)
		{
			// Calculate beginning of item 'iIndex'
			Int64 iPtr = pUIds.ToInt64() + (iIndex * Marshal.SizeOf(typeof(DAContentAccessDefinitions.CA_ITEM_ID)));
			// Convert integer to pointer
			IntPtr ptr = new IntPtr(iPtr);
			// Copy data from buffer
			return (DAContentAccessDefinitions.CA_ITEM_ID)Marshal.PtrToStructure(ptr, typeof(DAContentAccessDefinitions.CA_ITEM_ID));
		}
		

		private int ReadContact(DAContentAccessDefinitions.CA_ITEM_ID UID, ref CADataDefinitions.CA_DATA_CONTACT dataContact)
		{
			int hOperHandle = 0;
			int iRet = DAContentAccess.CABeginOperation(m_hCurrentConnection, 0, ref hOperHandle);
			if (iRet != PCCSErrors.CONA_OK)
			{
				//PCCAPIUtils.ShowErrorMessage("CABeginOperation", iRet);
				//syncEngine.CancelSync(PCCAPIUtils.CONAError2String(iRet));

				Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, PCCAPIUtils.CONAError2String(iRet), null);
			}
			dataContact.iSize = Marshal.SizeOf(dataContact);
			dataContact.bPICount = 0;
			dataContact.pPIFields = IntPtr.Zero;
			dataContact.bAddressCount = 0;
			dataContact.pAddressFields = IntPtr.Zero;
			dataContact.bNumberCount = 0;
			dataContact.pNumberFields = IntPtr.Zero;
			dataContact.bGeneralCount = 0;
			dataContact.pGeneralFields = IntPtr.Zero;
			// Allocate memory for buffers
			IntPtr buffer = Marshal.AllocHGlobal(Marshal.SizeOf(UID));
			Marshal.StructureToPtr(UID, buffer, true);
			UID.iSize = Marshal.SizeOf(typeof(DAContentAccessDefinitions.CA_ITEM_ID));
			m_pContactBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(dataContact));
			Marshal.StructureToPtr(dataContact, m_pContactBuffer, true);
			iRet = DAContentAccess.CAReadItem(hOperHandle, buffer, DAContentAccessDefinitions.CA_OPTION_USE_CACHE, CADataDefinitions.CA_DATA_FORMAT_STRUCT, m_pContactBuffer);
			if (iRet == PCCSErrors.CONA_OK)
			{
				// Copy data from buffer
				dataContact = (CADataDefinitions.CA_DATA_CONTACT)Marshal.PtrToStructure(m_pContactBuffer, typeof(CADataDefinitions.CA_DATA_CONTACT));
			}
			else
			{
				//PCCAPIUtils.ShowErrorMessage("CAReadItem", iRet);
				//syncEngine.CancelSync(PCCAPIUtils.CONAError2String(iRet));

				Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, PCCAPIUtils.CONAError2String(iRet), null);

				Marshal.FreeHGlobal(m_pContactBuffer);
				m_pContactBuffer = IntPtr.Zero;
			}
			Marshal.FreeHGlobal(buffer);
			int iResult = DAContentAccess.CAEndOperation(hOperHandle);
			if (iResult != PCCSErrors.CONA_OK)
			{
				//PCCAPIUtils.ShowErrorMessage("CAEndOperation", iResult);
				//syncEngine.CancelSync(PCCAPIUtils.CONAError2String(iRet));

				Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, PCCAPIUtils.CONAError2String(iRet), null);
			}
			return iRet;
		}
	}

}
