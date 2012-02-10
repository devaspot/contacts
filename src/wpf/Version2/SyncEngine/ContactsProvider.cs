//
//	Copyright (c) 2009 Synrc Research Center
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;
using System.Threading;
using Microsoft.Communications.Contacts;
using System.IO;
using System.Net;

namespace Synrc
{
	public delegate void VoidDelegate();
	public delegate void VoidInt(int c);
	public delegate void VoidString(string s);
	public delegate void VoidIntString(int c, string s);
	public delegate void SyncProgressEventHandler(ISyncSource source, string s, int c, int max);
	public delegate void SyncEndEventHandler(ISyncSource source, ISyncSource destination);
	public delegate void SyncCancelEventHandler(ISyncSource source, string message, string link);

    public class ContactsProvider : DispatcherObject
	{
		public Name GetCannonicalName(string fullName, string firstName, string middleName, string lastName, NameNotation inNotation, NameNotation outNotation)
		{
			if (fullName.Trim() == "") fullName = null;
			if (firstName == null || firstName.Trim() == "") firstName = null;
			if (middleName == null || middleName.Trim() == "") middleName = null;
			if (lastName == null || lastName.Trim() == "") lastName = null;

			if (fullName != null)
			{
				string[] names = fullName.Split(new char[] { ' ' });

				if (firstName == null && lastName == null && middleName == null)
				{
					if (inNotation == NameNotation.Human)
					{
						if (names.Length > 0)
							firstName = names[0];
					}
					else
					{
						if (names.Length >= 2)
							firstName = names[1];
						else if (names.Length > 0)
							firstName = names[0];
					}

					if (inNotation == NameNotation.Human)
					{
						if (names.Length == 3) // First Middle Last 
							middleName = names[1];
						//else if (names.Length == 1)
						//	middleName = names[0];
					}
					else
					{
						if (names.Length == 3) // Last First Middle  
							middleName = names[2];
					}

					if (inNotation == NameNotation.Human)
					{
						if (names.Length == 3)  //  First Middle Last
							lastName = names[2];
						else if (names.Length == 2) //  First Last
							lastName = names[1];
					}
					else
					{
						if (names.Length >= 2) //  Last First 
							lastName = names[0];
					}
				}
			}
			
			if (outNotation == NameNotation.Human)
				return new Name(firstName, middleName, lastName, NameCatenationOrder.GivenMiddleFamily);
			else 
				return new Name(firstName, middleName, lastName, NameCatenationOrder.FamilyGivenMiddle);
		}

		protected IGUICallbacks owner = null;
		protected IDictionary<string, Contact> contactsByFullName = new Dictionary<string, Contact>();
		protected IList<IMan> mans = new List<IMan>();
		protected IDictionary<string, Contact> mapAdded = new Dictionary<string, Contact>();
		protected IDictionary<string, Contact> mapRemoved = new Dictionary<string, Contact>();
		protected IDictionary<string, Contact> mapUpdated = new Dictionary<string, Contact>();
		protected bool quiet = false;
		protected Semaphore fetchSem = null;
		protected Semaphore updateSem = null;
		protected Semaphore syncSem = null;
		protected SyncEngine syncEngine = null;
		protected bool clearCredentials = false;
		protected NetworkCredential credentials = null;
		protected NameNotation nameNotation = NameNotation.Formal;
		protected string id = "";

		public virtual bool ClearCredentials { get { return clearCredentials; } set { clearCredentials = value; } }
		public virtual bool NeedAuthorization { get { return false; } }
		public virtual NameNotation Notation { get { return nameNotation; } set { nameNotation = value; } }
		public virtual NetworkCredential Credentials { get { return credentials; } set { credentials = value; } }
		public virtual string Name { get { return null; } set { } }
		public virtual string DisplayName { get { return null; } set { } }
		public virtual IDictionary<string, Contact> MapAdded { get { return mapAdded; } set { mapAdded = value; } }
		public virtual IDictionary<string, Contact> MapUpdated { get { return mapUpdated; } set { mapUpdated = value; } }
		public virtual IDictionary<string, Contact> MapRemoved { get { return mapRemoved; } set { mapRemoved = value; } }
		public virtual IList<IMan> Mans { get { return mans; } }
		public virtual IDictionary<string, Contact> Contacts { get { return contactsByFullName; } }
		public virtual string Id { get { return id; } set { id = value; } }

		public virtual Semaphore FetchSem
		{
			get
			{
				if (fetchSem == null)
					fetchSem = new Semaphore(1, 1, "fetchSem:" + Name); ;
				return fetchSem;
			}
		}
		public virtual Semaphore UpdateSem
		{
			get
			{
				if (updateSem == null)
					updateSem = new Semaphore(1, 1, "updateSem:" + Name); ;
				return updateSem;
			}
		}
		public virtual Semaphore SyncSem
		{
			set
			{
				syncSem = value;
			}
			get
			{
				return syncSem;
			}
		}

		
    }

}
