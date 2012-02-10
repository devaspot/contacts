//
//	Copyright (c) 2009 Synrc Research Center
//

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Communications.Contacts;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using Standard;
using System.Drawing;
using Synrc.Properties;

namespace Synrc
{
	public class WindowsContactsProvider : ContactsProvider, ISyncSource
    {
		ContactManager manager = null;
		public override string Name	{ get { return "WindowsContacts"; }	}
		public override string DisplayName { get { return "Windows Contacts"; } }
		public Bitmap Image
		{
			get
			{
				return Resources.WindowsContacts;
			}
		}
		public WindowsContactsProvider(IGUICallbacks host, SyncEngine syncEngine)
		{
			owner = host;
			manager = new ContactManager();
			this.syncEngine = syncEngine;
		}
		public int FetchCount
		{
			get
			{
				if (syncEngine.SyncCanceled) return 0;

				IEnumerator<FileInfo> files
					= FileWalker.GetFiles(new DirectoryInfo(manager.RootDirectory), "*.contact", manager.UseSubfolders).GetEnumerator();
				int i = 0;
				while (files.MoveNext()) i++;

				return i;
			}
		}
		public void FetchTask()
		{
			if (syncEngine.SyncCanceled)
			{
				this.FetchSem.Release();
				return;
			}

			syncEngine.CurrentTotal += FetchCount;

			mans.Clear();
			contactsByFullName.Clear();

			IList<Contact> retrieve = manager.GetContactCollection();

			foreach (Contact contact in retrieve)
			{
				if (owner != null)
				{
					syncEngine.CurrentItemNum++;
					Dispatcher.Invoke(new SyncProgressEventHandler(owner.Progress), this,
						"Loading " + "(" + syncEngine.CurrentItemNum + "/" + syncEngine.CurrentTotal + ")",
						syncEngine.CurrentItemNum, syncEngine.CurrentTotal);
				}

				contactsByFullName[contact.FullName] = contact;

				FileInfo fi = new FileInfo(contact.Path);
				if (contact.Dates["LastModificationTime"] != null)
					contact.Dates["LastModificationTime"] = fi.LastWriteTime.ToUniversalTime();
				else
					contact.Dates.Add(fi.LastWriteTime.ToUniversalTime(), "LastModificationTime");
				mans.Add(new Man { FullName = contact.FullName, EMail = contact.EMail, Phone = contact.Phone });
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

			foreach (Contact c in MapAdded.Values)
			{
				if (owner != null)
				{
					syncEngine.CurrentItemNum++;
					Dispatcher.Invoke(new SyncProgressEventHandler(owner.Progress), this,
							"Updating " + "(" + syncEngine.CurrentItemNum + "/" + syncEngine.CurrentTotal + ")",
							syncEngine.CurrentItemNum, syncEngine.CurrentTotal);
				}

				manager.AddContact(c);
				Contacts[c.FullName] = c;
			}

			foreach (Contact c in MapUpdated.Values)
			{
				
				manager.Remove(Contacts[c.FullName].Id);
				manager.AddContact(c);
				
			}

			UpdateSem.Release();
		}
	}
}
