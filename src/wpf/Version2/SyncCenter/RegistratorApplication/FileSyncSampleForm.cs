//-------------------------------------------------------------------------- 
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: FileSyncSampleForm.cs
//			
//  Description: Sample form for invoking methods in FileSync.
//
//-------------------------------------------------------------------------- 

using Microsoft.SyncCenter;
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Synrc;
using System.Runtime.InteropServices;

/// <summary>
/// Sample form for invoking methods in FileSync.
/// </summary>
public class InvokerForm : System.Windows.Forms.Form
{
	private ProgressForm progressForm = null;
	private System.Windows.Forms.Button registerFileSyncHandlerButton;
	private System.Windows.Forms.Button unregisterFileSyncHandlerButton;
	private System.Windows.Forms.Label hresultLabel;

	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	/// <summary>
	/// Initializes a new instance of the InvokerForm class.
	/// </summary>
	public InvokerForm()
	{
		//
		// Required for Windows Form Designer support.
		//
		InitializeComponent();
	}

	/// <summary>
	/// Disposes of the resources (other than memory) used by the form.
	/// </summary>
	/// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected override void Dispose( bool disposing )
	{
		if( disposing )
		{
			if (components != null) 
			{
				components.Dispose();
			}
		}
		base.Dispose( disposing );
	}

	#region Windows Form Designer generated code
	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		this.registerFileSyncHandlerButton = new System.Windows.Forms.Button();
		this.unregisterFileSyncHandlerButton = new System.Windows.Forms.Button();
		this.hresultLabel = new System.Windows.Forms.Label();
		this.SuspendLayout();
		// 
		// registerFileSyncHandlerButton
		// 
		this.registerFileSyncHandlerButton.Location = new System.Drawing.Point(88, 12);
		this.registerFileSyncHandlerButton.Name = "registerFileSyncHandlerButton";
		this.registerFileSyncHandlerButton.Size = new System.Drawing.Size(219, 51);
		this.registerFileSyncHandlerButton.TabIndex = 3;
		this.registerFileSyncHandlerButton.Text = "Register in Windows Sync Center";
		this.registerFileSyncHandlerButton.Click += new System.EventHandler(this.registerFileSyncHandlerButton_Click);
		// 
		// unregisterFileSyncHandlerButton
		// 
		this.unregisterFileSyncHandlerButton.Location = new System.Drawing.Point(88, 69);
		this.unregisterFileSyncHandlerButton.Name = "unregisterFileSyncHandlerButton";
		this.unregisterFileSyncHandlerButton.Size = new System.Drawing.Size(219, 50);
		this.unregisterFileSyncHandlerButton.TabIndex = 4;
		this.unregisterFileSyncHandlerButton.Text = "Unregister in Windows Sync Center";
		this.unregisterFileSyncHandlerButton.Click += new System.EventHandler(this.unregisterFileSyncHandlerButton_Click);
		// 
		// hresultLabel
		// 
		this.hresultLabel.Location = new System.Drawing.Point(248, 176);
		this.hresultLabel.Name = "hresultLabel";
		this.hresultLabel.Size = new System.Drawing.Size(200, 23);
		this.hresultLabel.TabIndex = 11;
		this.hresultLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// InvokerForm
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(319, 133);
		this.Controls.Add(this.hresultLabel);
		this.Controls.Add(this.unregisterFileSyncHandlerButton);
		this.Controls.Add(this.registerFileSyncHandlerButton);
		this.Name = "InvokerForm";
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Synrc Sync Contacts";
		this.Load += new System.EventHandler(this.InvokerForm_Load);
		this.ResumeLayout(false);

	}
	#endregion

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main() 
	{
		Application.Run(new InvokerForm());
	}

    //
	// registerFileSyncHandlerButton_Click
    //
    // Registers FileSyncHandler with SyncMgr.
    //
    // Parameters:
    //  sender - The source Button object for this event.
    //  e - The EventArgs object that contains the event data.
    //
	private void registerFileSyncHandlerButton_Click(object sender, System.EventArgs e)
	{
		try
		{
			Guid syncmgrClsid = new Guid("6295DF27-35EE-11D1-8707-00C04FD93327");
			Type syncmgrType = Type.GetTypeFromCLSID(syncmgrClsid);
			ISyncMgrRegister smr = (ISyncMgrRegister)Activator.CreateInstance(syncmgrType);

			Guid fileSyncHandlerId = //SynrcSyncMgrHandlerCollection.SyncHandlerCollectionId;
			SynrcSyncMgrHandler.SyncHandlerId;
				//new Guid("CE789B61-EC8A-499f-9977-2BA2593EEC60"); 
			//FileSyncConfig config = FileSyncConfig.GetConfig();
			int hresult = smr.RegisterSyncMgrHandler(ref fileSyncHandlerId, "2334", 0);
			if (hresult != 0)
			{
				throw new Exception("Failed to register with HRESULT = " + hresult);
			}

			//SyncCenter.RegisterWithSyncMgr();
		}
		catch (Exception ex)
		{
			MessageBox.Show("Exception from registering: " + ex.Message);
		}
	}

    //
    // unregisterFileSyncHandlerButton_Click
    //
    // Unregisters FileSyncHandler with SyncMgr.
    //
    // Parameters:
    //  sender - The source Button object for this event.
    //  e - The EventArgs object that contains the event data.
    //
	private void unregisterFileSyncHandlerButton_Click(object sender, System.EventArgs e)
	{
		try
		{
			Guid syncmgrClsid = new Guid("6295DF27-35EE-11D1-8707-00C04FD93327");
			Type syncmgrType = Type.GetTypeFromCLSID(syncmgrClsid);
			ISyncMgrRegister smr = (ISyncMgrRegister)Activator.CreateInstance(syncmgrType);

			Guid fileSyncHandlerId = //SynrcSyncMgrHandlerCollection.SyncHandlerCollectionId;
			SynrcSyncMgrHandler.SyncHandlerId;
				//new Guid("CE789B61-EC8A-499f-9977-2BA2593EEC60");// 
			int hresult = smr.UnregisterSyncMgrHandler(ref fileSyncHandlerId, 0);
			if (hresult != 0)
			{
				throw new Exception("Failed to register with HRESULT = " + hresult);
			}

			//SyncCenter.UnRegisterWithSyncMgr();
		}
		catch (Exception ex)
		{
			MessageBox.Show("Exception from unregistering: " + ex.Message);
		}
	}

    //
    // invokeFileSyncHandlerFromComButton_Click
    //
    // Perform a file synchronization using Synchronization Manager.
    //
    // Parameters:
    //  sender - The source Button object for this event.
    //  e - The EventArgs object that contains the event data.
    //
	private void invokeFileSyncHandlerFromComButton_Click(object sender, System.EventArgs e)
	{
		try
		{
			SynrcSync fileSync = new SynrcSync();
			fileSync.SyncThroughSyncMgr();
			//fileSync.InvokeDirectly = false;
			//fileSync.Sync();
		}
		catch (Exception ex)
		{
			MessageBox.Show("Exception from invocation: " + ex.Message);
		}
	}

	//
	// invokeFileSyncHandlerDirectlyButton_Click
	//
	// Perform a file synchronization directly.
	//
	// Parameters:
	//  sender - The source Button object for this event.
	//  e - The EventArgs object that contains the event data.
	//
	private void invokeFileSyncHandlerDirectlyButton_Click(object sender, System.EventArgs e)
	{
		hresultLabel.Text = "";
		try
		{
			SynrcSync fileSync = new SynrcSync();
			//fileSync.InvokeDirectly = true;
			fileSync.syncStatus += new SynrcSync.SyncStatusDelegate(OnFileSyncStatus);
			fileSync.syncError += new SynrcSync.SyncErrorDelegate(OnSyncError);
			progressForm = new ProgressForm();
			progressForm.Show();
			int retval = 0;// fileSync.Sync();
			hresultLabel.Text = "HRESULT is 0x" + retval.ToString("x");
		}
		catch (Exception ex)
		{
			MessageBox.Show("Exception from invocation: " + ex.Message);
			if (progressForm != null)
			{
				progressForm.Close();
			}
		}
	}

    //
    // showSyncMgrGuiButton_Click
    //
    // Show the Synchronization Manager UI.
    //
    // Parameters:
	//  sender - The source Button object for this event.
	//  e - The EventArgs object that contains the event data.
	//
	private void showSyncMgrGuiButton_Click(object sender, System.EventArgs e)
	{
		try
		{
			SynrcSync.ShowSyncMgrGui();
		}
		catch (Exception ex)
		{
			MessageBox.Show("Exception from invocation: " + ex.Message);
		}
	}

    //
    // propertiesButton_Click
    //
    // Display the FileSync Properties dialog.
    //
    // Parameters:
	//  sender - The source Button object for this event.
	//  e - The EventArgs object that contains the event data.
	//
	private void propertiesButton_Click(object sender, System.EventArgs e)
	{
		try
		{
			GooglePropertiesForm propertiesForm = new GooglePropertiesForm();
			SynrcSync fileSync = new SynrcSync();
			//propertiesForm.ClientFolder = fileSync.ClientFolder;
			//propertiesForm.ServerFolder = fileSync.ServerFolder;
			//propertiesForm.SyncHandlerName = fileSync.SyncHandlerName;
			//propertiesForm.SyncItemName = fileSync.SyncItemName;
			//propertiesForm.RegistryComment = fileSync.RegistryComment;
			
		
			if (DialogResult.OK == propertiesForm.ShowDialog(this))
			{
				//fileSync.ClientFolder = propertiesForm.ClientFolder;
				//fileSync.ServerFolder = propertiesForm.ServerFolder;
				//fileSync.SyncHandlerName = propertiesForm.SyncHandlerName;
				//fileSync.SyncItemName = propertiesForm.SyncItemName;
				//fileSync.RegistryComment = propertiesForm.RegistryComment;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Exception from properties dialog: " + ex.Message);
		}

	}

	//
    // OnFileSyncStatus
    //
    // Occurs when new status is available.
    //
    // Parameters:
	//  state - A SyncState value.
	//  text - The status description.
	//  progressValue - An integer that indicates the progress value.
	//  maxValue - An integer that indicates the maximum progress value.
	//  cancelUpdate - A ref value that allows the form to tell FileSync to cancel the synchronization.
    //
	private void OnFileSyncStatus(SyncState state, string text, int progressValue, int maxValue, ref bool cancelUpdate)
	{
		// Allow the Cancel button click to be serviced.
		Application.DoEvents();  

		if (progressForm != null)
		{
			progressForm.SyncState = state;
			progressForm.StatusText = text;
			progressForm.MaxValue = maxValue;
			progressForm.ProgressValue = progressValue;
			cancelUpdate = progressForm.Cancel;

			if (state != SyncState.Updating)
			{
				progressForm.SynchronizeCompleted();
			}
		}
	}

    //
    // OnSyncError
    //
    // Occurs when a synchronization error is detected.
    //
    // Parameters:
    //  level - A SyncErrorLevel value.
    //  description - The error description.
    //
	private void OnSyncError(SyncErrorLevel level, string description)
	{
		if (progressForm != null)
		{
			progressForm.SyncErrors += level.ToString() + ": "
				+ description + System.Environment.NewLine + System.Environment.NewLine;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SHFILEINFO
	{
		public const int NAMESIZE = 80;
		public IntPtr hIcon;
		public int iIcon;
		public uint dwAttributes;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string szDisplayName;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
		public string szTypeName;
	};

	public const uint SHGFI_ICON = 0x000000100;     // get icon
	public const uint SHGFI_DISPLAYNAME = 0x000000200;     // get display name
	public const uint SHGFI_TYPENAME = 0x000000400;     // get type name
	public const uint SHGFI_ATTRIBUTES = 0x000000800;     // get attributes
	public const uint SHGFI_ICONLOCATION = 0x000001000;     // get icon location
	public const uint SHGFI_EXETYPE = 0x000002000;     // return exe type
	public const uint SHGFI_SYSICONINDEX = 0x000004000;     // get system icon index
	public const uint SHGFI_LINKOVERLAY = 0x000008000;     // put a link overlay on icon
	public const uint SHGFI_SELECTED = 0x000010000;     // show icon in selected state
	public const uint SHGFI_ATTR_SPECIFIED = 0x000020000;     // get only specified attributes
	public const uint SHGFI_LARGEICON = 0x000000000;     // get large icon
	public const uint SHGFI_SMALLICON = 0x000000001;     // get small icon
	public const uint SHGFI_OPENICON = 0x000000002;     // get open icon
	public const uint SHGFI_SHELLICONSIZE = 0x000000004;     // get shell size icon
	public const uint SHGFI_PIDL = 0x000000008;     // pszPath is a pidl
	public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;     // use passed dwFileAttribute
	public const uint SHGFI_ADDOVERLAYS = 0x000000020;     // apply the appropriate overlays
	public const uint SHGFI_OVERLAYINDEX = 0x000000040;     // Get the index of the overlay

	public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
	public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

	[DllImport("User32.dll")]
	public static extern int DestroyIcon(IntPtr hIcon);

	[DllImport("Shell32.dll")]
	public static extern IntPtr SHGetFileInfo(
		string pszPath,
		uint dwFileAttributes,
		ref SHFILEINFO psfi,
		uint cbFileInfo,
		uint uFlags
		);

	private void InvokerForm_Load(object sender, EventArgs e)
	{

	}
}