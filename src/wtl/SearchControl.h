#pragma once

#include "Misc.h"

class CSearchEditCtrl : 
	public ATL::CWindowImpl<CSearchEditCtrl>,
	public WTL::CCustomDraw<CSearchEditCtrl>
{
public:
	DECLARE_WND_CLASS(_T("WTL_SearchBox"))

	CContainedWindowT<CEdit> m_ctrlEdit;
	CToolBarCtrl m_ctrlToolBar;
	CImageList m_Images;
	CRect m_rcButton;            // Size of button (w. dropdown)
	bool m_bDropped;             // Is dropdown activated? (sadly there is no runtime state for this)
	bool m_bTracking;            // Is button hot?

	CSearchEditCtrl() : m_ctrlEdit(this, 1), m_bDropped(false), m_bTracking(false)
	{
	}

	BEGIN_MSG_MAP(CSearchEditCtrl)
		MESSAGE_HANDLER(WM_CREATE, OnCreate)
		MESSAGE_HANDLER(WM_DESTROY, OnDestroy)
		MESSAGE_HANDLER(WM_SIZE, OnSize)
		NOTIFY_CODE_HANDLER(TBN_DROPDOWN, OnDropDown)
		COMMAND_CODE_HANDLER(EN_CHANGE, OnEditChange)
		COMMAND_ID_HANDLER(IDC_SEARCHDROP, OnClearEdit)
		CHAIN_MSG_MAP( CCustomDraw<CSearchEditCtrl> )
		ALT_MSG_MAP( 1 )  // Edit messages
		MESSAGE_HANDLER(WM_MOUSEMOVE, OnMouseMove)
		MESSAGE_HANDLER(WM_MOUSELEAVE, OnMouseLeave)
		MESSAGE_HANDLER(WM_KEYDOWN, OnKeyDown)
	END_MSG_MAP()

	LRESULT OnCreate(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/)
	{
		USES_CONVERSION;

		m_Images.CreateFromImage(IDB_SEARCH, 24, 0, RGB(1,1,0), IMAGE_BITMAP, LR_CREATEDIBSECTION);

		m_ctrlEdit.Create(m_hWnd, rcDefault, _T(""), WS_CHILD | WS_VISIBLE | ES_LEFT | ES_AUTOHSCROLL, 0);
		m_ctrlEdit.SetFont(AtlGetDefaultShellFont());
		m_ctrlEdit.SetCueBannerText(T2CW(CString(MAKEINTRESOURCE(IDS_SEARCH))));

		DWORD dwStyle = WS_CHILD | WS_VISIBLE | WS_TABSTOP 
			| TBSTYLE_TRANSPARENT | TBSTYLE_CUSTOMERASE | TBSTYLE_FLAT | TBSTYLE_LIST | TBSTYLE_TOOLTIPS 
			| CCS_NODIVIDER | CCS_NOPARENTALIGN | CCS_NORESIZE | CCS_TOP;
		m_ctrlToolBar.Create(m_hWnd, rcDefault, _T(""), dwStyle);
		m_ctrlToolBar.SetExtendedStyle(TBSTYLE_EX_MIXEDBUTTONS |  TBSTYLE_EX_DRAWDDARROWS);
		m_ctrlToolBar.ModifyStyleEx(0, TBSTYLE_EX_DOUBLEBUFFER);
		m_ctrlToolBar.SetImageList(m_Images);
		m_ctrlToolBar.SetIndent(0);
		m_ctrlToolBar.SetButtonSize(CSize(22, 22));
		m_ctrlToolBar.InsertButton(0, IDC_SEARCHDROP, BTNS_BUTTON | BTNS_DROPDOWN, TBSTATE_ENABLED, 0, 0, 0);
		m_ctrlToolBar.GetRect(IDC_SEARCHDROP, &m_rcButton);

		return 0;
	}

	LRESULT OnDestroy(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& bHandled)
	{
		bHandled = FALSE;
		return 0;
	}

	LRESULT OnSize(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM lParam, BOOL& bHandled)
	{
		CRect rcClient(0, 0, LOWORD(lParam), HIWORD(lParam));
		CRect rcEdit(0, 2, rcClient.Width() - m_rcButton.Width(), rcClient.Height());
		m_ctrlEdit.SetWindowPos(NULL, &rcEdit, SWP_NOACTIVATE | SWP_NOZORDER);
		CRect rcToolBar(rcClient.Width() - m_rcButton.Width() + 1, -1, rcClient.right, rcClient.Height());
		m_ctrlToolBar.SetWindowPos(NULL, &rcToolBar, SWP_NOACTIVATE | SWP_NOZORDER);
		bHandled = FALSE;
		return 0;
	}

	LRESULT OnClearEdit(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
	{
		m_ctrlEdit.SetWindowText(_T(""));
		return 0;
	}

	LRESULT OnDropDown(int /*idCtrl*/, LPNMHDR /*pnmh*/, BOOL& bHandled)
	{
		m_bDropped = true;
		m_ctrlToolBar.Invalidate();

		CMenu menu;
		menu.LoadMenu(IDM_SEARCHALL);
		CMenuHandle submenu = menu.GetSubMenu(0);
		RECT rcItem = { 0 };
		m_ctrlToolBar.GetItemRect(m_ctrlToolBar.CommandToIndex(IDC_SEARCHDROP), &rcItem);
		::MapWindowPoints(m_ctrlToolBar, HWND_DESKTOP, (LPPOINT) &rcItem, 2);
		TPMPARAMS tpmp = { sizeof(tpmp) };
		rcItem.bottom -= 6;
		tpmp.rcExclude = rcItem;
		TrackPopupMenuEx(submenu, TPM_LEFTBUTTON | TPM_RIGHTALIGN, rcItem.right, rcItem.bottom, m_hWnd, &tpmp);

		m_bDropped = false;
		m_ctrlToolBar.Invalidate();
		bHandled = FALSE;
		return 0;
	}

	LRESULT OnEditChange(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
	{
		m_ctrlToolBar.ChangeBitmap(IDC_SEARCHDROP, m_ctrlEdit.GetWindowTextLength() > 0 ? 1 : 0);
		// TODO: Notify someone about SEARCH filter change!
		return 0;
	}

	// Edit messages

	LRESULT OnMouseMove(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& bHandled)
	{
		if( !m_bTracking ) {
			_StartTrackMouseLeave();
			m_bTracking = true;
			m_ctrlToolBar.Invalidate();
		}
		bHandled = FALSE;
		return 0;
	}

	LRESULT OnMouseLeave(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/)
	{
		m_bTracking = false;
		m_ctrlToolBar.Invalidate();
		return 0;
	}

	LRESULT OnKeyDown(UINT /*uMsg*/, WPARAM wParam, LPARAM /*lParam*/, BOOL& bHandled)
	{
		if( wParam == VK_ESCAPE ) {
			m_ctrlEdit.SetWindowText(_T(""));
			return 0;
		}
		bHandled = FALSE;
		return 0;
	}

	// ListView Custom Draw

	DWORD OnPreErase(int /*idCtrl*/, LPNMCUSTOMDRAW /*lpNMCustomDraw*/)
	{
		return CDRF_SKIPDEFAULT;
	}

	DWORD OnPrePaint(int /*idCtrl*/, LPNMCUSTOMDRAW /*lpNMCustomDraw*/)
	{
		return CDRF_NOTIFYITEMDRAW;
	}

	DWORD OnItemPrePaint(int /*idCtrl*/, LPNMCUSTOMDRAW lpNMCustomDraw)
	{
		LPNMTBCUSTOMDRAW lpNMTBCD = (LPNMTBCUSTOMDRAW) lpNMCustomDraw;
		CONST INT CX_DROPARROW = 16;
		CDCHandle dc = lpNMTBCD->nmcd.hdc;
		bool bIsHot = (lpNMCustomDraw->uItemState & CDIS_HOT) != 0;
		bool bIsPressed = (lpNMCustomDraw->uItemState & CDIS_SELECTED) != 0;
		CRect rcButton = lpNMTBCD->nmcd.rc;
		CRect rcDrop = lpNMTBCD->nmcd.rc;
		rcButton.right = rcDrop.left = rcButton.right - CX_DROPARROW;
		int cyMiddle = rcButton.CenterPoint().y - 2;
		// Paint button part
		COLORREF clrWhite = ::GetSysColor(COLOR_WINDOW);
		int nTextLen = m_ctrlEdit.GetWindowTextLength();
		int iImage = m_ctrlToolBar.GetBitmap(IDC_SEARCHDROP);
		if( !bIsHot || nTextLen == 0 ) {
			dc.FillSolidRect(&rcButton, clrWhite); 
			m_Images.DrawEx(iImage, dc, rcButton.CenterPoint().x - 8, rcButton.top + 3, 16, 16, clrWhite, RGB(0,0,0), ILD_TRANSPARENT);
		}
		else {
			COLORREF clrTop = RGB(234,246,253);
			COLORREF clrMiddle1 = RGB(215,239,252);
			COLORREF clrMiddle2 = RGB(189,230,253);
			COLORREF clrBottom = RGB(166,217,244);
			if( bIsPressed ) {
				clrTop = clrMiddle1 = RGB(194,228,246);
				clrMiddle2 = RGB(169,217,242);
				clrBottom =  RGB(146,204,236);
			}
			TRIVERTEX triv1[] = {
				{ rcButton.left,  rcButton.top,       (WORD)(GetRValue(clrTop) << 8),      (WORD)(GetGValue(clrTop) << 8),      (WORD)(GetBValue(clrTop) << 8),      0xFF00 },
				{ rcButton.right, cyMiddle,           (WORD)(GetRValue(clrMiddle1) << 8),  (WORD)(GetGValue(clrMiddle1) << 8),  (WORD)(GetBValue(clrMiddle1) << 8),  0xFF00 },
				{ rcButton.left,  cyMiddle,           (WORD)(GetRValue(clrMiddle2) << 8),  (WORD)(GetGValue(clrMiddle2) << 8),  (WORD)(GetBValue(clrMiddle2) << 8),  0xFF00 },
				{ rcButton.right, rcButton.bottom,    (WORD)(GetRValue(clrBottom) << 8),   (WORD)(GetGValue(clrBottom) << 8),   (WORD)(GetBValue(clrBottom) << 8),   0xFF00 },
			};
			GRADIENT_RECT grc1[] = { {0, 1}, {2, 3} };
			dc.GradientFill(triv1, sizeof(triv1) / sizeof(triv1[0]), grc1, sizeof(grc1) / sizeof(grc1[0]), GRADIENT_FILL_RECT_V);
			dc.Draw3dRect(rcButton, RGB(250,253,254), RGB(250,253,254));
			dc.FillSolidRect(CRect(rcButton.left, rcButton.top, rcButton.left + 1, rcButton.bottom), RGB(44,98,139));
			m_Images.DrawEx(iImage, dc, rcButton.CenterPoint().x - 8, rcButton.top + 3, 16, 16, clrWhite, RGB(0,0,0), ILD_TRANSPARENT);


		}
		// Paint dropdown button
		if( !bIsHot && !m_bTracking ) {
			dc.FillSolidRect(&rcDrop, clrWhite); 
		}
		else {
			COLORREF clrTop = RGB(234,246,253);
			COLORREF clrMiddle1 = RGB(215,239,252);
			COLORREF clrMiddle2 = RGB(189,230,253);
			COLORREF clrBottom = RGB(166,217,244);
			if( m_bDropped ) {
				clrTop = clrMiddle1 = RGB(194,228,246);
				clrMiddle2 = RGB(169,217,242);
				clrBottom =  RGB(146,204,236);
			}
			TRIVERTEX triv1[] = {
				{ rcDrop.left,  rcDrop.top,       (WORD)(GetRValue(clrTop) << 8),      (WORD)(GetGValue(clrTop) << 8),      (WORD)(GetBValue(clrTop) << 8),      0xFF00 },
				{ rcDrop.right, cyMiddle,         (WORD)(GetRValue(clrMiddle1) << 8),  (WORD)(GetGValue(clrMiddle1) << 8),  (WORD)(GetBValue(clrMiddle1) << 8),  0xFF00 },
				{ rcDrop.left,  cyMiddle,         (WORD)(GetRValue(clrMiddle2) << 8),  (WORD)(GetGValue(clrMiddle2) << 8),  (WORD)(GetBValue(clrMiddle2) << 8),  0xFF00 },
				{ rcDrop.right, rcDrop.bottom,    (WORD)(GetRValue(clrBottom) << 8),   (WORD)(GetGValue(clrBottom) << 8),   (WORD)(GetBValue(clrBottom) << 8),   0xFF00 },
			};
			GRADIENT_RECT grc1[] = { {0, 1}, {2, 3} };
			dc.GradientFill(triv1, sizeof(triv1) / sizeof(triv1[0]), grc1, sizeof(grc1) / sizeof(grc1[0]), GRADIENT_FILL_RECT_V);
			if( !bIsPressed ) dc.Draw3dRect(rcDrop, RGB(250,253,254), RGB(250,253,254));
			dc.FillSolidRect(CRect(rcDrop.left - 1, rcDrop.top, rcDrop.left, rcDrop.bottom), RGB(44,98,139));
		}
		// Paint dropdown arrow
		int xoffset = rcDrop.left + 2;
		POINT points[] =
		{
			{ xoffset + 2, cyMiddle - 2 },
			{ xoffset + 8, cyMiddle - 2 },
			{ xoffset + 5, cyMiddle + 1 },
		};
		CPen pen;
		CBrush brush;
		pen.CreatePen(PS_SOLID, 1, RGB(0,0,0));
		brush.CreateSolidBrush(RGB(0,0,0));
		HPEN hOldPen = dc.SelectPen(pen);
		HBRUSH hOldBrush = dc.SelectBrush(brush);
		dc.Polygon(points, sizeof(points) / sizeof(points[0]));
		dc.SelectBrush(hOldBrush);
		dc.SelectPen(hOldPen);
		return CDRF_SKIPDEFAULT;
	}

	// Implementation

	BOOL _StartTrackMouseLeave()
	{
		TRACKMOUSEEVENT tme = { 0 };
		tme.cbSize = sizeof(tme);
		tme.dwFlags = TME_LEAVE;
		tme.hwndTrack = m_ctrlEdit;
		return _TrackMouseEvent(&tme);
	}
};
