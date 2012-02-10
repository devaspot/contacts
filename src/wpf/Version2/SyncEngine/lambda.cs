using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;

public class status {
	public static void Main()
	{
		Func<int, bool> func = x => x == 5;
		System.Console.WriteLine("{0}", func(5));
	}
}