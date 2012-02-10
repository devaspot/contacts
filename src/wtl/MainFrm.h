
#pragma once

#include "Misc.h"
#include "VirtualListView.h"
#include "NavigationView.h"
//#include "SearchControl.h"

class CMainFrame : 
	public CAeroFrameImpl<CMainFrame>,
	//public CUpdateUI<CMainFrame>,
	public CMessageFilter,
	public CIdleHandler
{

public:

	enum { CY_NAVBAR = 100 };

	DECLARE_FRAME_WND_CLASS(NULL, IDR_MAINFRAME)

	CComObject<CGroupedVirtualModeView>* listView;
	CNavigationView navigationBar;
	//CContainedWindowT<CSearchEditCtrl> searchControl;

	CMainFrame() //: navigationBar(this, 1)
	{
	}

	virtual BOOL PreTranslateMessage(MSG* pMsg)
	{
		if(CAeroFrameImpl<CMainFrame>::PreTranslateMessage(pMsg))
			return TRUE;

		return false; //listView->PreTranslateMessage(pMsg);
	}

	virtual BOOL OnIdle()
	{
		return FALSE;
	}

	//BEGIN_UPDATE_UI_MAP(CMainFrame)
	//END_UPDATE_UI_MAP()

	BEGIN_MSG_MAP(CMainFrame)
		//CHAIN_MSG_MAP(CUpdateUI<CMainFrame>)
		MESSAGE_HANDLER(WM_CREATE, OnCreate)
		MESSAGE_HANDLER(WM_DESTROY, OnDestroy)
		CHAIN_MSG_MAP(CAeroFrameImpl<CMainFrame>)
		MESSAGE_HANDLER(WM_SIZE, OnSize)
		DEFAULT_REFLECTION_HANDLER()
		REFLECT_NOTIFICATIONS()
	END_MSG_MAP()

	LRESULT OnCreate(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/)
	{
		// register object for message filtering and idle updates
		CMessageLoop* pLoop = _Module.GetMessageLoop();
		ATLASSERT(pLoop != NULL);
		pLoop->AddMessageFilter(this);
		pLoop->AddIdleHandler(this);

		CComObject<CGroupedVirtualModeView>::CreateInstance(&listView);

		m_hWndClient = 
			listView->Create(m_hWnd, rcDefault, NULL, WS_VSCROLL  |
				WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN | 
				LVS_ICON | LVS_SHOWSELALWAYS | LVS_AUTOARRANGE | LVS_ALIGNTOP | LVS_OWNERDATA, 
				/*WS_EX_CLIENTEDGE | WS_EX_TRANSPARENT*/0);

		MARGINS m2 = {2,2, CY_NAVBAR, 2};
		SetMargins(m2);

		/*
		searchControl.Create(m_hWnd, rcDefault, NULL,
			WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN | 
			RBS_VARHEIGHT | RBS_AUTOSIZE | RBS_VERTICALGRIPPER | 
			CCS_NODIVIDER | CCS_NOPARENTALIGN | CCS_TOP, 
			WS_EX_TOOLWINDOW | WS_EX_CONTROLPARENT);
		*/

		navigationBar.Create(m_hWnd, rcDefault, NULL,
			WS_CHILD | WS_VISIBLE | WS_GROUP | WS_CLIPSIBLINGS | WS_CLIPCHILDREN | 
			RBS_VARHEIGHT | RBS_AUTOSIZE | RBS_VERTICALGRIPPER | 
			CCS_NODIVIDER | CCS_NOPARENTALIGN | CCS_TOP, 
			WS_EX_CONTROLPARENT);

		return 0;
	}

	LRESULT OnDestroy(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& bHandled)
	{
		CMessageLoop* pLoop = _Module.GetMessageLoop();
		ATLASSERT(pLoop != NULL);
		pLoop->RemoveMessageFilter(this);
		pLoop->RemoveIdleHandler(this);

		bHandled = FALSE;
		return 1;
	}

	LRESULT OnSize(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM lParam, BOOL& /*bHandled*/)
	{
		CRect rect;
		GetClientRect(&rect);
		CRect rcSearch(0,60,rect.Width(),80);
		CRect rcList(0,95,rect.Width(),rect.Height());
		navigationBar.SetWindowPos(NULL, &rcSearch, SWP_NOACTIVATE);

		return 0;
	}


};
