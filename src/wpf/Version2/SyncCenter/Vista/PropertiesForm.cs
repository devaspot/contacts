//-------------------------------------------------------------------------- 
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: PropertiesForm.cs
//			
//  Description: Supports editing of the FileSync properties.
//
//-------------------------------------------------------------------------- 

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Synrc
{
	/// <summary>
	/// Supports editing of the FileSync properties.
	/// </summary>
	public class PropertiesForm : System.Windows.Forms.Form
	{
		/// <summary>
		/// The folder that files will be synchronized to.
		/// </summary>
		public string ClientFolder;
		
		/// <summary>
		/// The folder from which files will be synchronized.
		/// </summary>
		public string ServerFolder;

		/// <summary>
		/// The name of the synchronization handler.
		/// </summary>
		public string SyncHandlerName;

		/// <summary>
		/// The name of the synchronization item.
		/// </summary>
		public string SyncItemName;

		/// <summary>
		/// The comment that will be added to the Registry entry.
		/// </summary>
		public string RegistryComment;
		
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox serverFolderTextBox;
		private System.Windows.Forms.Button ServerFolderBrowseButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button clientFolderBrowseButton;
		private System.Windows.Forms.TextBox clientFolderTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox syncHandlerNameTextBox;
		private System.Windows.Forms.TextBox syncItemNameTextBox;
		private System.Windows.Forms.TextBox registryCommentTextBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Allows the user to edit the properties of the file synchronization instance.
		/// </summary>
		public PropertiesForm()
		{
			//
			// Required for Windows Form Designer support.
			//
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.serverFolderTextBox = new System.Windows.Forms.TextBox();
			this.ServerFolderBrowseButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.clientFolderBrowseButton = new System.Windows.Forms.Button();
			this.clientFolderTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.syncHandlerNameTextBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.syncItemNameTextBox = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.registryCommentTextBox = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.TabIndex = 0;
			this.label1.Text = "Server Folder:";
			// 
			// serverFolderTextBox
			// 
			this.serverFolderTextBox.Location = new System.Drawing.Point(16, 32);
			this.serverFolderTextBox.Name = "serverFolderTextBox";
			this.serverFolderTextBox.Size = new System.Drawing.Size(464, 20);
			this.serverFolderTextBox.TabIndex = 1;
			this.serverFolderTextBox.Text = "";
			// 
			// ServerFolderBrowseButton
			// 
			this.ServerFolderBrowseButton.Location = new System.Drawing.Point(496, 32);
			this.ServerFolderBrowseButton.Name = "ServerFolderBrowseButton";
			this.ServerFolderBrowseButton.TabIndex = 2;
			this.ServerFolderBrowseButton.Text = "Browse...";
			this.ServerFolderBrowseButton.Click += new System.EventHandler(this.ServerFolderBrowseButton_Click);
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(215, 312);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 12;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(303, 312);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 13;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// clientFolderBrowseButton
			// 
			this.clientFolderBrowseButton.Location = new System.Drawing.Point(496, 88);
			this.clientFolderBrowseButton.Name = "clientFolderBrowseButton";
			this.clientFolderBrowseButton.TabIndex = 5;
			this.clientFolderBrowseButton.Text = "Browse...";
			this.clientFolderBrowseButton.Click += new System.EventHandler(this.clientFolderBrowseButton_Click);
			// 
			// clientFolderTextBox
			// 
			this.clientFolderTextBox.Location = new System.Drawing.Point(16, 88);
			this.clientFolderTextBox.Name = "clientFolderTextBox";
			this.clientFolderTextBox.Size = new System.Drawing.Size(464, 20);
			this.clientFolderTextBox.TabIndex = 4;
			this.clientFolderTextBox.Text = "";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 72);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(136, 23);
			this.label2.TabIndex = 3;
			this.label2.Text = "Client Folder:";
			// 
			// syncHandlerNameTextBox
			// 
			this.syncHandlerNameTextBox.Location = new System.Drawing.Point(16, 144);
			this.syncHandlerNameTextBox.Name = "syncHandlerNameTextBox";
			this.syncHandlerNameTextBox.Size = new System.Drawing.Size(552, 20);
			this.syncHandlerNameTextBox.TabIndex = 7;
			this.syncHandlerNameTextBox.Text = "";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 128);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(136, 23);
			this.label3.TabIndex = 6;
			this.label3.Text = "Sync Handler Name:";
			// 
			// syncItemNameTextBox
			// 
			this.syncItemNameTextBox.Location = new System.Drawing.Point(16, 200);
			this.syncItemNameTextBox.Name = "syncItemNameTextBox";
			this.syncItemNameTextBox.Size = new System.Drawing.Size(552, 20);
			this.syncItemNameTextBox.TabIndex = 9;
			this.syncItemNameTextBox.Text = "";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 184);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(136, 23);
			this.label4.TabIndex = 8;
			this.label4.Text = "Sync Item Name:";
			// 
			// registryCommentTextBox
			// 
			this.registryCommentTextBox.Location = new System.Drawing.Point(16, 256);
			this.registryCommentTextBox.Name = "registryCommentTextBox";
			this.registryCommentTextBox.Size = new System.Drawing.Size(552, 20);
			this.registryCommentTextBox.TabIndex = 11;
			this.registryCommentTextBox.Text = "";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 240);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(136, 23);
			this.label5.TabIndex = 10;
			this.label5.Text = "Registry Comment:";
			// 
			// PropertiesForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(592, 358);
			this.ControlBox = false;
			this.Controls.Add(this.registryCommentTextBox);
			this.Controls.Add(this.syncItemNameTextBox);
			this.Controls.Add(this.syncHandlerNameTextBox);
			this.Controls.Add(this.clientFolderTextBox);
			this.Controls.Add(this.serverFolderTextBox);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.clientFolderBrowseButton);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.ServerFolderBrowseButton);
			this.Controls.Add(this.label1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PropertiesForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "FileSync Properties";
			this.Load += new System.EventHandler(this.PropertiesForm_Load);
			this.ResumeLayout(false);
		}
		#endregion

		//
		// ServerFolderBrowseButton_Click
		//
		// Locate the server folder path.
		//
		// Parameters:
		//  sender - The source Button object for this event.
		//  e - The EventArgs object that contains the event data.
		//
		private void ServerFolderBrowseButton_Click(object sender, System.EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			fbd.SelectedPath = ServerFolder;
			DialogResult result = fbd.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				serverFolderTextBox.Text = fbd.SelectedPath;
			}
		}

		//
		// clientFolderBrowseButton_Click
		//
		// Locate the client folder path.
		//
		// Parameters:
		//  sender - The source Button object for this event.
		//  e - The EventArgs object that contains the event data.
		//
		private void clientFolderBrowseButton_Click(object sender, System.EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			fbd.SelectedPath = ClientFolder;
			DialogResult result = fbd.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				clientFolderTextBox.Text = fbd.SelectedPath;
			}
		}

		//
		// okButton_Click
		//
		// Close the form.
		//
		// Parameters:
		//  sender - The source Button object for this event.
		//  e - The EventArgs object that contains the event data.
		//
		private void okButton_Click(object sender, System.EventArgs e)
		{
			// The entered directories are not tested for existence or content.
			// Add appropriate validation here as required.
			ServerFolder = serverFolderTextBox.Text;
			ClientFolder = clientFolderTextBox.Text;
			SyncHandlerName = syncHandlerNameTextBox.Text;
			SyncItemName = syncItemNameTextBox.Text;
			RegistryComment = registryCommentTextBox.Text;

			DialogResult = DialogResult.OK;
		}

		//
		// cancelButton_Click
		//
		// Cancel the form.
		//
		// Parameters:
		//  sender - The source Button object for this event.
		//  e - The EventArgs object that contains the event data.
		//
		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		//
		// PropertiesForm_Load
		//
		// Occurs before the form is displayed for the first time.
		//
		// Parameters:
		//  sender - The source Button object for this event.
		//  e - The EventArgs object that contains the event data.
		//
		private void PropertiesForm_Load(object sender, System.EventArgs e)
		{
			serverFolderTextBox.Text = ServerFolder;
			clientFolderTextBox.Text = ClientFolder;
			syncHandlerNameTextBox.Text = SyncHandlerName;
			syncItemNameTextBox.Text = SyncItemName;
			registryCommentTextBox.Text = RegistryComment;
		}
	}
}