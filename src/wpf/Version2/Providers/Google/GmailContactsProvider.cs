//
//	Copyright (c) 2009 Synrc Research Center
//

using System;
using System.Collections.Generic;
using System.Text;
using Google.GData.Contacts;
using Google.GData.Client;
using Google.GData.Extensions;
using Microsoft.Communications.Contacts;
using System.Windows;
using System.Net;
using System.Threading;
using System.Drawing;
using Synrc.Properties;

namespace Synrc
{
	public class GmailContactsProvider : ContactsProvider, ISyncSource
	{
		ContactsFeed feed = null;
		GroupsFeed groupsFeed = null;
		ContactsService googleService = null;
		string queruUriFull = null;
		ContactsQuery contactsQuery = null;

		public Bitmap Image
		{
			get
			{
				return Resources.Google;
			}
		}

		public override string Name { get { return "Google"; } }
		public override string DisplayName { get { return "Google"; } }
		public override bool NeedAuthorization { get { return true; } }

		public GmailContactsProvider(IGUICallbacks host, SyncEngine syncEngine)
		{
			this.syncEngine = syncEngine;
			owner = host;
		}
		public void FillProviderCreatedItem(Contact contact, ContactEntry c)
		{
			c.Title.Text = contact.GetFullName(this.Notation);

			foreach (Microsoft.Communications.Contacts.PhoneNumber phone in contact.PhoneNumbers)
			{
				Google.GData.Extensions.PhoneNumber p = new Google.GData.Extensions.PhoneNumber();
			}
		}
		public Contact GetCanonicalContact(ContactEntry c)
		{
			Contact contact = new Contact();

			IList<string> groups = new List<string>();
			foreach (GroupMembership group in c.GroupMembership)
			{
				foreach (GroupEntry ge in groupsFeed.Entries)
				{
					if (group.HRef.Equals(ge.Id.AbsoluteUri))
					{
						groups.Add(ge.Title.Text);
					}
				}
				
			}

			string title = c.Title.Text;
			if (title.Contains("\""))
				title = title.Replace("\"", "%20");

			Name name = GetCannonicalName(title, null, null, null,
			    Notation, NameNotation.Formal);

			contact.Names.Add(name);
			foreach (string g in groups)
			{
				contact.Names.GetLabelsAt(0).Add(g);
			}

			bool setPreffered = false;
			foreach (EMail e in c.Emails)
			{
				EmailAddress email = new EmailAddress(e.Address);
				if (setPreffered == false)
				{
					contact.EmailAddresses.Add(email, PropertyLabels.Preferred);
					setPreffered = true;
				}
				else
					contact.EmailAddresses.Add(email);
			}

			foreach (Organization o in c.Organizations)
			{
				Microsoft.Communications.Contacts.Position pos  =
					new Microsoft.Communications.Contacts.Position(o.Name, o.Title, o.Name, null, null, o.Title, null);

				contact.Positions.Add(pos, PropertyLabels.Business);
			}

			foreach (PostalAddress pa in c.PostalAddresses)
			{
				PhysicalAddress adr = new PhysicalAddress(
					null, pa.Value, null,null, null, null, null, 
					null
				);

				if (pa.Home)
				{
					contact.Addresses.Add(adr, PropertyLabels.Personal);
				}
				else if (pa.Work)
				{
					contact.Addresses.Add(adr, PropertyLabels.Business);
				}
				else
					contact.Addresses.Add(adr);
			}

			foreach (Google.GData.Extensions.PhoneNumber n in c.Phonenumbers)
			{

				Microsoft.Communications.Contacts.PhoneNumber phone = 
					new Microsoft.Communications.Contacts.PhoneNumber(n.Value);

				if (n.Work)
				{
					if (n.Primary)
						contact.PhoneNumbers.Add(phone, PhoneLabels.Voice, PropertyLabels.Business, PropertyLabels.Preferred);
					else
						contact.PhoneNumbers.Add(phone, PhoneLabels.Voice, PropertyLabels.Business);
				}

				if (n.Home)
				{
					if (n.Primary)
						contact.PhoneNumbers.Add(phone, PhoneLabels.Voice, PropertyLabels.Personal, PropertyLabels.Preferred);
					else
						contact.PhoneNumbers.Add(phone, PhoneLabels.Voice, PropertyLabels.Personal);
				}

				if (n.Rel == null) break;

				if (n.Rel.Contains("#mobile"))
					contact.PhoneNumbers.Add(phone, PhoneLabels.Cellular);

				if (n.Rel.Contains("#home_fax"))
					contact.PhoneNumbers.Add(phone, PhoneLabels.Fax, PropertyLabels.Personal);

				if (n.Rel.Contains("#work_fax"))
					contact.PhoneNumbers.Add(phone, PhoneLabels.Fax, PropertyLabels.Business);

				if (n.Rel.Contains("#pager"))
					contact.PhoneNumbers.Add(phone, PhoneLabels.Pager);

				if (n.Rel.Contains("#other"))
					contact.PhoneNumbers.Add(phone, PhoneLabels.Voice);
				
			}

			DateTime lastChanged = c.Updated.ToUniversalTime();
			contact.Dates.Add(lastChanged, "LastModificationTime" );
			
			int lastIndexOfDash = c.Id.AbsoluteUri.LastIndexOf('/') + 1;
			string absoluteUri = c.Id.AbsoluteUri.Substring(lastIndexOfDash,c.Id.AbsoluteUri.Length-lastIndexOfDash);
			Guid guid = new Guid(absoluteUri.PadLeft(32, '0'));
			contact.ContactIds.Add(guid, "Gmail");

			return contact;
		}

		public void FetchTask()
		{
			if (syncEngine.SyncCanceled)
			{
				this.FetchSem.Release();
				return;
			}

			mans.Clear();
			contactsByFullName.Clear();

			googleService = new ContactsService("Contact Infomation");

			if (Credentials == null)
			{
				this.FetchSem.Release();
				return;
			}
			else
			{
				if (Credentials.UserName != null && !Credentials.UserName.ToLower().EndsWith("@gmail.com"))
				{
					Credentials.UserName += "@gmail.com";
				}
				googleService.setUserCredentials(Credentials.UserName, Credentials.Password);
			}

			((GDataRequestFactory)googleService.RequestFactory).Timeout = 3000;

			queruUriFull = ContactsQuery.CreateContactsUri(Credentials.UserName, ContactsQuery.fullProjection);
			contactsQuery = new ContactsQuery(queruUriFull);
			contactsQuery.NumberToRetrieve = 1000;

			try
			{

				feed = googleService.Query(contactsQuery);
			}
			catch (Google.GData.Client.GDataRequestException e)
			{
				Credentials = null;

				Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, e.InnerException.Message, null);

				this.FetchSem.Release();
				return;
			}
			catch (Google.GData.Client.InvalidCredentialsException e)
			{
				Credentials = null;

				Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, e.Message, null);

				this.FetchSem.Release();
				return;
			}
			catch (CaptchaRequiredException e)
			{
				Credentials = null;

				Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, e.Message, "https://www.google.com/accounts/UnlockCaptcha");

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

			syncEngine.CurrentTotal += feed.Entries.Count;
			
			GroupsQuery groupsQuery = new GroupsQuery(GroupsQuery.
				CreateGroupsUri(Credentials.UserName, ContactsQuery.thinProjection));

			try
			{
				groupsFeed = googleService.Query(groupsQuery);
			} 
			catch (Google.GData.Client.GDataRequestException e)
			{
				Credentials = null;

				Dispatcher.Invoke(new SyncCancelEventHandler(owner.CancelSync),
						this, e.InnerException.Message, null);
								
				this.FetchSem.Release();
				return;
			}

			foreach (ContactEntry entry in feed.Entries)
			{

				if (owner != null)
				{
					Dispatcher.Invoke(new SyncProgressEventHandler(owner.Progress), this,
						"Loading " + "(" + syncEngine.CurrentItemNum + "/" + syncEngine.CurrentTotal + ")",
						syncEngine.CurrentItemNum, syncEngine.CurrentTotal);
					syncEngine.CurrentItemNum++;
				}

				Contact contact = GetCanonicalContact(entry);
				
				string fullname = contact.FullName;

				if (fullname == null || fullname.Trim() == "")
				{
				}
				else
				{
					contactsByFullName[fullname] = contact;
					mans.Add(new Man { FullName = fullname, EMail = contact.EMail, Phone = contact.Phone });
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

			foreach (Contact contact in MapAdded.Values)
			{
				if (owner != null)
				{
					Dispatcher.Invoke(new SyncProgressEventHandler(owner.Progress), this,
						"Updating " + this.Name + "(" + syncEngine.CurrentItemNum + "/" + syncEngine.CurrentTotal + ")",
						syncEngine.CurrentItemNum, syncEngine.CurrentTotal);
					syncEngine.CurrentItemNum++;
				}

				ContactEntry newEntry = new ContactEntry();
				object userData = new object();
				FillProviderCreatedItem(contact, newEntry);
				{
					if (contact.FullName != "")
					{
						googleService.InsertAsync(contactsQuery.Uri, newEntry, userData);
					}
					else
					{
					}
				}
			}

			if (ClearCredentials)
				Credentials = null;

			UpdateSem.Release();
		}
	}
}
