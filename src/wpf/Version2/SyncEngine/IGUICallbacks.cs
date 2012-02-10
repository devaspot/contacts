using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Synrc
{
	public interface IGUICallbacks
	{
		void HideContacts();
		void ShowContacts();
		
		void UpdateItems();
		void UpdateApplicationMenu();
		
		void EndSync(ISyncSource source, ISyncSource destination);
		void Progress(ISyncSource source, string message, int val, int max);
		void CancelSync(ISyncSource source, string message, string link);

		ISyncSource CurrentProvider { get; set;  }
	}

	
}
