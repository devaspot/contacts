//
//	Copyright (c) 2009 Synrc Research Center
//

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Communications.Contacts;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using Standard;
using Microsoft.LiveFX;
using Microsoft.LiveFX.Client;
using Microsoft.LiveFX.ResourceModel;
using System.Net;
using Microsoft.Web;
using Microsoft.Web.Clients;
using System.Collections.ObjectModel;
using Synrc.Properties;
using System.Drawing;
//using Microsoft.WindowsLive.Id.Client;

namespace Synrc
{
	public class LiveContactsProvider : ContactsProvider, ISyncSource
    {
		LiveOperatingEnvironment env;
		IDictionary<string, Microsoft.LiveFX.Client.Contact> liveContactByFullname = new Dictionary<string, Microsoft.LiveFX.Client.Contact>();

		public Bitmap Image
		{
			get
			{
				return Resources.WindowsLive;
			}
		}

		public override string Name	{ get { return "WindowsLive"; } }
		public override string DisplayName { get { return "Windows Live"; } }
		public override bool NeedAuthorization { get { return true; } }

		public LiveContactsProvider(IGUICallbacks host, SyncEngine syncEngine)
		{
			owner = host;
			this.syncEngine = syncEngine;
		}

		public Microsoft.Communications.Contacts.Contact GetCanonicalContact(Microsoft.LiveFX.Client.Contact c)
		{
			Microsoft.Communications.Contacts.Contact contact = new Microsoft.Communications.Contacts.Contact();
			//LiveItemCollection<Profile, ProfileResource> profiles = c.Profiles;

			contact.Names.Add(new Name(c.Resource.GivenName, c.Resource.MiddleName, c.Resource.FamilyName, NameCatenationOrder.FamilyGivenMiddle));

			foreach (ContactPhone cp in c.Resource.PhoneNumbers)
			{
				PhoneNumber pn = new PhoneNumber(cp.Value);
				if (cp.Type == ContactPhone.ContactPhoneType.Business ||
					cp.Type == ContactPhone.ContactPhoneType.Business2)
					contact.PhoneNumbers.Add(pn, PhoneLabels.Voice, PropertyLabels.Business);
				else if (cp.Type == ContactPhone.ContactPhoneType.BusinessMobile)
					contact.PhoneNumbers.Add(pn, PhoneLabels.Cellular, PropertyLabels.Business);
				else if (cp.Type == ContactPhone.ContactPhoneType.Mobile)
					contact.PhoneNumbers.Add(pn, PhoneLabels.Cellular);
				else if (cp.Type == ContactPhone.ContactPhoneType.Personal)
					contact.PhoneNumbers.Add(pn, PhoneLabels.Voice, PropertyLabels.Personal);
				else if (cp.Type == ContactPhone.ContactPhoneType.Personal2)
					contact.PhoneNumbers.Add(pn, PhoneLabels.Voice, PropertyLabels.Personal);
				else if (cp.Type == ContactPhone.ContactPhoneType.Other)
					contact.PhoneNumbers.Add(pn, PhoneLabels.Voice);
				else if (cp.Type == ContactPhone.ContactPhoneType.OtherFax)
					contact.PhoneNumbers.Add(pn, PhoneLabels.Fax);
				else if (cp.Type == ContactPhone.ContactPhoneType.Fax)
					contact.PhoneNumbers.Add(pn, PhoneLabels.Fax);
				else 
					contact.PhoneNumbers.Add(pn);
			}

			if (c.Resource.WindowsLiveId != null && c.Resource.WindowsLiveId.Trim() != null)
				contact.EmailAddresses.Add(new EmailAddress(c.Resource.WindowsLiveId, "Windows Live ID"), PropertyLabels.Preferred);

			foreach (ContactEmail ce in c.Resource.Emails)
			{
				EmailAddress ea = new EmailAddress(ce.Value);
				if (ce.Type == ContactEmail.ContactEmailType.Personal)
					contact.EmailAddresses.Add(ea, PropertyLabels.Personal);
				else if (ce.Type == ContactEmail.ContactEmailType.Business)
					contact.EmailAddresses.Add(ea, PropertyLabels.Business);
				else if (ce.Type == ContactEmail.ContactEmailType.Other)
					contact.EmailAddresses.Add(ea);
				else
					contact.EmailAddresses.Add(ea);
			}

			DateTime lastChanged = c.Resource.LastUpdatedTime.DateTime.ToUniversalTime();
			contact.Dates.Add(lastChanged, "LastModificationTime");

			string absoluteUri = c.Resource.Id;
			Guid guid = new Guid(absoluteUri.PadLeft(32, '0'));
			contact.ContactIds.Add(guid, "Live");

			return contact;
		}
		public void FillProviderCreatedItem(Microsoft.Communications.Contacts.Contact contact, Microsoft.LiveFX.Client.Contact c)
		{
			if (c == null) return;

			c.Resource.FamilyName = contact.Names[0].FamilyName;
			c.Resource.MiddleName = contact.Names[0].MiddleName;
			c.Resource.GivenName = contact.Names[0].GivenName;

			//foreach (PhoneNumber pn in contact.PhoneNumbers)
			{
				
				if (contact.PhoneNumbers[PhoneLabels.Cellular] != null)
				{
					ContactPhone cp = new ContactPhone();
					cp.Type = ContactPhone.ContactPhoneType.Mobile;
					cp.Value = contact.PhoneNumbers[PhoneLabels.Cellular].Number;
					c.Resource.PhoneNumbers.Add(cp);
				}

				if (contact.PhoneNumbers[PropertyLabels.Personal] != null)
				{
					ContactPhone cp = new ContactPhone();
					cp.Type = ContactPhone.ContactPhoneType.Personal;
					cp.Value = contact.PhoneNumbers[PropertyLabels.Personal].Number;
					c.Resource.PhoneNumbers.Add(cp);
				}
				
				if (contact.PhoneNumbers[PropertyLabels.Business] != null)
				{
					ContactPhone cp = new ContactPhone();
					cp.Type = ContactPhone.ContactPhoneType.Business;
					cp.Value = contact.PhoneNumbers[PropertyLabels.Business].Number;
					c.Resource.PhoneNumbers.Add(cp);
				}
				
				if (contact.PhoneNumbers[PhoneLabels.Fax] != null)
				{
					ContactPhone cp = new ContactPhone();
					cp.Type = ContactPhone.ContactPhoneType.Fax;
					cp.Value = contact.PhoneNumbers[PhoneLabels.Fax].Number;
					c.Resource.PhoneNumbers.Add(cp);
				}

			}

			if (contact.Positions["Business"] != null)
			{
				c.Resource.JobTitle = contact.Positions["Business"].JobTitle;
			}

			
			if (contact.EmailAddresses.Count > 0)
			{
				ContactEmail ce = new ContactEmail();	
				ce.Type = ContactEmail.ContactEmailType.Personal;
				ce.Value = contact.EmailAddresses[0].Address;
				c.Resource.Emails.Add(ce);
			}
			if (contact.EmailAddresses.Count > 1)
			{
				ContactEmail ce = new ContactEmail();
				ce.Type = ContactEmail.ContactEmailType.Personal;
				ce.Value = contact.EmailAddresses[1].Address;
				c.Resource.Emails.Add(ce);
			}
			if (contact.EmailAddresses.Count > 2)
			{
				ContactEmail ce = new ContactEmail();
				ce.Type = ContactEmail.ContactEmailType.Personal;
				ce.Value = contact.EmailAddresses[2].Address;
				c.Resource.Emails.Add(ce);
			}
		}

		public class LinqRes
		{
			public int Count
			{
				get;
				set;
			}
		}

		LiveItemCollection<Microsoft.LiveFX.Client.Contact, ContactResource> liveContacts = null;
		public void FetchTask()
		{
			

			if (syncEngine.SyncCanceled)
			{
				this.FetchSem.Release();
				return;
			}

			mans.Clear();
			contactsByFullName.Clear();

			string appId = "000000004000FCD9";

			if (Credentials == null)
			{
				this.FetchSem.Release();
				return;
			}
			else
			{
				if (Credentials.UserName != null && !Credentials.UserName.Contains("@") && Credentials.UserName.Trim() != "")
				{
					Credentials.UserName += "@live.com";
				}
			}

			// Synrc Contacts
			// https://lx.azure.microsoft.com/Cloud/Provisioning/Manage.aspx?AppID=000000004000FCD9

			//IdentityManager manager = null;

			//try
			//{
			//    manager = IdentityManager.CreateInstance(appId, "Synrc Contacts");
			//}
			//catch (Exception e)
			//{
			//    Credentials = null;
			//    syncEngine.CancelSync(e.Message);
			//    this.FetchSem.Release();
			//    return;
			//}
			//Identity identity = null;

			//if (String.IsNullOrEmpty(defaultUserName) && manager != null)
			//{
			//    identity = manager.CreateIdentity();
			//}
			//else if (manager != null)
			//{
			//    identity = manager.CreateIdentity(defaultUserName);
			//}

			//if (identity != null && identity.SavedCredentials == CredentialType.UserNameAndPassword)
			//{
			//    identity.Authenticate(AuthenticationType.Silent);
			//}
			//else if (identity != null)
			//{
			//    identity.Authenticate();
			//    defaultUserName = identity.UserName;
			//}
			
			env = new LiveOperatingEnvironment();

			string serverUrl = "https://user-ctp.windows.net";
			Uri uri = new Uri(serverUrl);
			LiveItemAccessOptions liao = new LiveItemAccessOptions(true);

			string token = null;
			env.ConnectCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(env_ConnectCompleted);

			if (Credentials.UserName == null || Credentials.UserName.Trim() == "")
			{
				Credentials = null;
				Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, "Cannot logon with blank username.", null);
				//syncEngine.CancelSync("Cannot logon with blank username.");
				this.FetchSem.Release();
				return;
			}

			try
			{
				token = Credentials.GetWindowsLiveAuthenticationToken(appId, true);
				//token = identity.GetTicket("user-ctp.windows.net", "MBI_SSL", true);
				env.Connect(token, AuthenticationTokenType.UserToken, uri, liao);
			}
			catch (WebException e)
			{
				Credentials = null;
				if (e.Message.Contains("404"))
					Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, "Maybe account not redeemed.", "https://lx.azure.microsoft.com/Cloud/Launch/Redeem.aspx?Token=FFFFF-FFFFF-FFFFF-FFFFF-FFFFF");
					//syncEngine.CancelSync("Maybe account not redeemed.", "", "https://lx.azure.microsoft.com/Cloud/Launch/Redeem.aspx?Token=FFFFF-FFFFF-FFFFF-FFFFF-FFFFF");
				else
					Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, e.Message, null);
					//syncEngine.CancelSync(e.Message);
				this.FetchSem.Release();
				return;
			}
			catch (Exception e)
			{
				Credentials = null;
				Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, e.Message, null);
				this.FetchSem.Release();
				return;
			}

			env.Contacts.LoadAsync(new object());
			liveContacts = env.Contacts;

			//var MeshProjects = (from mo in env.Contacts.Entries
			//                    group mo by mo.LiveOperatingEnvironment.BaseUri into result
			//                    select new LinqRes { Count = result.Count() });
			//syncEngine.CurrentTotal += MeshProjects.First<LinqRes>().Count;// .3;

			foreach (Microsoft.LiveFX.Client.Contact live in liveContacts.Entries)
			{
				//live.LoadAsync(new object());
				syncEngine.CurrentTotal++;
			}

			foreach (Microsoft.LiveFX.Client.Contact live in liveContacts.Entries)
			{
				if (owner != null)
				{
					syncEngine.CurrentItemNum++;
					Dispatcher.Invoke(new SyncProgressEventHandler(owner.Progress), this,
						"Loading " + "(" + syncEngine.CurrentItemNum + "/" + syncEngine.CurrentTotal + ")",
						syncEngine.CurrentItemNum, syncEngine.CurrentTotal);
				}

				Microsoft.Communications.Contacts.Contact contact = GetCanonicalContact(live);

				if (contact != null && contact.FullName != null && contact.FullName.Trim() != "")
				{
					if (!contactsByFullName.ContainsKey(contact.FullName))
					{
						contactsByFullName[contact.FullName] = contact;
						liveContactByFullname[contact.FullName] = live;
						mans.Add(new Man { FullName = contact.FullName, EMail = contact.EMail, Phone = contact.Phone });
					}
				}

			}

			this.FetchSem.Release();
		}

		void env_ConnectCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
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

			foreach (Microsoft.Communications.Contacts.Contact c in MapAdded.Values)
			{
				if (owner != null)
				{
					syncEngine.CurrentItemNum++;
					Dispatcher.Invoke(new SyncProgressEventHandler(owner.Progress), this,
							"Updating " + "(" + syncEngine.CurrentItemNum + "/" + syncEngine.CurrentTotal + ")",
							syncEngine.CurrentItemNum, syncEngine.CurrentTotal);
				}

				if (c.FullName != "")
				{
					Microsoft.LiveFX.Client.Contact newContact = new Microsoft.LiveFX.Client.Contact();// null;
					object objectState = new object();
					if (c != null)
						FillProviderCreatedItem(c, newContact);
					liveContacts.Add(ref newContact);
					liveContactByFullname[c.FullName] = newContact;

				}
				else
				{
				}

			}

			foreach (Microsoft.Communications.Contacts.Contact c in MapUpdated.Values)
			{
				if (owner != null)
				{
					syncEngine.CurrentItemNum++;
					Dispatcher.Invoke(new SyncProgressEventHandler(owner.Progress), this,
							"Updating " + "(" + syncEngine.CurrentItemNum + "/" + syncEngine.CurrentTotal + ")",
							syncEngine.CurrentItemNum, syncEngine.CurrentTotal);
				}

				// UPDATE
				FillProviderCreatedItem(c, liveContactByFullname[c.FullName]);
				liveContactByFullname[c.FullName].Update();
			}

			UpdateSem.Release();
		}
	}
}
