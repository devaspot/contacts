#pragma once

#include "SearchBand.h"

class CNavigationView : 
	public CWindowImpl<CNavigationView, CReBarCtrl>
{

public:

	DECLARE_WND_SUPERCLASS(_T("WTL_NavigationBar"), CReBarCtrl::GetWndClassName())

	CSearchRootView m_wndSearchBand;

	BEGIN_MSG_MAP(CNavigationView)
		MESSAGE_HANDLER(WM_CREATE, OnCreate)
		REFLECT_NOTIFICATIONS()
	END_MSG_MAP()

	LRESULT OnCreate(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/)
	{
		LRESULT lRes = DefWindowProc();

		::SetWindowTheme(m_hWnd, L"NavbarComposited", NULL);

		m_wndSearchBand.Create(m_hWnd, rcDefault, NULL, 
			WS_CHILD | WS_GROUP | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN | WS_GROUP,
			WS_EX_CONTROLPARENT);

		enum { 
			CX_SEARCH = 200,
			CY_SEARCH = 28,
		};

		REBARBANDINFO rbi = { 0 };
		rbi.cbSize = sizeof(REBARBANDINFO);
		rbi.fMask = RBBIM_CHILD | RBBIM_CHILDSIZE | RBBIM_STYLE | RBBIM_SIZE | RBBIM_IDEALSIZE;
		rbi.fStyle = RBBS_TOPALIGN | RBBS_NOGRIPPER;
		rbi.hwndChild = m_wndSearchBand;
		rbi.cx = rbi.cxIdeal = rbi.cxMinChild = CX_SEARCH;
		rbi.cyChild = rbi.cyMinChild = CY_SEARCH;
		InsertBand(0, &rbi);

		return lRes;
	}
};

