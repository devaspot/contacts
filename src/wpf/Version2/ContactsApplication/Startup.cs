//
//	Copyright (c) 2009 Synrc Research Center
// 

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.VisualBasic.ApplicationServices;
using System.Deployment.Application;
using System.Windows.Forms;

using MessageBox = System.Windows.MessageBox;
using System.ComponentModel;
using System.Reflection;

namespace Synrc
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public class WpfApp : System.Windows.Application
	{
		bool useXPskin = false;
		bool noneProvider = false;
		string providerSpecified = null;

		public void ProcessArgs(System.Windows.StartupEventArgs e)
		{
			foreach (string s in e.Args)
			{
				if (s.ToLower().Equals("xp"))
				{
					useXPskin = true;
				}
				else if (s.ToLower().Equals("none"))
				{
					noneProvider = true;
				}
				else if (s.ToLower().Equals("gmail")
					|| s.ToLower().Equals("live")
					|| s.ToLower().Equals("windows")
					|| s.ToLower().Equals("outlook"))
				{
					providerSpecified = s;
				}
			}

			if (Environment.OSVersion.Version.Major < 6)
				useXPskin = true;

			if (useXPskin)
				MainWindow = new XPWindow(providerSpecified, noneProvider);
			else
				MainWindow = new VistaWindow(providerSpecified, noneProvider);

		}

		protected override void OnStartup(System.Windows.StartupEventArgs e)
		{
			System.Windows.Application.Current.Resources.MergedDictionaries.Add(
			System.Windows.Application.LoadComponent(
			   new Uri("Synrc;component/ResourceDictionary.xaml",
			   UriKind.Relative)) as ResourceDictionary);

			System.Windows.Application.Current.Resources.Source =
				new Uri("/Synrc;component/ResourceDictionary.xaml", UriKind.Relative);
			
			base.OnStartup(e);

			ProcessArgs(e);

			if (MainWindow != null)
				MainWindow.Show();
		}

		public void ShowCommandLineHelp(System.Collections.ObjectModel.ReadOnlyCollection<string> e)
		{
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
			Version version = assembly.GetName().Version;

			MessageBox.Show(
				"Synrc (R) Contacts version " + version.ToString() + "\n" +
				"for Microsoft (R) Windows XP, Vista or Windows 7\n" +
				"Copyright (C) Synrc Research Center. All Rights Reserved.\n\n" +
				"The application cannot be runned twice.\n\n" +
				"Usage: syncm.exe [localprovider] \n" +
				"Examples:\n" +
				"\tsyncm.exe none\n" +
				"\tsyncm.exe gmail\n" +
				"\tsyncm.exe windows\n" +
				"\tsyncm.exe outlook\n" +
				"\tsyncm.exe nokia\n\n" +
				"The default local provider is Windows Contacts folder. Full list of possible providers can be found in documentation or support."
				,
				"Synrc Contacts"
			);


		}
	}

	public class SingleInstanceAppWrapper : WindowsFormsApplicationBase
	{
		private WpfApp _app;

		public SingleInstanceAppWrapper()
		{
			this.IsSingleInstance = true;
		}

		protected override bool OnStartup(
			 Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e)
		{
			_app = new WpfApp();
			_app.Run();
			return false;

		}

		protected override void OnStartupNextInstance(
			 Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs e)
		{
			base.OnStartupNextInstance(e);
			_app.MainWindow.Activate();
			_app.ShowCommandLineHelp(e.CommandLine);
		}
	}

	public class Startup
	{
		[STAThread]
		public static void Main(string[] args)
		{
			SingleInstanceAppWrapper wrapper = new SingleInstanceAppWrapper();
			wrapper.Run(args);

		}
	}
}
