#pragma once

#define WINVER		0x0600
#define _WIN32_WINNT	0x0600
#define _WIN32_IE	0x0700
#define _RICHEDIT_VER	0x0100
#define NTDDI_VERSION NTDDI_WIN7

#include <atlbase.h>
#include <atlapp.h>

extern CAppModule _Module;

#include <atlmisc.h> // ADDED (Needed for CRect)
#include <atlcom.h>
#include <atlhost.h>
#include <atlwin.h>
#include <atlframe.h>
#include <atlctl.h>
#include <atlctrls.h>
#include <atlctrlx.h>
#include <atldlgs.h>
#include <atltheme.h>
#include "atldwm.h"


#include <commoncontrols.h>
#include <strsafe.h>
#include <uxtheme.h>

#pragma comment(lib, "strsafe.lib")
#pragma comment(lib, "uxtheme.lib")
/*
#if defined _M_IX86
  #pragma comment(linker, "/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='x86' publicKeyToken='6595b64144ccf1df' language='*'\"")
#elif defined _M_IA64
  #pragma comment(linker, "/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='ia64' publicKeyToken='6595b64144ccf1df' language='*'\"")
#elif defined _M_X64
  #pragma comment(linker, "/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='amd64' publicKeyToken='6595b64144ccf1df' language='*'\"")
#else
  #pragma comment(linker, "/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")
#endif
*/
#define LVM_QUERYINTERFACE (LVM_FIRST + 189)

__inline 
BOOL IsWin7(void)
{
	OSVERSIONINFO ovi = { sizeof(OSVERSIONINFO) };
	if(GetVersionEx(&ovi)) {
		return ((ovi.dwMajorVersion == 6 && ovi.dwMinorVersion >= 1) || ovi.dwMajorVersion > 6);
	}
	return FALSE;
}

