//
//	Copyright (c) 2009 Synrc Research Center
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Synrc
{
    public interface IMan
    {
        string FullName { get; set; }
        string EMail { get; set; }
        string Phone { get; set; }
		string Background { get; set; }
    }

    public class Man : IMan
    {
        public string FullName { get; set; }
        public string EMail { get; set; }
        public string Phone { get; set; }
		public string Background { get; set; }

        public override string ToString()
        {
            return FullName;
        }
    }

	public static class Mans
	{
		public static IList<IMan> GetProfiles()
		{
			List<IMan> mans = new List<IMan>();
			mans.Add(new Man { FullName = "Windows Contacts", EMail = "Local", Phone = "" });
			mans.Add(new Man { FullName = "Outlook PIM", EMail = "Local", Phone = "" });
			mans.Add(new Man { FullName = "GMAIL", EMail = "On-line", Phone = "" });
			mans.Add(new Man { FullName = "LDAP", EMail = "On-Line", Phone = "" });
			mans.Add(new Man { FullName = "NOKIA", EMail = "Local", Phone = "" });
			mans.Add(new Man { FullName = "Windows Live", EMail = "On-Line", Phone = "" });
			mans.Add(new Man { FullName = "Facebook", EMail = "On-Line", Phone = "" });
			return mans;
		}
	}

}
