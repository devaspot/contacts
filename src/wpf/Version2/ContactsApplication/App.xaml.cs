//
//	Copyright (c) 2009 Synrc Research Center
//

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.VisualBasic.ApplicationServices;

namespace Synrc
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		public void HandleCommandLine(System.Collections.ObjectModel.ReadOnlyCollection<string> e)
		{
			//Code to handle command line arguments from other instances goes here

		}
    }

	//The old-style application wrapper
	public class SingleInstanceAppWrapper : WindowsFormsApplicationBase
	{
		public SingleInstanceAppWrapper()
		{
			// Enable single-instance mode.
			this.IsSingleInstance = true;
		}

		// Create the WPF application class.
		private App _app;

		//Override OnStartup() method to create the WPF application object
		protected override bool OnStartup(
			 Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e)
		{
			_app = new App();

			_app.Run();

			return false;

		}

		// Override OnStartupNextInstance() to handle multiple application instances.
		protected override void OnStartupNextInstance(
			 Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs e)
		{
			//In case of command line arguments, send them to the WPF application object    
			if (e.CommandLine.Count > 0)
			{
				_app.HandleCommandLine(e.CommandLine);
			}


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
