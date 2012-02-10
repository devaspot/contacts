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
using CustomWindow;

namespace Synrc
{
	/// <summary>
	/// Interaction logic for XPWindow.xaml
	/// </summary>
	public partial class XPWindow : EssentialWindow
	{
		public XPWindow(string providerSpecified, bool noneProvider)
		{
			

			InitializeComponent();
			contactsControl.Owner = this;
			contactsControl.Init(providerSpecified, noneProvider);
		}

		protected override Decorator GetWindowButtonsPlaceholder()
		{
			return WindowButtonsPlaceholder;
		}

		// FORM HANDLERS

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
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
			//contactsControl.Height = this.Height - 27;
		}
	}


}
