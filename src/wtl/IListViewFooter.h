#pragma once

const IID IID_IListViewFooter = {0xF0034DA8, 0x8A22, 0x4151, {0x8F, 0x16, 0x2E, 0xBA, 0x76, 0x56, 0x5B, 0xCC}};
const IID IID_IListViewFooterCallback = {0x88EB9442, 0x913B, 0x4AB4, {0xA7, 0x41, 0xDD, 0x99, 0xDC, 0xB7, 0x55, 0x8B}};


class IListViewFooterCallback :
	public IUnknown
{
public:
	/// \brief Notifies the client that a footer item has been clicked
	///
	/// This method is called by the list view control to notify the client application that the user has
	/// clicked a footer item.
	///
	/// \param[in] itemIndex The zero-based index of the footer item that has been clicked.
	/// \param[in] lParam The application-defined integer value that is associated with the clicked item.
	/// \param[out] pRemoveFooter If set to \c TRUE, the list view control will remove the footer area.
	///
	/// \return An \c HRESULT error code.
	virtual HRESULT STDMETHODCALLTYPE OnButtonClicked(int itemIndex, LPARAM lParam, PINT pRemoveFooter) = 0;
	/// \brief Notifies the client that a footer item has been removed
	///
	/// This method is called by the list view control to notify the client application that it has removed a
	/// footer item.
	///
	/// \param[in] itemIndex The zero-based index of the footer item that has been removed.
	/// \param[in] lParam The application-defined integer value that is associated with the removed item.
	///
	/// \return An \c HRESULT error code.
	virtual HRESULT STDMETHODCALLTYPE OnDestroyButton(int itemIndex, LPARAM lParam) = 0;
};



class IListViewFooter :
	public IUnknown
{
public:
	/// \brief Retrieves whether the footer area is currently displayed
	///
	/// Retrieves whether the list view control's footer area is currently displayed.
	///
	/// \param[out] pVisible \c TRUE if the footer area is visible; otherwise \c FALSE.
	///
	/// \return An \c HRESULT error code.
	virtual HRESULT STDMETHODCALLTYPE IsVisible(PINT pVisible) = 0;
	/// \brief Retrieves the caret footer item
	///
	/// Retrieves the list view control's focused footer item.
	///
	/// \param[out] pItemIndex Receives the zero-based index of the footer item that has the keyboard focus.
	///
	/// \return An \c HRESULT error code.
	virtual HRESULT STDMETHODCALLTYPE GetFooterFocus(PINT pItemIndex) = 0;
	/// \brief Sets the caret footer item
	///
	/// Sets the list view control's focused footer item.
	///
	/// \param[in] itemIndex The zero-based index of the footer item to which to set the keyboard focus.
	///
	/// \return An \c HRESULT error code.
	virtual HRESULT STDMETHODCALLTYPE SetFooterFocus(int itemIndex) = 0;
	/// \brief Sets the footer area's caption
	///
	/// Sets the title text of the list view control's footer area.
	///
	/// \param[in] pText The text to display in the footer area's title.
	///
	/// \return An \c HRESULT error code.
	virtual HRESULT STDMETHODCALLTYPE SetIntroText(LPCWSTR pText) = 0;
	/// \brief Makes the footer area visible
	///
	/// Makes the list view control's footer area visible and registers the callback object that is notified
	/// about item clicks and item deletions.
	///
	/// \param[in] pCallbackObject The \c IListViewFooterCallback implementation of the callback object to
	///            register.
	///
	/// \return An \c HRESULT error code.
	virtual HRESULT STDMETHODCALLTYPE Show(IListViewFooterCallback* pCallbackObject) = 0;
	/// \brief Removes all footer items
	///
	/// Removes all footer items from the list view control's footer area.
	///
	/// \return An \c HRESULT error code.
	virtual HRESULT STDMETHODCALLTYPE RemoveAllButtons(void) = 0;
	/// \brief Inserts a footer item
	///
	/// Inserts a new footer item with the specified properties at the specified position into the list view
	/// control.
	///
	/// \param[in] insertAt The zero-based index at which to insert the new footer item.
	/// \param[in] pText The new footer item's text.
	/// \param[in] pUnknown ???
	/// \param[in] iconIndex The zero-based index of the new footer item's icon.
	/// \param[in] lParam The integer data that will be associated with the new footer item.
	///
	/// \return An \c HRESULT error code.
	virtual HRESULT STDMETHODCALLTYPE InsertButton(int insertAt, LPCWSTR pText, LPCWSTR pUnknown, UINT iconIndex, LONG lParam) = 0;
	/// \brief Retrieves a footer item's associated data
	///
	/// Retrieves the integer data associated with the specified footer item.
	///
	/// \param[in] itemIndex The zero-based index of the footer for which to retrieve the associated data.
	/// \param[out] pLParam Receives the associated data.
	///
	/// \return An \c HRESULT error code.
	virtual HRESULT STDMETHODCALLTYPE GetButtonLParam(int itemIndex, LONG* pLParam) = 0;
};
