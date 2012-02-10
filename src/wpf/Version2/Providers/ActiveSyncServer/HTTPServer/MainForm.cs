using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;

using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using Synrc.HTTPServer;

namespace Synrc
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox HTTP_PORT;
		private System.Windows.Forms.Button Start;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Button Resume;
		private System.Windows.Forms.Button Stop;
		private System.Windows.Forms.Button Suspend;
		private System.Windows.Forms.ErrorProvider Error;
		private System.Windows.Forms.TextBox HTTP_FOLDER;
		private System.Windows.Forms.Label LHTTP_FOLDER;
		private System.Windows.Forms.Label LHTTP_PORT;
		private System.Windows.Forms.Button Browse;
		private System.Windows.Forms.FolderBrowserDialog FolderBrowser;
		private System.Windows.Forms.NotifyIcon notifyIcon;
		private System.Windows.Forms.ContextMenu contextMenu;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem menuItem8;
		private System.Windows.Forms.CheckBox startServing;
		private System.Windows.Forms.CheckBox startMinimized;

		//
		CsHTTPServer HTTPServer;
		int eX, eY;
		bool fisrtView = true;
		private System.Windows.Forms.CheckBox startOnWindows;

		//
		private Configuration config = new Configuration(Path.GetDirectoryName(Application.ExecutablePath));
		

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.HTTP_PORT = new System.Windows.Forms.TextBox();
			this.Start = new System.Windows.Forms.Button();
			this.Resume = new System.Windows.Forms.Button();
			this.Stop = new System.Windows.Forms.Button();
			this.Suspend = new System.Windows.Forms.Button();
			this.Error = new System.Windows.Forms.ErrorProvider(this.components);
			this.HTTP_FOLDER = new System.Windows.Forms.TextBox();
			this.LHTTP_FOLDER = new System.Windows.Forms.Label();
			this.LHTTP_PORT = new System.Windows.Forms.Label();
			this.Browse = new System.Windows.Forms.Button();
			this.FolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
			this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenu = new System.Windows.Forms.ContextMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.menuItem6 = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.menuItem8 = new System.Windows.Forms.MenuItem();
			this.startServing = new System.Windows.Forms.CheckBox();
			this.startMinimized = new System.Windows.Forms.CheckBox();
			this.startOnWindows = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.Error)).BeginInit();
			this.SuspendLayout();
			// 
			// HTTP_PORT
			// 
			this.HTTP_PORT.Cursor = System.Windows.Forms.Cursors.Default;
			this.HTTP_PORT.Location = new System.Drawing.Point(64, 48);
			this.HTTP_PORT.Name = "HTTP_PORT";
			this.HTTP_PORT.Size = new System.Drawing.Size(210, 20);
			this.HTTP_PORT.TabIndex = 0;
			this.HTTP_PORT.Text = "80";
			this.HTTP_PORT.Validated += new System.EventHandler(this.HTTP_PORT_Validated);
			this.HTTP_PORT.Validating += new System.ComponentModel.CancelEventHandler(this.HTTP_PORT_Validating);
			// 
			// Start
			// 
			this.Start.BackColor = System.Drawing.Color.Transparent;
			this.Start.Location = new System.Drawing.Point(16, 80);
			this.Start.Name = "Start";
			this.Start.Size = new System.Drawing.Size(64, 23);
			this.Start.TabIndex = 1;
			this.Start.Text = "Start";
			this.Start.UseVisualStyleBackColor = false;
			this.Start.Click += new System.EventHandler(this.Start_Click);
			// 
			// Resume
			// 
			this.Resume.BackColor = System.Drawing.Color.Transparent;
			this.Resume.Enabled = false;
			this.Resume.Location = new System.Drawing.Point(144, 80);
			this.Resume.Name = "Resume";
			this.Resume.Size = new System.Drawing.Size(64, 23);
			this.Resume.TabIndex = 3;
			this.Resume.Text = "Resume";
			this.Resume.UseVisualStyleBackColor = false;
			this.Resume.Click += new System.EventHandler(this.Resume_Click);
			// 
			// Stop
			// 
			this.Stop.BackColor = System.Drawing.Color.Transparent;
			this.Stop.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Stop.Enabled = false;
			this.Stop.Location = new System.Drawing.Point(208, 80);
			this.Stop.Name = "Stop";
			this.Stop.Size = new System.Drawing.Size(64, 23);
			this.Stop.TabIndex = 4;
			this.Stop.Text = "Stop";
			this.Stop.UseVisualStyleBackColor = false;
			this.Stop.Click += new System.EventHandler(this.Stop_Click);
			// 
			// Suspend
			// 
			this.Suspend.BackColor = System.Drawing.Color.Transparent;
			this.Suspend.Enabled = false;
			this.Suspend.Location = new System.Drawing.Point(80, 80);
			this.Suspend.Name = "Suspend";
			this.Suspend.Size = new System.Drawing.Size(64, 23);
			this.Suspend.TabIndex = 2;
			this.Suspend.Text = "Suspend";
			this.Suspend.UseVisualStyleBackColor = false;
			this.Suspend.Click += new System.EventHandler(this.Suspend_Click);
			// 
			// Error
			// 
			this.Error.ContainerControl = this;
			// 
			// HTTP_FOLDER
			// 
			this.HTTP_FOLDER.Location = new System.Drawing.Point(64, 24);
			this.HTTP_FOLDER.Name = "HTTP_FOLDER";
			this.HTTP_FOLDER.Size = new System.Drawing.Size(210, 20);
			this.HTTP_FOLDER.TabIndex = 5;
			this.HTTP_FOLDER.Text = "C:\\www";
			this.HTTP_FOLDER.Validated += new System.EventHandler(this.HTTP_FOLDER_Validated);
			this.HTTP_FOLDER.Validating += new System.ComponentModel.CancelEventHandler(this.HTTP_FOLDER_Validating);
			// 
			// LHTTP_FOLDER
			// 
			this.LHTTP_FOLDER.BackColor = System.Drawing.Color.Transparent;
			this.LHTTP_FOLDER.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.LHTTP_FOLDER.Location = new System.Drawing.Point(13, 24);
			this.LHTTP_FOLDER.Name = "LHTTP_FOLDER";
			this.LHTTP_FOLDER.Size = new System.Drawing.Size(50, 20);
			this.LHTTP_FOLDER.TabIndex = 6;
			this.LHTTP_FOLDER.Text = "Folder";
			this.LHTTP_FOLDER.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LHTTP_PORT
			// 
			this.LHTTP_PORT.BackColor = System.Drawing.Color.Transparent;
			this.LHTTP_PORT.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.LHTTP_PORT.Location = new System.Drawing.Point(13, 48);
			this.LHTTP_PORT.Name = "LHTTP_PORT";
			this.LHTTP_PORT.Size = new System.Drawing.Size(50, 20);
			this.LHTTP_PORT.TabIndex = 7;
			this.LHTTP_PORT.Text = "Port";
			this.LHTTP_PORT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// Browse
			// 
			this.Browse.BackColor = System.Drawing.Color.Transparent;
			this.Browse.Location = new System.Drawing.Point(280, 23);
			this.Browse.Name = "Browse";
			this.Browse.Size = new System.Drawing.Size(64, 23);
			this.Browse.TabIndex = 8;
			this.Browse.Text = "Browse";
			this.Browse.UseVisualStyleBackColor = false;
			this.Browse.Click += new System.EventHandler(this.Browse_Click);
			// 
			// notifyIcon
			// 
			this.notifyIcon.ContextMenu = this.contextMenu;
			this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
			this.notifyIcon.Text = "MyServer";
			this.notifyIcon.Visible = true;
			// 
			// contextMenu
			// 
			this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem2,
            this.menuItem3,
            this.menuItem4,
            this.menuItem5,
            this.menuItem6,
            this.menuItem7,
            this.menuItem8});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.Text = "&Show";
			this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 1;
			this.menuItem2.Text = "&Hide";
			this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 2;
			this.menuItem3.Text = "-";
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 3;
			this.menuItem4.Text = "S&tart";
			this.menuItem4.Click += new System.EventHandler(this.Start_Click);
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 4;
			this.menuItem5.Text = "Sto&p";
			this.menuItem5.Click += new System.EventHandler(this.Stop_Click);
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 5;
			this.menuItem6.Text = "S&uspend";
			this.menuItem6.Click += new System.EventHandler(this.Suspend_Click);
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 6;
			this.menuItem7.Text = "&Resume";
			this.menuItem7.Click += new System.EventHandler(this.Resume_Click);
			// 
			// menuItem8
			// 
			this.menuItem8.Index = 7;
			this.menuItem8.Text = "E&xit";
			// 
			// startServing
			// 
			this.startServing.BackColor = System.Drawing.Color.Transparent;
			this.startServing.Location = new System.Drawing.Point(22, 128);
			this.startServing.Name = "startServing";
			this.startServing.Size = new System.Drawing.Size(96, 25);
			this.startServing.TabIndex = 13;
			this.startServing.Text = "Start serving.";
			this.startServing.UseVisualStyleBackColor = false;
			// 
			// startMinimized
			// 
			this.startMinimized.BackColor = System.Drawing.Color.Transparent;
			this.startMinimized.Location = new System.Drawing.Point(110, 128);
			this.startMinimized.Name = "startMinimized";
			this.startMinimized.Size = new System.Drawing.Size(80, 25);
			this.startMinimized.TabIndex = 14;
			this.startMinimized.Text = "Minimized.";
			this.startMinimized.UseVisualStyleBackColor = false;
			// 
			// startOnWindows
			// 
			this.startOnWindows.BackColor = System.Drawing.Color.Transparent;
			this.startOnWindows.Location = new System.Drawing.Point(187, 128);
			this.startOnWindows.Name = "startOnWindows";
			this.startOnWindows.Size = new System.Drawing.Size(87, 25);
			this.startOnWindows.TabIndex = 16;
			this.startOnWindows.Text = "On Windows";
			this.startOnWindows.UseVisualStyleBackColor = false;
			this.startOnWindows.CheckedChanged += new System.EventHandler(this.startOnWindows_CheckedChanged);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(357, 170);
			this.ContextMenu = this.contextMenu;
			this.Controls.Add(this.startOnWindows);
			this.Controls.Add(this.startMinimized);
			this.Controls.Add(this.startServing);
			this.Controls.Add(this.Browse);
			this.Controls.Add(this.LHTTP_PORT);
			this.Controls.Add(this.LHTTP_FOLDER);
			this.Controls.Add(this.HTTP_FOLDER);
			this.Controls.Add(this.Stop);
			this.Controls.Add(this.Resume);
			this.Controls.Add(this.Suspend);
			this.Controls.Add(this.Start);
			this.Controls.Add(this.HTTP_PORT);
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.KeyPreview = true;
			this.Name = "Form1";
			this.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Text = "Exchange ActiveSync HTTP Server";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.Activated += new System.EventHandler(this.Form1_Activated);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseMove);
			((System.ComponentModel.ISupportInitialize)(this.Error)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void Start_Click(object sender, System.EventArgs e)
		{
			//
			HTTPServer = new MyServer(Convert.ToInt32(HTTP_PORT.Text),HTTP_FOLDER.Text);
			HTTPServer.Start();
			//
			HTTP_PORT.Enabled = false;
			HTTP_FOLDER.Enabled = false;
			Browse.Enabled = false;
			Start.Enabled = false;
			Suspend.Enabled = true;
			Stop.Enabled = true;
			//
			contextMenu.MenuItems[3].Enabled = false;
			contextMenu.MenuItems[4].Enabled = true;
			contextMenu.MenuItems[5].Enabled = true;
		}



		private void Resume_Click(object sender, System.EventArgs e)
		{
			//
			HTTPServer.Resume();
			//
			Suspend.Enabled = true;
			Resume.Enabled = false;
			//
			contextMenu.MenuItems[5].Enabled = true;
			contextMenu.MenuItems[6].Enabled = false;
		}

		private void Stop_Click(object sender, System.EventArgs e)
		{
			//
			HTTPServer.Stop();
			//
			HTTP_PORT.Enabled = true;
			HTTP_FOLDER.Enabled = true;
			Browse.Enabled = true;
			Suspend.Enabled = false;
			Resume.Enabled = false;
			Stop.Enabled = false;
			Start.Enabled = true;
			//
			contextMenu.MenuItems[5].Enabled = false;
			contextMenu.MenuItems[6].Enabled = false;
			contextMenu.MenuItems[4].Enabled = false;
			contextMenu.MenuItems[3].Enabled = true;
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
			// load configuration
			if (config.LoadSettings())
			{
				// set window location and size
				this.Location = config.mainWindowLocation;

				//
				if (config.startServing)
					Start_Click(this,e);
				//
				startServing.Checked = config.startServing;
				startMinimized.Checked = config.startMinimized;
				startOnWindows.Checked = config.startOnWindows;
			}
		}



		private void HTTP_PORT_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			string errorMsg;
			if(!ValidPort(HTTP_PORT.Text, out errorMsg))
			{
				// Cancel the event and select the text to be corrected by the user.
				e.Cancel = true;
				HTTP_PORT.Select(0, HTTP_PORT.Text.Length);

				// Set the ErrorProvider error with the text to display. 
				this.Error.SetError(HTTP_PORT, errorMsg);
			}
		}

		private void HTTP_PORT_Validated(object sender, System.EventArgs e)
		{
			// If all conditions have been met, clear the ErrorProvider of errors.
			Error.SetError(HTTP_PORT, "");
		}

		public bool ValidPort(string Port, out string errorMessage)
		{
			// Confirm that the Port string is not empty.
			if(Port.Length == 0)
			{
				errorMessage = "Port number is required.";
				return false;
			}

			int Value;
			// Confirm that it is number
			try 
			{
				Value = Int32.Parse(Port);
			}
			catch (FormatException)
			{
				errorMessage = "Port number must be 1 to 65535.\n" +
					"For example '8080' ";
				return false;
			}

			if ((Value > 0) && (Value < 65536))
			{
				errorMessage = "";
				return true;
			}
   
			errorMessage = "Port number must be 1 to 65535.\n" +
				"For example '8080' ";
			return false;
		}

		private void Browse_Click(object sender, System.EventArgs e)
		{
			if (FolderBrowser.ShowDialog() == DialogResult.OK)
                HTTP_FOLDER.Text = FolderBrowser.SelectedPath;
		}

		private void HTTP_FOLDER_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			string errorMsg = "Folder must be valid path.\n" +
				"For example 'c:\\www' ";
			if (!Directory.Exists(HTTP_FOLDER.Text))
			{
				// Cancel the event and select the text to be corrected by the user.
				e.Cancel = true;
				HTTP_PORT.Select(0, HTTP_FOLDER.Text.Length);

				// Set the ErrorProvider error with the text to display. 
				this.Error.SetError(HTTP_FOLDER, errorMsg);
			}

		}

		private void HTTP_FOLDER_Validated(object sender, System.EventArgs e)
		{
			// If all conditions have been met, clear the ErrorProvider of errors.
			Error.SetError(HTTP_FOLDER, "");
		}

		private void Form1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				this.Left += e.X - eX;
				this.Top += e.Y - eY;
				this.OnMove(e);
			}	
		}

		private void Form1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				eX = e.X;
				eY = e.Y;
			}
		}

		private void Suspend_Click(object sender, System.EventArgs e)
		{
			//
			HTTPServer.Suspend();
			//
			Resume.Enabled = true;
			Suspend.Enabled = false;
			//
			contextMenu.MenuItems[6].Enabled = true;
			contextMenu.MenuItems[5].Enabled = false;
		}

		private void menuItem1_Click(object sender, System.EventArgs e)
		{
			this.Show();
			this.BringToFront();
		}

		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			this.Hide();
		}

		private void Form1_Activated(object sender, System.EventArgs e)
		{
			//
			if ((config.startMinimized) && (fisrtView))
			{
				this.Hide();
				fisrtView = false;
			}
		}

		private void OpenInBrowser(string URL)
		{
			RegistryKey rk = Registry.ClassesRoot.OpenSubKey(@"htmlfile\shell\open\command", true);

			// Get the data from a specified item in the key.
			String s = (String)rk.GetValue("");

			int next = s.IndexOf("\"");
			s = s.Substring(next+1,s.IndexOf("\"",next+1) - next-1);

			try
			{
				Process.Start(s,URL);
			}
			catch (Win32Exception er)
			{
				if(er.NativeErrorCode == 2)
				{
					MessageBox.Show(er.Message + ". Check the path.");
				} 

				else if (er.NativeErrorCode == 5)
				{
					MessageBox.Show(er.Message + 
						". You do not have permission to run the default browser.");
				}
			}
		}



		private void label3_Click(object sender, System.EventArgs e)
		{
			OpenInBrowser("http://www.codeproject.com/script/articles/list_articles.asp?userid=970931");
		}

		private void startOnWindows_CheckedChanged(object sender, System.EventArgs e)
		{
			RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

			// Get the data from a specified item in the key.
			String s = (String)rk.GetValue("");

			if (startOnWindows.Checked)
				rk.SetValue("rmortega77.MyServer",Application.ExecutablePath);
			else
				rk.DeleteValue("rmortega77.MyServer");
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if ((HTTPServer != null) && (HTTPServer.IsAlive))
				HTTPServer.Stop();

			// save configuration
			//
			config.mainWindowLocation = this.Location;
			//
			config.startServing = startServing.Checked;
			config.startMinimized = startMinimized.Checked;
			config.startOnWindows = startOnWindows.Checked;

			config.SaveSettings();
			Application.ExitThread();
		}

	}
}
