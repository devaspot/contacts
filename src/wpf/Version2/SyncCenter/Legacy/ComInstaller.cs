//-------------------------------------------------------------------------- 
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: ComInstaller.cs
//			
//  Description: Registers the CLSID for the FileSyncHandler so that it can
//    be invoked as a COM object from SyncMgr.  Callable from installers.
//
//-------------------------------------------------------------------------- 

using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Runtime.InteropServices;

namespace Synrc
{
	/// <summary>
	/// Registers the CLSID for the FileSyncHandler so that it can be invoked as 
	/// a COM object from SyncMgr.  Callable from installers.
	/// </summary>
	[RunInstaller(true)]
	public class ComInstaller : System.Configuration.Install.Installer
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Initializes a new instance of the ComInstaller class.
		/// </summary>
		public ComInstaller()
		{
			// This call is required by the Designer.
			InitializeComponent();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Registers the CLSID for the FileSyncHandler so that it can be invoked as 
		/// a COM object from SyncMgr.  Callable from installers.
		/// </summary>
		/// <param name="stateSaver">An IDictionary used to save information needed to perform a commit, rollback, or uninstall operation.</param>
		/// <exception cref="System.Configuration.Install.InstallException">Thrown when the assembly can't be registered.</exception>
		public override void Install(IDictionary stateSaver)
		{
			System.Diagnostics.Trace.WriteLine("Starting Install");
			base.Install (stateSaver);
		
			RegistrationServices regsrv = new RegistrationServices();
			if (!regsrv.RegisterAssembly(this.GetType().Assembly,
				AssemblyRegistrationFlags.SetCodeBase))
			{
				System.Diagnostics.Trace.WriteLine("Install failed");
				throw new InstallException("Failed To Register for COM");
			}
			System.Diagnostics.Trace.WriteLine("Install succeeded");
		}

		/// <summary>
		/// Unregisters the CLSID for the FileSyncHandler.  Callable from installers.
		/// </summary>
		/// <param name="savedState">An IDictionary that contains the state of the computer after the installation was complete.</param>
		/// <exception cref="System.Configuration.Install.InstallException">Thrown when the assembly can't be unregistered.</exception>
		public override void Uninstall(IDictionary savedState)
		{
			System.Diagnostics.Trace.WriteLine("Starting Uninstall");
			base.Uninstall (savedState);
		
			RegistrationServices regsrv = new RegistrationServices();
			if (!regsrv.UnregisterAssembly(this.GetType().Assembly))
			{
				throw new InstallException("Failed To Unregister for COM");
			}
			System.Diagnostics.Trace.WriteLine("Unnstall succeeded");
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
	}
}
