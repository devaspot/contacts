//
//	Copyright (c) 2009 Synrc Research Center
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Standard;
using System.Collections;
using Microsoft.Communications.Contacts;
using System.Threading;
using Microsoft.ContactsBridge.Interop;
using System.Net;
using Microsoft.Win32;
using Standard.Interop;
using System.Reflection;
using System.Runtime.InteropServices;
using CustomWindow;

namespace Synrc
{
	public delegate string StringDelegate();
	public delegate bool MatchDelegate(IMan man, string text);

    public partial class AddressBookWindow : EssentialWindow, IGUICallbacks
    {

		ISyncSource outlook = null;
		ISyncSource vista   = null;
		ISyncSource gmail   = null;
		ISyncSource nokia   = null;
		ISyncSource live    = null;

		#region Custom Window
		protected override Decorator GetWindowButtonsPlaceholder()
		{
			return WindowButtonsPlaceholder;
		}

		private void Header_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
				this.DragMove();
		}

		private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
		{
			if (this.Width + e.HorizontalChange > 10)
				this.Width += e.HorizontalChange;
			if (this.Height + e.VerticalChange > 10)
				this.Height += e.VerticalChange;
		}
		#endregion

		NokiaDiscoverer nokiaDiscoverer = null;
		Cursor cur;
		ISyncSource currentProvider = null;
        IList<IMan> searchContext; 
		IList<IMan> searchList = new List<IMan>();
		SearchTextBox searchBox = null;
		SyncEngine syncEngine = null;

		public Window ContactWindow = null;
		public bool HandleLocationAndPositionChanging = true;

		public ISyncSource CurrentProvider
		{
			get { return currentProvider; }
			set { currentProvider = value; }
		}


		public bool IsNokiaInstalled()
		{
			bool yes = false;

			RegistryKey masterKey = Registry.LocalMachine.OpenSubKey
   ("SOFTWARE\\PCSuite");
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

		public bool IsNET35Installed()
		{
			try { 
				AppDomain.CurrentDomain.Load("System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
				return true;
			}
			catch { 
				return false;
			}
		}

		public bool IsOutlookInstalled()
		{
			bool yes = false;

			RegistryKey masterKey = Registry.LocalMachine.OpenSubKey
				("SOFTWARE\\Classes\\Applications\\Outlook.EXE");
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

        public AddressBookWindow()
        {
            InitializeComponent();

			cachedHeight = 300;
			cachedMaxHeight = double.PositiveInfinity;
        }
		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			GlassHelper.ExtendGlassFrameComplete(this);
			GlassHelper.SetWindowThemeAttribute(this, false, false);
		}
		
		// GUI

		public void SaveCursor()
		{
			cur = this.Cursor;
			this.Cursor = Cursors.Wait;
		}
		public void RestoreCursor()
		{
			this.DataContext = searchList;
			ContactsView.Items.Refresh();
			this.Cursor = cur;
		}
		public void Progress(ISyncSource source, string s, int c, int max)
		{
			if (syncEngine.SyncCanceled)
				return;

			this.lastSyncMessage.Visibility = Visibility.Hidden;
			this.url.Visibility = Visibility.Hidden;

			this.progressBar.Maximum = max;
			this.progressBar.Minimum = 0;
			this.progressBar.Value = c;
			this.progressBar.Height = 25;
			this.progressBar.Visibility = System.Windows.Visibility.Visible;

			this.statusLabel.Content = s;
			this.statusLabel.Visibility = Visibility.Visible;

			this.syncButton.IsEnabled = false;
		}
		public void HideContacts()
		{
			expander.IsExpanded = false;
		}
		public void ShowContacts()
		{
			expander.IsExpanded = true;
		}

		bool wasGoodSync = false;

		public void HideProgressBar()
		{
			this.progressBar.Visibility = System.Windows.Visibility.Collapsed;
			this.statusLabel.Visibility = Visibility.Hidden;
			this.lastSyncMessage.Visibility = Visibility.Visible;

			this.syncButton.IsEnabled = true;
			this.syncButton.Content = "Sync";
		}

		public void EndSync(ISyncSource source, ISyncSource destination) // EndSync
		{
			//Cursor cur = this.Cursor;
			//this.Cursor = Cursors.Wait;

			HideProgressBar();
			UpdateItems();

			if (source != destination)
				this.lastSyncMessage.Content = "Last sync was made between " + source.DisplayName + " and " + destination.DisplayName;
			else
				this.lastSyncMessage.Content = "Contacts refreshed from " + source.DisplayName;

			wasGoodSync = true;

			if (source == destination)
			{
				MenuItem mi = GetSyncMenuItemByName(source.Name);
				mi.Header = source.DisplayName + " (Refresh)";
			}

			//this.Cursor = cur;
		}

		public void CancelSync(ISyncSource source, string message, string link)
		{
			syncEngine.SyncCanceled = true;

			HideProgressBar();
			UpdateItems();

			this.lastSyncMessage.Content = message;
			if (link != null)
			{
				this.url.Visibility = Visibility.Visible;
				Hyperlink o = this.url.Content as Hyperlink;
				o.NavigateUri = new Uri(link);
			}

			if (!wasGoodSync)
				currentProvider = null;

			//wasGoodSync = false;
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			// open URL
			Hyperlink source = sender as Hyperlink;
			if (source != null)
				System.Diagnostics.Process.Start(source.NavigateUri.ToString());
		}

		public void UpdateItems()
		{
			if (syncEngine.SyncCanceled) return;
			searchContext = syncEngine.SyncedMans;
			DataContext = searchContext;
			this.ContactsView.Items.Refresh();
			GridView view = ContactsView.View as GridView;
			if (view != null)
			{
				view.Columns[0].Width = 1;
				view.Columns[0].Width = double.NaN;
			}
			string items = this.ContactsView.Items.Count == 0 ? "No contacts" : 
				(this.ContactsView.Items.Count == 1 ? "1 contact" :
				this.ContactsView.Items.Count.ToString() + " contacts");
			statusMessage.Content = items + " in " + currentProvider.DisplayName;
			this.ContactsView.UpdateLayout();
		}

		void DropDown_Opened(object sender, RoutedEventArgs e)
		{
			nokiaDiscoverer.DiscoveryTask();
		}

		public void UpdateApplicationMenu()
		{
			IDictionary<string, MenuItem> toDelete = new Dictionary<string, MenuItem>();
			foreach (MenuItem mi in this.syncButton.DropDown.Items)
			{
				if (mi.Tag as NokiaPhone != null)
				{
					toDelete[mi.Header.ToString()] = mi;
				}
			}

			foreach (MenuItem mi in toDelete.Values)
			{
				this.syncButton.DropDown.Items.Remove(mi);
			}

			foreach (NokiaPhone phone in nokiaDiscoverer.nokiaDevices)
			{
				MenuItem mi = new MenuItem();
				mi.Header = phone.Name;
				mi.Tag = phone;
				mi.Click += new System.Windows.RoutedEventHandler(mi_Click);
				this.syncButton.DropDown.Items.Add(mi);
			}
		}

		void mi_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			NokiaPhone phone = (sender as MenuItem).Tag as NokiaPhone;
			if (phone == null) return;
			nokiaDiscoverer.nokiaProvider.Id = phone.SN;
			nokiaDiscoverer.nokiaProvider.Name = phone.Name;
			this.SyncNokia(this, new RoutedEventArgs());
		}

		public Assembly LoadLocalAssembly(string AssemblyName)
		{
			System.AppDomain domain = System.AppDomain.CurrentDomain;
			System.IO.StreamReader reader = new System.IO.StreamReader(AssemblyName, System.Text.Encoding.GetEncoding(1252), false);
			byte[] b = new byte[reader.BaseStream.Length];
			reader.BaseStream.Read(b, 0, System.Convert.ToInt32(reader.BaseStream.Length));
			domain.Load(b);
			System.Reflection.Assembly[] a = domain.GetAssemblies();
			int index = 0;
			for (int x = 0; x < a.Length; x++)
			{
				if (a[x].GetName().Name + ".dll" == System.IO.Path.GetFileName(AssemblyName))
				{
					index = x;
					return a[x];
				}
			}
			return null;
		}

		// GUI HANDLERS

		public MenuItem GetSyncMenuItemByName(string name)
		{
			foreach (MenuItem mi in syncButton.DropDown.Items)
			{
				if (mi.Name.Equals(name))
					return mi;
			}
			return null;
		}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
			syncEngine = new SyncEngine(this, false);

			this.lastSyncMessage.Content = "Choose your current provider.";
			this.lastSyncMessage.Visibility = Visibility.Visible;

			try
			{
				if (!IsNET35Installed())
					throw new System.Exception("Windows Live is unavailable. Install .NET 3.5 Framework.");

				Assembly a = LoadLocalAssembly(
					System.IO.Path.GetDirectoryName(
						Assembly.GetExecutingAssembly().Location) 
					+ "\\Synrc.WindowsLive.dll");
				
				Type t = a.GetType("Synrc.LiveContactsProvider");
				live = Activator.CreateInstance(t, new object[] { this, syncEngine }) as ISyncSource;

				//live = new LiveContactsProvider(this, syncEngine);
			}
			catch (Exception excp)
			{
				syncButton.DropDown.Items.Remove(GetSyncMenuItemByName("WindowsLive"));

				this.lastSyncMessage.Content = excp.Message;
				this.lastSyncMessage.Visibility = Visibility.Visible;
			}

			try
			{
				if (!IsOutlookInstalled())
				throw new System.Exception("Outlook not installed.");

				//Assembly a = LoadLocalAssembly("Synrc.Outlook.dll");
				//Type t = a.GetType("Synrc.OutlookContactsProvider");
				//outlook = Activator.CreateInstance(t, new object[] { this, syncEngine}) as ISyncSource;

				outlook = new OutlookContactsProvider(this, syncEngine);
			}
			catch
			{
				syncButton.DropDown.Items.Remove(GetSyncMenuItemByName("Outlook"));
			}

			vista = new WindowsContactsProvider(this, syncEngine);
			gmail = new GmailContactsProvider(this, syncEngine);
			

			if (IsNokiaInstalled())
			{
				nokia = new NokiaContactsProvider(this, syncEngine);
				nokiaDiscoverer = new NokiaDiscoverer(this, nokia as NokiaContactsProvider);
				this.syncButton.DropDown.Opened += new RoutedEventHandler(DropDown_Opened);
				nokiaDiscoverer.DiscoveryTask();
			}
			
			currentProvider = null;

			//searchContext = syncEngine.Sync(currentProvider, currentProvider);

			DataContext = searchContext;
        }
        private void _OnMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            this.DragMove();
        }
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        private void Window_LocationChanged(object sender, EventArgs e)
        {
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

		// SEARCH LOGIC

		public bool MatchSearch(IMan man, string search)
		{
			if ((man.EMail != null && man.EMail.ToLower().Contains(search)) ||
				(man.Phone != null && man.Phone.ToLower().Contains(search)) ||
				(man.FullName != null && man.FullName.ToLower().Contains(search)))
				return true;
			else return false;
		}
		public string GetSearchText()
		{
			return searchBox.Text;
		}
		public void SearchTask()
		{
			Dispatcher.Invoke(new VoidDelegate(SaveCursor), null);
			searchList.Clear();
			for (int i = 0; i < searchContext.Count; i++)
			{
				IMan man = searchContext[i];
				string s = Dispatcher.Invoke(new StringDelegate(GetSearchText), null).ToString();
				object o = Dispatcher.Invoke(new MatchDelegate(MatchSearch), new object[] { man, s.Trim().ToLower() });
				bool res = Convert.ToBoolean(o);
				if (res)
					searchList.Add(man);
			}
			Dispatcher.Invoke(new VoidDelegate(RestoreCursor), null);
		}
		private void SearchTextBox_Search(object sender, RoutedEventArgs e)
		{
			searchBox = sender as SearchTextBox;
			//Thread thread = new Thread(SearchTask);
			//thread.Start();
			SearchTask();
		}

		// CORE LOGIC

        private void SyncOutlookPIM(object sender, RoutedEventArgs e) {
			if (currentProvider == null)
				currentProvider = outlook;
			syncEngine.Sync(outlook, currentProvider);
        }

		private void SyncWindowsContacts(object sender, RoutedEventArgs e) {
			if (currentProvider == null)
				currentProvider = vista;
			syncEngine.Sync(vista, currentProvider);
		}

		private void SyncLiveContacts(object sender, RoutedEventArgs e) {
			if (currentProvider == null)
				currentProvider = live;
			syncEngine.Sync(live, currentProvider);
		}

		private void SyncGMAIL(object sender, RoutedEventArgs e) {
			if (currentProvider == null)
				currentProvider = gmail;
			syncEngine.Sync(gmail, currentProvider);
		}

		public  void SyncNokia(object sender, RoutedEventArgs e) {
			if (currentProvider == null)
				currentProvider = nokia;
			syncEngine.Sync(nokia, currentProvider);
		}

		private void expander1_Expanded(object sender, RoutedEventArgs e)
		{
			this.MaxHeight = cachedMaxHeight;
			this.Height = cachedHeight;
			//this.ContactsView.UpdateLayout();
		}

		double cachedHeight = 300;
		double cachedMaxHeight = 0;
		private void expander1_Collapsed(object sender, RoutedEventArgs e)
		{
			cachedHeight = this.Height;
			this.Height = 0;
			cachedMaxHeight = this.MaxHeight;
			this.MaxHeight = 105;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (syncEngine != null)
			syncEngine.CleanupResources();
		}

    }
}

