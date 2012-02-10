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
using System.Windows.Shapes;
using Standard;
using CustomWindow;

namespace Synrc
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class LogonWindowXP : EssentialWindow, ISyncSourceProfile
	{
		public LogonWindowXP()
		{
			InitializeComponent();
		}

		protected override Decorator GetWindowButtonsPlaceholder()
		{
			return WindowButtonsPlaceholder;
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			GlassHelper.ExtendGlassFrameComplete(this);
			GlassHelper.SetWindowThemeAttribute(this, false, false);
		}

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			this.Close();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			usernameBox.Focus();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				passwordBox.Password = null;
				this.DialogResult = false;
				this.Close();
			}
		}

		#region ISyncSourceProfile Members

		public ImageSource ImageSource
		{
			get { return image1.Source; }
			set { image1.Source = value; } 
		}

		public string ProviderName
		{
			get { return providerName.Content.ToString(); }
			set { providerName.Content = value; }
		}

		public string Password
		{
			get
			{
				return passwordBox.Password;
			}
			set
			{
				passwordBox.Password = value;
			}
		}

		public string Login
		{
			get
			{
				return usernameBox.Text;
			}
			set
			{
				usernameBox.Text = value;
			}
		}

		public bool ReadOnly
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public bool HumanNotation
		{
			get
			{
				return ((System.Windows.Controls.ComboBoxItem)comboBox1.SelectedValue).Content.Equals("Family Given Middle");
			}
			set
			{
				if (value)
					comboBox1.SelectedIndex = 0;
				else
					comboBox1.SelectedIndex = 1;
			}
		}

		public bool RememberSettings
		{
			get
			{
				return checkBox1.IsChecked.Value;
			}
			set
			{
				checkBox1.IsChecked = value;
			}
		}

		#endregion
	}
}
