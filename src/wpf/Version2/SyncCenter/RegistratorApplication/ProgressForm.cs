//-------------------------------------------------------------------------- 
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: ProgressForm.cs
//			
//  Description: Displays the progress of file synchronization.
//
//-------------------------------------------------------------------------- 

using System;
using System.Collections;
using System.ComponentModel;

namespace Synrc
{
	/// <summary>
	/// Displays the progress of file synchronization.
	/// </summary>
	public class ProgressForm : System.Windows.Forms.Form
	{
		/// <summary>
		/// Sets a number indicating the relative progress of the synchronization.
		/// </summary>
		public int ProgressValue
		{
			set
			{
				progressValue = value;
				progressValueLabel.Text = progressValue + " of " + maxValue + " files";
				statusProgressBar.Value = value;
				Update();
			}
		}
		private int progressValue = 0;

		/// <summary>
		/// Sets he maximum value for ProgressValue.
		/// </summary>
		public int MaxValue
		{
			set
			{
				maxValue = value;
				statusProgressBar.Maximum = value;
			}
		}
		private int maxValue = 0;

		/// <summary>
		/// Sets the state of the synchronization.
		/// </summary>
		public SyncState SyncState
		{
			set
			{
				statusTypeLabel.Text = value.ToString();
			}
		}

		/// <summary>
		/// Sets the text describing the current synchronization status.
		/// </summary>
		public string StatusText
		{
			set 
			{
				statusTextBox.Text = value;
				Update();
			}
		}

		/// <summary>
		/// Gets or sets text describing all of the errors that have occurred
		/// during the current synchronization.
		/// </summary>
		public string SyncErrors
		{
			get
			{
				return syncErrorsTextBox.Text;
			}
			set
			{
				syncErrorsTextBox.Text = value;
			}
		}

		/// <summary>
		/// Gets the cancel flag; true if the user has clicked Cancel.
		/// </summary>
		public bool Cancel
		{
			get
			{
				return cancel;
			}
		}
		private bool cancel = false;

		/// <summary>
		/// Signals this form that the synchronization has been completed. 
		/// The Cancel button is disabled and the OK button is enabled.
		/// </summary>
		public void SynchronizeCompleted()
		{
			cancelButton.Enabled = false;
			OkButton.Enabled = true;
		}

		private System.Windows.Forms.TextBox statusTextBox;
		private System.Windows.Forms.Label progressValueLabel;
		private System.Windows.Forms.Label statusTypeLabel;
		private System.Windows.Forms.ProgressBar statusProgressBar;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button OkButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox syncErrorsTextBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Displays the progress of the file synchronization.
		/// </summary>
		public ProgressForm()
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
			this.statusTextBox = new System.Windows.Forms.TextBox();
			this.progressValueLabel = new System.Windows.Forms.Label();
			this.statusTypeLabel = new System.Windows.Forms.Label();
			this.statusProgressBar = new System.Windows.Forms.ProgressBar();
			this.cancelButton = new System.Windows.Forms.Button();
			this.OkButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.syncErrorsTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// statusTextBox
			// 
			this.statusTextBox.Location = new System.Drawing.Point(16, 48);
			this.statusTextBox.Name = "statusTextBox";
			this.statusTextBox.ReadOnly = true;
			this.statusTextBox.Size = new System.Drawing.Size(560, 20);
			this.statusTextBox.TabIndex = 1;
			this.statusTextBox.Text = "";
			// 
			// progressValueLabel
			// 
			this.progressValueLabel.Location = new System.Drawing.Point(16, 88);
			this.progressValueLabel.Name = "progressValueLabel";
			this.progressValueLabel.Size = new System.Drawing.Size(464, 23);
			this.progressValueLabel.TabIndex = 2;
			// 
			// statusTypeLabel
			// 
			this.statusTypeLabel.Location = new System.Drawing.Point(16, 16);
			this.statusTypeLabel.Name = "statusTypeLabel";
			this.statusTypeLabel.Size = new System.Drawing.Size(560, 23);
			this.statusTypeLabel.TabIndex = 0;
			// 
			// statusProgressBar
			// 
			this.statusProgressBar.Location = new System.Drawing.Point(16, 128);
			this.statusProgressBar.Name = "statusProgressBar";
			this.statusProgressBar.Size = new System.Drawing.Size(560, 23);
			this.statusProgressBar.TabIndex = 4;
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(501, 88);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// OkButton
			// 
			this.OkButton.Enabled = false;
			this.OkButton.Location = new System.Drawing.Point(501, 320);
			this.OkButton.Name = "OkButton";
			this.OkButton.TabIndex = 7;
			this.OkButton.Text = "Close";
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 168);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 5;
			this.label1.Text = "Errors:";
			// 
			// syncErrorsTextBox
			// 
			this.syncErrorsTextBox.AcceptsReturn = true;
			this.syncErrorsTextBox.Location = new System.Drawing.Point(16, 192);
			this.syncErrorsTextBox.Multiline = true;
			this.syncErrorsTextBox.Name = "syncErrorsTextBox";
			this.syncErrorsTextBox.ReadOnly = true;
			this.syncErrorsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.syncErrorsTextBox.Size = new System.Drawing.Size(560, 112);
			this.syncErrorsTextBox.TabIndex = 6;
			this.syncErrorsTextBox.Text = "";
			// 
			// ProgressForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(592, 358);
			this.ControlBox = false;
			this.Controls.Add(this.syncErrorsTextBox);
			this.Controls.Add(this.statusTextBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.statusProgressBar);
			this.Controls.Add(this.statusTypeLabel);
			this.Controls.Add(this.progressValueLabel);
			this.Name = "ProgressForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Sync Progress";
			this.ResumeLayout(false);

		}
		#endregion

		//
		// cancelButton_Click
		//
		// Cancels synchronization.
		//
		// Parameters:
		//  sender - The source Button object for this event.
		//  e - The EventArgs object that contains the event data.
		//
		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			cancel = true;
		}

		//
		// OkButton_Click
		//
		// Closes the form.
		//
		// Parameters:
		//  sender - The source Button object for this event.
		//  e - The EventArgs object that contains the event data.
		//
		private void OkButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}