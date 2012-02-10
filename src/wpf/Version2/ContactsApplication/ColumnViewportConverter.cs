//
//	Copyright (c) 2009 Synrc Research Center
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Synrc
{
	public class ColumnViewportConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double columnHeight = System.Convert.ToDouble(value);
			return new Rect(0, 0, 1, columnHeight * 2);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException("Source shouldn't be updated");
		}

		#endregion
	}
}
