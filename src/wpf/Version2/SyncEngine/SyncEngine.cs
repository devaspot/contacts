//
//	Copyright (c) 2009 Synrc Research Center
//

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Communications.Contacts;
using System.Threading;
using System.Windows.Threading;
using System.IO;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Reflection;
using System.Drawing;
using System.Windows.Interop;
using System.Windows;
using CustomWindow;

namespace Synrc
{
	public class SyncEngine : DispatcherObject
	{
		#region protected 
		protected IGUICallbacks owner = null;

		protected Thread fetchThreadLocal = null;		// 1 - Sync Engine Threads
		protected Thread fetchThreadRemote = null;		// 2
		protected Thread syncThread = null;				// 3
		protected Thread updateThreadLocal = null;		// 4
		protected Thread updateThreadRemote = null;		// 5
		protected Thread lastSyncSaverThread = null;	// 6

		protected Semaphore syncSemForLocal = null;
		protected Semaphore syncSemForRemote = null;
		protected Semaphore lastSyncSaverSem = null;

		protected ISyncSource localSyncSource = null;
		protected ISyncSource remoteSyncSource = null;
		protected IList<IMan> futureManList = null;
		protected IList<Contact> futureContactList = null;
		#endregion

		#region semaphores
		public virtual Semaphore SyncSemForLocal
		{
			get
			{
				if (syncSemForLocal == null)
					syncSemForLocal = new Semaphore(1, 1, "syncSemForLocal"); ;
				return syncSemForLocal;
			}
		}
		public virtual Semaphore SyncSemForRemote
		{
			get
			{
				if (syncSemForRemote == null)
					syncSemForRemote = new Semaphore(1, 1, "syncSemForRemote"); ;
				return syncSemForRemote;
			}
		}
		public virtual Semaphore LastSyncSaverSem
		{
			get
			{
				if (lastSyncSaverSem == null)
					lastSyncSaverSem = new Semaphore(1, 1, "lastSyncSaverSem"); ;
				return lastSyncSaverSem;
			}
		}
		#endregion

		public IList<IMan> SyncedMans { get { return futureManList; } }
		public SyncProgressEventHandler SyncProgress;
		public int CurrentItemNum;
		public int CurrentTotal;
		public bool IsXP;

		public SyncEngine(IGUICallbacks host, bool XP)
		{
			owner = host;
			IsXP = XP;
		}
		
		DateTime now = DateTime.MinValue;
		
		public void ClearLabelsFrom(Contact c)
		{
			if (c == null) return;
			ILabelCollection labels = c.ContactIds.GetLabelsAt(0) as ILabelCollection;
			labels.Clear();
		}

		public void CopyReplicatedToLabelsFromTo(Contact from, Contact to)
		{
			if (from == null) return;
			ILabelCollection replicatedToLabels = from.ContactIds.GetLabelsAt(0);
			ILabelCollection labels = to.ContactIds.GetLabelsAt(0);
			if (replicatedToLabels != null)
				foreach (string s in replicatedToLabels)
					labels.Add(s);
		}

		public void SetReplicatedTo(Contact c, string label)
		{
			ILabelCollection labels = c.ContactIds.GetLabelsAt(0);
			labels.Add(label);
		}

		public void ClearReplicatedTo(Contact c, string label)
		{
			ILabelCollection labels = c.ContactIds.GetLabelsAt(0);
			labels.Remove(label);
		}

		public bool IsReplicatedTo(Contact contact, string name)
		{
			if (contact == null)
				return false;

			ILabelCollection labels = contact.ContactIds.GetLabelsAt(0);
			if (labels != null)
				return labels.Contains(name);
			return false;
		}

		public void MergeContacts(ISyncSource local, ISyncSource remote, string FullName, bool copylabels)
		{
			Contact localLastSyncContact = LastSyncContact(localSyncSource, FullName);
			Contact remoteLastSyncContact = LastSyncContact(remoteSyncSource, FullName);

			Contact m1 = Merge(remoteSyncSource.Contacts[FullName], localSyncSource.Contacts[FullName]);
			Contact m2 = Merge(remoteSyncSource.Contacts[FullName], localSyncSource.Contacts[FullName]);

			if (copylabels)
			{
				CopyReplicatedToLabelsFromTo(localLastSyncContact, m1);
				CopyReplicatedToLabelsFromTo(remoteLastSyncContact, m1);
				CopyReplicatedToLabelsFromTo(localLastSyncContact, m2);
				CopyReplicatedToLabelsFromTo(remoteLastSyncContact, m2);
			}
			else
			{
				ClearLabelsFrom(m1);
				ClearLabelsFrom(m2);
			}

			SetReplicatedTo(m1, localSyncSource.Name);
			SetReplicatedTo(m2, remoteSyncSource.Name);

			remoteSyncSource.MapUpdated[FullName] = m1;
			localSyncSource.MapUpdated[FullName] = m2;
		}

		public void SyncTask()
		{
			localSyncSource.FetchSem.WaitOne();
			remoteSyncSource.FetchSem.WaitOne();

			if (SyncCanceled)
			{
				SyncSemForLocal.Release();
				SyncSemForRemote.Release();
				return;
			}

			CurrentTotal = localSyncSource.Contacts.Count + remoteSyncSource.Contacts.Count;
			CurrentItemNum = 0;

			foreach (Contact c in localSyncSource.Contacts.Values)
			{
				if (owner != null)
				{
					CurrentItemNum++;
					Dispatcher.Invoke(new SyncProgressEventHandler(owner.Progress), localSyncSource,
						"Syncing " + "(" + CurrentItemNum + "/" + CurrentTotal + ")",
						CurrentItemNum, CurrentTotal);
					
				}

				if (!remoteSyncSource.Contacts.ContainsKey(c.FullName))
				{
					if (!remoteSyncSource.MapAdded.ContainsKey(c.FullName)) {
						remoteSyncSource.MapAdded[c.FullName] = c;
					}
					else
						Merge(remoteSyncSource.MapAdded[c.FullName], c);
				}
				else
				{
					DateTime? localLastModification = c.Dates["LastModificationTime"].Value;
					DateTime? localLastSync = LastSyncTime(localSyncSource, c.FullName);
					DateTime? remoteLastModification = remoteSyncSource.Contacts[c.FullName].Dates["LastModificationTime"];
					DateTime? remoteLastSync = LastSyncTime(remoteSyncSource, c.FullName);
					bool localHasModified = localLastModification > localLastSync;
					bool remoteHasModified = remoteLastModification > remoteLastSync;
					Contact localLastSyncContact = LastSyncContact(localSyncSource, c.FullName);
					Contact remoteLastSyncContact = LastSyncContact(remoteSyncSource, c.FullName);

					if (localHasModified && !remoteHasModified)
					{
						if (IsReplicatedTo(remoteLastSyncContact, localSyncSource.Name))
						{
							// D + C = u (Update from Dirty Contact)
							SetReplicatedTo(c, localSyncSource.Name);
							SetReplicatedTo(c, remoteSyncSource.Name);
							remoteSyncSource.MapUpdated[c.FullName] = c;
						}
						else
						{
							// D + C = m (Merge with Dirty)
							MergeContacts(localSyncSource, remoteSyncSource, c.FullName, true);
						}
					}
					else if (remoteHasModified && !localHasModified)
					{
						if (IsReplicatedTo(localLastSyncContact, remoteSyncSource.Name))
						{
							// D + C = u (Update from Dirty Contact)
							SetReplicatedTo(remoteSyncSource.Contacts[c.FullName], remoteSyncSource.Name);
							SetReplicatedTo(remoteSyncSource.Contacts[c.FullName], localSyncSource.Name);
							localSyncSource.MapUpdated[c.FullName] = remoteSyncSource.Contacts[c.FullName];
						}
						else
						{
							// D + C = m (Merge with Dirty)
							MergeContacts(localSyncSource, remoteSyncSource, c.FullName, true);
						}
					}
					else if (localHasModified && remoteHasModified)
					{
						// D + D = m (Merge Two Dirty Contacts Always)
						MergeContacts(localSyncSource, remoteSyncSource, c.FullName, false);
					}
					else if (!localHasModified && !remoteHasModified)
					{
						if (localLastSyncContact.Labels.Equals(remoteLastSyncContact.Labels))
						{
							// NOTHING TO DO
						}
						else
						{
							// C + C = m (Merge Two Clear Contacts if sync source lables are not identical)
							MergeContacts(localSyncSource, remoteSyncSource, c.FullName, true);
						}
					}
				}
			}

			foreach (Contact c in remoteSyncSource.Contacts.Values)
			{

				if (owner != null)
				{
					CurrentItemNum++;
					Dispatcher.Invoke(new SyncProgressEventHandler(owner.Progress), remoteSyncSource,
						"Syncing " + "(" + CurrentItemNum + "/" + CurrentTotal + ")",
						CurrentItemNum, CurrentTotal);
				}

				if (!localSyncSource.Contacts.ContainsKey(c.FullName))
				{
					if (!localSyncSource.MapAdded.ContainsKey(c.FullName)) {
						localSyncSource.MapAdded[c.FullName] = c;
					}
					else
						Merge(localSyncSource.MapAdded[c.FullName], c);
				}
			}

			CurrentTotal = 0;
			CurrentItemNum = 0;
			
			SyncSemForLocal.Release();
			SyncSemForRemote.Release();
		}

		bool cancelSync = false;
		public bool SyncCanceled { get { return cancelSync; } set { cancelSync = value;  } }
		
		public void UpdateFutureList()
		{
			localSyncSource.FetchSem.WaitOne();

			localSyncSource.FetchTask();

			if (SyncCanceled)
				return;

			foreach (Contact contact in localSyncSource.Contacts.Values)
			{
				futureManList.Add(new Man { FullName = contact.FullName, Phone = contact.Phone, EMail = contact.EMail });
			}

			CurrentTotal = 0;
			CurrentItemNum = 0; 

			if (owner != null)
				Dispatcher.BeginInvoke(new SyncEndEventHandler(owner.EndSync), localSyncSource, remoteSyncSource);

			if (owner != null)
				Dispatcher.BeginInvoke(new VoidDelegate(owner.ShowContacts));

			if (owner != null)
				Dispatcher.BeginInvoke(new VoidDelegate(owner.UpdateItems));

		}
		public void SaveTask(ISyncSource source, bool updateFutureManList, DateTime lastSyncTime)
		{
			if (SyncCanceled) return;

			string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string appDataSynrcContacts = appData + "/Synrc/Contact Manager/" + source.Name;

			if (Directory.Exists(appDataSynrcContacts))
				Directory.Delete(appDataSynrcContacts, true);

			DirectoryInfo dirinfo = Directory.CreateDirectory(appDataSynrcContacts);

			//Dispatcher.BeginInvoke(new VoidDelegate(owner.ClearProgressBar));
			//Dispatcher.BeginInvoke(new VoidInt(owner.SetBarLimits), source.Contacts.Count);

			CurrentTotal = source.Contacts.Count;
			CurrentItemNum = 0;

			foreach (Contact contact in source.Contacts.Values)
			{
				//if (owner != null)
				//	Dispatcher.BeginInvoke(new VoidString(owner.IncBarPosition), "Saving " + source.Name);

				if (owner != null)
				{
					Dispatcher.Invoke(new SyncProgressEventHandler(owner.Progress), source,
							"Saving " + source.Name + " (" + CurrentItemNum + "/" + CurrentTotal + ")",
							CurrentItemNum, CurrentTotal);
					CurrentItemNum++;
				}

				if (contact.Dates["LastSync"] != null)
					contact.Dates["LastSync"] = lastSyncTime;
				else
					contact.Dates.Add(lastSyncTime, "LastSync");

				//ClearReplicatedTo(contact, source.Name);
				contact.Save(appDataSynrcContacts + "/" + contact.FullName + ".contact");

				if (updateFutureManList)
					futureManList.Add(new Man { FullName = contact.FullName, Phone = contact.Phone, EMail = contact.EMail });

			}
		}
		public void lastSyncSaverThreadTask()
		{
			localSyncSource.UpdateSem.WaitOne();
			remoteSyncSource.UpdateSem.WaitOne();

			if (SyncCanceled)
			{
				LastSyncSaverSem.Release();
				return;
			}

			{

				now = DateTime.Now.ToUniversalTime().AddSeconds(1);

				futureManList.Clear();

				foreach (Contact c in localSyncSource.MapAdded.Values)
					localSyncSource.Contacts[c.FullName] = c;

				foreach (Contact c in remoteSyncSource.MapAdded.Values)
					remoteSyncSource.Contacts[c.FullName] = c;

				foreach (Contact c in localSyncSource.MapUpdated.Values)
					localSyncSource.Contacts[c.FullName] = c;

				foreach (Contact c in remoteSyncSource.MapUpdated.Values)
					remoteSyncSource.Contacts[c.FullName] = c;

				foreach (Contact c in localSyncSource.MapRemoved.Values)
					localSyncSource.Contacts[c.FullName] = null;

				foreach (Contact c in remoteSyncSource.MapRemoved.Values)
					remoteSyncSource.Contacts[c.FullName] = null;

				SaveTask(localSyncSource, false, now);
				SaveTask(remoteSyncSource, true, now);
			}

			if (owner != null)
				Dispatcher.BeginInvoke(new SyncEndEventHandler(owner.EndSync), localSyncSource, remoteSyncSource);

			if (owner != null)
				Dispatcher.BeginInvoke(new VoidDelegate(owner.ShowContacts));

			if (owner != null)
				Dispatcher.BeginInvoke(new VoidDelegate(owner.UpdateItems));

			LastSyncSaverSem.Release();

		}

		public Contact Merge(Contact a, Contact b)
		{
			//MergeEmails();
			//MergePhones();
			return a;
		}

		public Contact LastSyncContact(ISyncSource source, string FullName)
		{
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string appDataSynrcContacts = appData + "/Synrc/Contact Manager/" + source.Name;
			if (File.Exists(appDataSynrcContacts + "/" + FullName + ".contact"))
			{
				Contact contact = new Contact(appDataSynrcContacts + "/" + FullName + ".contact");
				return contact;

			}
			return null;
		}

		public DateTime LastSyncTime(ISyncSource source, string FullName)
		{
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string appDataSynrcContacts = appData + "/Synrc/Contact Manager/" + source.Name;

			if (File.Exists(appDataSynrcContacts + "/" + FullName + ".contact"))
			{
				Contact contact = new Contact(appDataSynrcContacts + "/" + FullName + ".contact");
				if (contact != null && contact.Dates["LastSync"] != null)
				{
					return contact.Dates["LastSync"].Value;
				} 

			}
			
			return DateTime.MinValue;
		}

		public void CleanupResources()
		{
			if (fetchThreadLocal != null) fetchThreadLocal.Abort();
			if (fetchThreadRemote != null) fetchThreadRemote.Abort();
			if (syncThread != null) syncThread.Abort();
			if (updateThreadLocal != null) updateThreadLocal.Abort();
			if (updateThreadRemote != null) updateThreadRemote.Abort();
			if (lastSyncSaverThread != null) lastSyncSaverThread.Abort();

			fetchThreadLocal = null;
			fetchThreadRemote = null;
			syncThread = null;
			updateThreadLocal = null;
			updateThreadRemote = null;
			lastSyncSaverThread = null;

			if (localSyncSource != null)
			{
				localSyncSource.MapAdded.Clear();
				localSyncSource.MapRemoved.Clear();
				localSyncSource.MapUpdated.Clear();
			}

			if (remoteSyncSource != null)
			{
				remoteSyncSource.MapAdded.Clear();
				remoteSyncSource.MapRemoved.Clear();
				remoteSyncSource.MapUpdated.Clear();
			}

			try
			{
				syncSemForLocal.Release();
			}
			catch { }

			try
			{
				localSyncSource.FetchSem.Release();
			}
			catch { }

			try
			{
				localSyncSource.UpdateSem.Release();
			}
			catch { }

			if (localSyncSource == remoteSyncSource)
				return;

			try
			{
				syncSemForRemote.Release();
			}
			catch { }

			try
			{
				remoteSyncSource.FetchSem.Release();
			}
			catch { }

			try
			{
				remoteSyncSource.UpdateSem.Release();
			}
			catch { }

		}
		public void StartRetrievingLocal()
		{
			if (fetchThreadLocal != null)
				CleanupResources();

			//localSyncSource.FetchSem.WaitOne();
			//fetchThreadLocal = new Thread(localSyncSource.FetchTask);
			//fetchThreadLocal.Priority = ThreadPriority.Highest;
			//fetchThreadLocal.Start();

			lastSyncSaverThread = new Thread(UpdateFutureList);
			lastSyncSaverThread.Priority = ThreadPriority.Highest;
			lastSyncSaverThread.Start();
		}
		ISyncSource lastSyncSource = null;
		public void StartThreads()
		{
			CleanupResources();

			CurrentTotal = 0;
			CurrentItemNum = 0;

			lastSyncSource = localSyncSource;

			localSyncSource.FetchSem.WaitOne();
			fetchThreadLocal = new Thread(localSyncSource.FetchTask);
			fetchThreadLocal.Priority = ThreadPriority.Highest;
			fetchThreadLocal.SetApartmentState(ApartmentState.STA);
			fetchThreadLocal.Start();

			remoteSyncSource.FetchSem.WaitOne();
			fetchThreadRemote = new Thread(remoteSyncSource.FetchTask);
			fetchThreadRemote.Priority = ThreadPriority.Highest;
			fetchThreadRemote.SetApartmentState(ApartmentState.STA);
			fetchThreadRemote.Start();

			SyncSemForLocal.WaitOne();
			SyncSemForRemote.WaitOne();
			localSyncSource.SyncSem = SyncSemForLocal;
			remoteSyncSource.SyncSem = SyncSemForRemote;
			syncThread = new Thread(SyncTask);
			syncThread.Priority = ThreadPriority.Highest;
			syncThread.Start();

			localSyncSource.UpdateSem.WaitOne();
			updateThreadLocal = new Thread(localSyncSource.UpdateTask);
			updateThreadLocal.Priority = ThreadPriority.Highest;
			updateThreadLocal.Start();

			remoteSyncSource.UpdateSem.WaitOne();
			updateThreadRemote = new Thread(remoteSyncSource.UpdateTask);
			updateThreadRemote.Priority = ThreadPriority.Highest;
			updateThreadRemote.Start();

			LastSyncSaverSem.WaitOne();
			lastSyncSaverThread = new Thread(lastSyncSaverThreadTask);
			lastSyncSaverThread.Priority = ThreadPriority.Highest;
			lastSyncSaverThread.Start();

		}

		public bool CheckCredentials(ISyncSource source)
		{
			if (source.NeedAuthorization && source.Credentials == null)
			{

				ISyncSourceProfile logon = null;
				if (IsXP)
					logon = new LogonWindowXP();
				else
					logon = new LogonWindow();

				logon.ProviderName = source.DisplayName;

				Bitmap providerImage = source.Image;
				BitmapSource imageSource = null;

				if (providerImage != null)
				{
					IntPtr HBitmap = providerImage.GetHbitmap();
					imageSource = Imaging.CreateBitmapSourceFromHBitmap(HBitmap, IntPtr.Zero,
						Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
					providerImage.Dispose();
					logon.ImageSource = imageSource;
				}

				(logon as Window).ShowDialog();

				bool? b = (logon as Window).DialogResult;
				if (b == null || b == false)
				{
					return false;
				}

				if (logon.RememberSettings)
				{
					source.ClearCredentials = false;
				}
				else
					source.ClearCredentials = true;
				source.Credentials = new NetworkCredential(logon.Login, logon.Password);
				if (logon.HumanNotation)
				{
					source.Notation = NameNotation.Human;
				}
				else
				{
					source.Notation = NameNotation.Formal;
				}
				return true;
			}
			else
				return true;
		}

		public IList<IMan> Sync(ISyncSource local, ISyncSource remote)
		{
			futureManList = new List<IMan>();
			cancelSync = false;
			if (local != remote && local != null && remote != null)
			{
				localSyncSource = local;
				remoteSyncSource = remote;
				if (!CheckCredentials(localSyncSource))
				{
					cancelSync = true;
					return futureManList;
				}

				if (!CheckCredentials(remoteSyncSource))
				{
					cancelSync = true;
					return futureManList;
				}
				StartThreads();
			}
			else if (local == remote && local != null)
			{
				localSyncSource = local;
				remoteSyncSource = remote;
				if (!CheckCredentials(localSyncSource))
				{
					cancelSync = true;
					return futureManList;
				}
				StartRetrievingLocal();
			}
			return futureManList;
		}

	}
}
