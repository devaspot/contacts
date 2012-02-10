//
//	Copyright (c) 2009 Synrc Research Center
//

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Communications.Contacts;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using Redemption;
using System.Diagnostics;
using Microsoft.Win32;
using Microsoft.Office.Interop.Outlook;
using Synrc.Properties;
using System.Drawing;

//using Outlook; // 2003

namespace Synrc
{
	public class OutlookContactsProvider : ContactsProvider, ISyncSource
	{
		Microsoft.Office.Interop.Outlook._Application outlookApp = null;
		object nsObject = null;

		RDOSessionClass rdoSession = null;
		RDOAddressBook rdoAddressBook = null;
		RDOAddressList rdoAddressList = null;
		RDOFolder rdoFld = null;
		RDOItems rdoItems = null;

		public bool IsRedemptionInstalled()
		{
			bool yes = false;

			RegistryKey masterKey = Registry.ClassesRoot.OpenSubKey("Redemption.RDOSession");
			if (masterKey == null)
			{
				yes = false;

			}
			else
			{
				yes = true;
				masterKey.Close();
			}

			return yes;
		}

		IDictionary<string, RDOContactItem> rdoByFullname = new Dictionary<string,RDOContactItem>();
		IDictionary<string, int> indexByFullname = new Dictionary<string, int>();

		public Bitmap Image
		{
			get
			{
				return Resources.Outlook;
			}
		}

		public override string Name { get { return "Outlook"; } set { } }
		public override string DisplayName { get { return "Microsoft Outlook"; } }

		private static string GetStorePath(string storeID)
		{
			int iStart = storeID.IndexOf("0000", 9) + 4;
			int iEnd = storeID.IndexOf("00", iStart);
			string strPathRaw = string.Empty;
			string strProvider = storeID.Substring(iStart, iEnd - iStart);
			strProvider = HexToString(strProvider, 2);

			switch (strProvider.ToLower())
			{
				case "mspst.dll":
				case "pstprx.dll":
					iStart = storeID.LastIndexOf("00000000") + 8;
					strPathRaw = storeID.Substring(iStart);
					strPathRaw = HexToString(strPathRaw, 2);
					break;
				case "msncon.dll":
					iStart = storeID.LastIndexOf("00") + 2;
					strPathRaw = storeID.Substring(iStart);
					strPathRaw = HexToString(strPathRaw, 2);
					break;
				case "emsmdb.dll":
					strPathRaw = "Exchange";
					break;
				default:
					strPathRaw = "Unknown";
					break;
			}
			return strPathRaw;
		}

		private static string HexToString(string value, int step)
		{
			string retVal = string.Empty;
			for (int i = 0; i < value.Length; i = (i + step))
			{
				string tempRepStr = value.Substring(i, step);
				int tempRepDec = int.Parse(tempRepStr, System.Globalization.NumberStyles.HexNumber, null);
				if (tempRepDec > 0)
				{
					char tempRecChr = Convert.ToChar(tempRepDec);
					retVal += tempRecChr.ToString();
				}
			}
			return retVal;
		} 

		public OutlookContactsProvider(IGUICallbacks host, SyncEngine syncEngine)
		{
			owner = host;

			outlookApp = new Microsoft.Office.Interop.Outlook.ApplicationClass();
			Microsoft.Office.Interop.Outlook.NameSpace ns = outlookApp .GetNamespace("MAPI");
			nsObject = ns.MAPIOBJECT;

			if (!IsRedemptionInstalled())
			{
				Process reg = new Process();
				reg.StartInfo.FileName = "regsvr32.exe";
				reg.StartInfo.Arguments = "/s Redemption.dll";
				reg.StartInfo.UseShellExecute = false;
				reg.StartInfo.CreateNoWindow = true;
				reg.StartInfo.RedirectStandardOutput = true;
				reg.Start();
				reg.WaitForExit();
				reg.Close();
			}
			else
			{
			}

			rdoSession = new RDOSessionClass();
			try
			{
				rdoSession.MAPIOBJECT = nsObject;
			}
			catch
			{
				Microsoft.Office.Interop.Outlook.MAPIFolder mf = outlookApp.Session.GetDefaultFolder(OlDefaultFolders.olFolderContacts);
				string pstPath = GetStorePath(mf.StoreID);
				//string pstPath = @"C:\Users\maxim\AppData\Local\Microsoft\Outlook\Outlook.pst";
				rdoSession.LogonPstStore(pstPath, 1, "", "", 0);
			}
			
			rdoAddressBook = rdoSession.AddressBook;
			rdoAddressList = rdoAddressBook.GAL;
			rdoFld = rdoSession.GetDefaultFolder(rdoDefaultFolders.olFolderContacts);
			rdoItems = rdoFld.Items;

			this.syncEngine = syncEngine;
		}
		public void FillProviderCreatedItem(Contact contact, RDOContactItem ctc)
		{
			// Outlook has just One Name

			if (contact.Names.Count > 0)
			{
				Name name = contact.Names[0];

				//name = GetCannonicalName(name.FormattedName,
				//    name.GivenName, name.MiddleName, name.FamilyName,
				//    NameNotation.Formal, NameNotation.Formal);

				name.PersonalTitle = contact.Names[0].PersonalTitle;
				name.Prefix = contact.Names[0].Prefix;
				name.Suffix = contact.Names[0].Suffix;

				ctc.FullName = name.FormattedName;
				ctc.FirstName = name.GivenName;
				ctc.MiddleName = name.MiddleName;
				ctc.LastName = name.FamilyName;
				ctc.NickName = name.Nickname;
				ctc.Suffix = name.Generation + (name.Generation == null ? "" : " " ) + name.Suffix;
				ctc.Title = name.Prefix + (name.Prefix == null || name.Prefix == "" ? "" : " ") + name.PersonalTitle;

			}

			/// Outlook Possible Phone Numbers

			// ctc.BusinessFaxNumber			business fax
			// ctc.BusinessTelephoneNumber		buisness voice
			// ctc.Business2TelephoneNumber		business voice

			// ctc.HomeFaxNumber				personal fax
			// ctc.HomeTelephoneNumber			personal voice
			// ctc.Home2TelephoneNumber			personal voice

			// ctc.MobileTelephoneNumber		cell
			// ctc.OtherTelephoneNumber			
			// ctc.PrimaryTelephoneNumber		primary
			
			// ctc.CarTelephoneNumber			
			// ctc.ISDNNumber					isdn
			// ctc.PagerNumber					pager
			// ctc.CallbackTelephoneNumber		
			// ctc.TelexNumber
			
			//for (int i = 0; i < contact.PhoneNumbers.Count; i++)
			//{
			//    ILabelCollection labels = contact.PhoneNumbers.GetLabelsAt(i);
			//    PhoneNumber ph = contact.PhoneNumbers[i];
			//    if (labels.Contains("Cellular"))
			//    {
			//        ctc.MobileTelephoneNumber = ph;
					
			//    } else if ()
			//}

			if (contact.PhoneNumbers["Cellular"] != null)
				ctc.MobileTelephoneNumber = contact.PhoneNumbers["Cellular"].Number;

			if (contact.PhoneNumbers["Voice", "Personal"] != null)
				ctc.HomeTelephoneNumber = contact.PhoneNumbers["Voice", "Personal"].Number;

			if (contact.PhoneNumbers["Voice", "Business"] != null)
				ctc.BusinessTelephoneNumber = contact.PhoneNumbers["Voice", "Business"].Number;

			if (contact.Positions["Business"] != null)
			{
				ctc.JobTitle = contact.Positions["Business"].JobTitle;
				ctc.OfficeLocation = contact.Positions["Business"].Office;
				ctc.CompanyName = contact.Positions["Business"].Company;
				ctc.Department = contact.Positions["Business"].Department;
			}

			if (contact.EmailAddresses.Count > 0)
				ctc.Email1Address = contact.EmailAddresses[0].Address;
			if (contact.EmailAddresses.Count > 1)
				ctc.Email2Address = contact.EmailAddresses[1].Address;
			if (contact.EmailAddresses.Count > 2)
				ctc.Email3Address = contact.EmailAddresses[2].Address;


		}


        public Contact GetCanonicalContact(RDOContactItem ctc)
        {
			if (ctc == null) 
				return null;

            Contact contact = new Contact();

			contact.Names.Add(new Name(ctc.FirstName, ctc.MiddleName, ctc.LastName, NameCatenationOrder.FamilyGivenMiddle));
			PhoneNumber phone = null;
			EmailAddress email = null;
			if (ctc.MobileTelephoneNumber != null && ctc.MobileTelephoneNumber.Trim() != "")
			{
				phone = new PhoneNumber(ctc.MobileTelephoneNumber);
				contact.PhoneNumbers.Add(phone, PhoneLabels.Cellular);
			}

			if (ctc.HomeTelephoneNumber != null && ctc.HomeTelephoneNumber.Trim() != "")
			{
				phone = new PhoneNumber(ctc.HomeTelephoneNumber);
				contact.PhoneNumbers.Add(phone, PhoneLabels.Voice, PropertyLabels.Personal);
			}

			if (ctc.BusinessTelephoneNumber != null && ctc.BusinessTelephoneNumber.Trim() != "")
			{
				phone = new PhoneNumber(ctc.BusinessTelephoneNumber);
				contact.PhoneNumbers.Add(phone, PhoneLabels.Voice, PropertyLabels.Business);
			}

			if (ctc.Email1Address != null && ctc.Email1Address.Trim() != "")
			{
				email = new EmailAddress(ctc.Email1Address);
				contact.EmailAddresses.Add(email, PropertyLabels.Preferred);
			}

			if (ctc.Email2Address != null && ctc.Email2Address.Trim() != "")
			{
				email = new EmailAddress(ctc.Email2Address);
				contact.EmailAddresses.Add(email, PropertyLabels.Personal);
			}

			if (ctc.Email3Address != null && ctc.Email3Address.Trim() != "")
			{
				email = new EmailAddress(ctc.Email3Address);
				contact.EmailAddresses.Add(email, PropertyLabels.Business);
			}

			DateTime lastChanged = ctc.LastModificationTime.ToUniversalTime();
			contact.Dates.Add(lastChanged, new string[] { "LastModificationTime" });
			Guid hi = new Guid(ctc.EntryID.Substring(0, 16).PadLeft(32, '0'));
			Guid lo = new Guid(ctc.EntryID.Substring(16, 32));
			contact.ContactIds.Add(hi, new string[] { "OutlookIdHi" });
			contact.ContactIds.Add(lo, new string[] { "OutlookIdLo" });

            return contact;
        }

        public void FetchTask()
        {

			//rdoSession = new RDOSessionClass();
			//rdoSession.MAPIOBJECT = nsObject;
			//rdoAddressBook = rdoSession.AddressBook;
			//rdoAddressList = rdoAddressBook.GAL;
			//rdoFld = rdoSession.GetDefaultFolder(rdoDefaultFolders.olFolderContacts);
			//rdoItems = rdoFld.Items;

			//if (owner != null)
			//	Dispatcher.BeginInvoke(new VoidInt(owner.SetBarLimits),
			//		rdoItems.Count);

			syncEngine.CurrentTotal += rdoItems.Count;

			if (syncEngine.SyncCanceled)
			{
				this.FetchSem.Release();
				return;
			}

			mans.Clear();
			contactsByFullName.Clear();
			rdoByFullname.Clear();
			int index = 1;
			foreach(RDOContactItem ctc in rdoFld.Items)
			{
				if (owner != null)
				{
					Dispatcher.Invoke(new SyncProgressEventHandler(owner.Progress), this,
						"Loading " + "(" + syncEngine.CurrentItemNum + "/" + syncEngine.CurrentTotal + ")",
						syncEngine.CurrentItemNum, syncEngine.CurrentTotal);
					syncEngine.CurrentItemNum++;
				}

				Contact contact = null;
				try
				{
					contact = GetCanonicalContact(ctc);
				}
				catch 
				{
					//syncEngine.CancelSync("Operation Aborted by Outlook.");

					Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, "Operation aborted by Outlook.", null);

					this.FetchSem.Release();
					break;
				}

				if (contact != null && contact.FullName != null && contact.FullName.Trim() != "")
				{
					if (!contactsByFullName.ContainsKey(contact.FullName))
					{
						contactsByFullName[contact.FullName] = contact;
						rdoByFullname[contact.FullName] = ctc;
						indexByFullname[contact.FullName] = index++;
						mans.Add(new Man { FullName = contact.FullName, EMail = contact.EMail, Phone = contact.Phone });
					}
				}
			}

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

			syncEngine.CurrentTotal += MapAdded.Count + MapUpdated.Count;

			rdoSession = new RDOSessionClass();
			rdoSession.MAPIOBJECT = nsObject;
			rdoAddressBook = rdoSession.AddressBook;
			rdoAddressList = rdoAddressBook.GAL;
			rdoFld = rdoSession.GetDefaultFolder(rdoDefaultFolders.olFolderContacts);
			rdoItems = rdoFld.Items;

			foreach (Contact c in MapUpdated.Values)
			{
				if (owner != null)
				{
					Dispatcher.Invoke(new SyncProgressEventHandler(owner.Progress), this,
						"Updating " + "(" + syncEngine.CurrentItemNum + "/" + syncEngine.CurrentTotal + ")",
						syncEngine.CurrentItemNum, syncEngine.CurrentTotal);
					syncEngine.CurrentItemNum++;
				}

				RDOContactItem ctc = rdoItems[indexByFullname[c.FullName]] as RDOContactItem;
				FillProviderCreatedItem(c, ctc);
				ctc.Save();
			}

			foreach (Contact contact in MapAdded.Values)
			{
				if (owner != null)
				{
					Dispatcher.Invoke(new SyncProgressEventHandler(owner.Progress), this,
						"Updating " + "(" + syncEngine.CurrentItemNum + "/" + syncEngine.CurrentTotal + ")",
						syncEngine.CurrentItemNum, syncEngine.CurrentTotal);
					syncEngine.CurrentItemNum++;
				}

				if (contact.FullName != "")
				{
					RDOContactItem ctc = rdoItems.Add("IPM.Contact") as RDOContactItem;

					if (ctc != null)
						FillProviderCreatedItem(contact, ctc);

					ctc.Save();
				}
				else
				{
				}

				
			}

			UpdateSem.Release();
		}
	}
}
