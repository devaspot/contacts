
	Synrc Contacts Version 2.0
	==========================

	1. Contacts Application
	
			Windows Presentation Foundation zen-style contacts managing application.

			[1] Synrc Contacts Product Guide
				http://synrc.com/contact-manager.htm

	2. Sync Engine
			
			Synrc Sync Engine [1], Synrc Sync Replication Protocol [2],
			Synrc Sync Source Interface [3] and Synrc Sync Merge [4,5] Core Library.

			[1] Synrc Sync Multithreaded Architecture
				http://synrc.com/syncm/MultithreadedEngine.txt

			[2] Synrc Sync Replication Protocol
				http://synrc.com/syncm/ReplicationProtocol.txt

			[3] Synrc Sync Source Interface
				http://synrc.com/syncm/SyncSources.txt 

			[4] Synrc Sync Name Resolving
				http://synrc.com/syncm/NameResolving.txt 

			[5] Synrc Sync Contact Conflicts Resolving
				http://synrc.com/syncm/ContactsConflictsResolving.txt 

	3. Providers	
			
			o	Windows Contacts Sync Source Provider (base)
			o	Outlook Sync Source Provider
			o	Google Sync Source Provider
			o	Windows Live Sync Source Provider
			o	Yahoo! Sync Source Provider
			o	LDAP Sync Source Provider

			Used Libraries:

			[1] Contact.NET
				http://contacts.codeplex.com
				Licenced under Microsoft Public License (Ms-PL)
				
			[2]	Redemption
				http://www.dimastr.com/Redemption/
				This product may not be distributed by Synrc or Codeplex to you.
				Your need to download it from author's site. This DLL
				is hosted in Codeplex SVN repository for backup purposes.

			[3] NOKIA PC Connectivity API
				http://www.forum.nokia.com/Resources_and_Information/Tools/Plug-ins/Enablers/PC_Connectivity_API/

			[4] Live Framrwork SDK April 2009 CTP
				Note: Registered as Windows Azure account with App Name "Synrc Contacts"
				http://dev.live.com/

			[5] Yahoo!
				http://www.zoomasp.net/Yahoo_Contact_Importer_csharp.aspx

			[6] Facebook and Bluetooth OBEX Push Profile vCARD Transfer
				http://facebooknet.codeplex.com/
				http://blogs.msdn.com/coding4fun/archive/2007/12/28/6893024.aspx


	4. Sync Center
	
			Windows Sync Center Handlers. Managed COM objects for native intergation
			with Windows Vista Sync Center.

			Was used Creating a Custom Synchronization Manager Handler
			Example [1] for Vista Sync Center [2]. Implemented legacy
			ISyncMgrSynchronize Interface [3]  that works booth with Windows XP and Vista.

			Note: Vista ISyncMgrHandler Interface [4] are not in working state.
			Futher work for that option is needed.

			Links:

			[1] Creating a Custom Synchronization Manager Handler
				http://msdn.microsoft.com/en-us/library/aa480674.aspx

			[2] Sync Center
				http://msdn.microsoft.com/en-us/library/aa369140(VS.85).aspx

		    [3] ISyncMgrSynchronize Interface
		    	http://msdn.microsoft.com/en-us/library/bb760901(VS.85).aspx

		    [4] ISyncMgrHandler Interface
		        http://msdn.microsoft.com/en-us/library/bb760982(VS.85).aspx
			
