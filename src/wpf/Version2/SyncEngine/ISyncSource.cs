//
//	Copyright (c) 2009 Synrc Research Center
//

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Communications.Contacts;
using System.Threading;
using System.Net;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Synrc
{
	public interface ISyncSource
	{
		NameNotation Notation { get; set; } // GivenMiddleFamily or FamilyGivenMiddle sync source
		string Name { get; set; } // e.g. "Nokia E71", "Gmail"
		string DisplayName { get; set; } // e.g. "Nokia E71", "Gmail"
		string Id { get; set; } // Serial Number of the device, account name, etc.
		Bitmap Image { get; }

		bool NeedAuthorization { get; } // for on-line sync sources
		NetworkCredential Credentials { get; set; } // for On-Line providers
		bool ClearCredentials { get; set; } // If to remember login and password

		void FetchTask(); // Main RETRIVAL Task executed in dedicated thread
		void UpdateTask(); // Main UPDATER Task executed in dedicated thread

		IList<IMan> Mans { get; } // simplified list for ListView binding
		IDictionary<string, Contact> Contacts { get; } // actual contacts in sync

		IDictionary<string, Contact> MapAdded { get; set; }
		IDictionary<string, Contact> MapUpdated { get; set; }
		IDictionary<string, Contact> MapRemoved { get; set; }

		Semaphore FetchSem { get; } // internal public semaphore for the sync engine 
		Semaphore UpdateSem { get; } // internal public semaphore for the sync engine 
		Semaphore SyncSem { set; } // internal public semaphore for the sync engine 
	}

	public interface ISyncSourceProfile
	{
		string ProviderName { get; set; }
		string Password { get; set; }
		string Login { get; set; }
		bool ReadOnly { get; set; }
		bool HumanNotation { get; set; }
		bool RememberSettings { get; set; }
		ImageSource ImageSource { get; set; }
	}
}
