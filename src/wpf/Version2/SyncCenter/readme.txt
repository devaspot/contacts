
	Synrc Contacts Sync Center Handlers
	===================================

	Projects in this directory are in format of Visual Studio 2005 and configured
	to run in .NET Framework 2.0 environment.

	1. LegacySyncHandlerRegistrator.sln
	
			Laucher of Legacy (ISyncMgrSynchronize)
			Sync Center Handler and Registrator Application.
			Works both under Windows XP and Vista.

	2. VistaSyncHandlerRegistrator.sln
	
			Laucher of Vista (ISyncMgrHandler)
			Sync Center Handler and Registrator Application.
			Works only from Vista.

	3. Legacy	
			
			Legacy Sync Center Handler project and sources.
			Includes Sync Center for Windows XP Interop.
			Signed COM Server assembly.
			Exposed legacy ISyncMgrSynchronize COM API.

			Links:

			[1] Creating a Custom Synchronization Manager Handler
				http://msdn.microsoft.com/en-us/library/aa480674.aspx

	4. Vista	
			
			Vista Sync Center Handler project and sources.
			Includes Sync Center for Windows XP and Sync Center for Vista Interop.
			Signed COM Server assembly.
			Exposed new ISyncMgrHandler COM API.

	5. RegistratorApplication
	
			Registrator Application project and sources.
			Must be re-references with Legacy or Vista projects when switch.
			Using ISyncMgrRegister API.
											

