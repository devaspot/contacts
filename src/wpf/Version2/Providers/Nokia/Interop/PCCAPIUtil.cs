//Filename    : PCCAPIUtils.cs
//Part of     : PCCAPI Example codes
//Description : Helper utilities, error management
//Version     : 3.2
//
//This example is only to be used with PC Connectivity API version 3.2.
//Compability ("as is") with future versions is not quaranteed.
//
//Copyright (c) 2005-2007 Nokia Corporation.
//
//This material, including but not limited to documentation and any related
//computer programs, is protected by intellectual property rights of Nokia
//Corporation and/or its licensors.
//All rights are reserved. Reproducing, modifying, translating, or
//distributing any or all of this material requires the prior written consent
//of Nokia Corporation. Nokia Corporation retains the right to make changes
//to this material at any time without notice. A copyright license is hereby
//granted to download and print a copy of this material for personal use only.
//No other license to any other intellectual property rights is granted. The
//material is provided "as is" without warranty of any kind, either express or
//implied, including without limitation, any warranty of non-infringement,
//merchantability and fitness for a particular purpose. In no event shall
//Nokia Corporation be liable for any direct, indirect, special, incidental,
//or consequential loss or damages, including but not limited to, lost profits
//or revenue,loss of use, cost of substitute program, or loss of data or
//equipment arising out of the use or inability to use the material, even if
//Nokia Corporation has been advised of the likelihood of such damages occurring.
namespace Synrc
{
	public class PCCAPIUtils
	{
		//===================================================================
		// CONAError2String --  Returns error text for given CONA error code
		//
		//
		//===================================================================
		public static string CONAError2String(int errorCode)
		{
			string functionReturnValue = null;
			functionReturnValue = "";
			switch (errorCode)
			{
				case PCCSErrors.CONA_OK:
					functionReturnValue = "OK?";
					break;
				case PCCSErrors.CONA_OK_UPDATED_MEMORY_VALUES:
					functionReturnValue = "Inconsistance. Updated memory values.";
					break;
				case PCCSErrors.CONA_OK_UPDATED_MEMORY_AND_FILES:
					functionReturnValue = "Inconsistance. Updated memory and files.";
					break;
				case PCCSErrors.CONA_OK_UPDATED:
					functionReturnValue = "Inconsistance.";
					break;
				case PCCSErrors.CONA_OK_BUT_USER_ACTION_NEEDED:
					functionReturnValue = "User action needed on device.";
					break;
				case PCCSErrors.CONA_WAIT_CONNECTION_IS_BUSY:
					functionReturnValue = "Connection is busy.";
					break;

				// Common error codes:
				case PCCSErrors.ECONA_INIT_FAILED:
					functionReturnValue = "DLL initialization failed.";
					break;
				case PCCSErrors.ECONA_INIT_FAILED_COM_INTERFACE:
					functionReturnValue = "Failed to get connection to system.";
					break;
				case PCCSErrors.ECONA_NOT_INITIALIZED:
					functionReturnValue = "API is not initialized.";
					break;
				case PCCSErrors.ECONA_UNSUPPORTED_API_VERSION:
					functionReturnValue = "API version not supported.";
					break;
				case PCCSErrors.ECONA_NOT_SUPPORTED_MANUFACTURER:
					functionReturnValue = "Manufacturer is not supported.";
					break;

				case PCCSErrors.ECONA_UNKNOWN_ERROR:
					functionReturnValue = "Failed, unknown error.";
					break;
				case PCCSErrors.ECONA_UNKNOWN_ERROR_DEVICE:
					functionReturnValue = "Failed, unknown error from device.";
					break;
				case PCCSErrors.ECONA_INVALID_POINTER:
					functionReturnValue = "Required pointer is invalid.";
					break;
				case PCCSErrors.ECONA_INVALID_PARAMETER:
					functionReturnValue = "Invalid parameter value.";
					break;
				case PCCSErrors.ECONA_INVALID_HANDLE:
					functionReturnValue = "Invalid handle.";
					break;
				case PCCSErrors.ECONA_NOT_ENOUGH_MEMORY:
					functionReturnValue = "Memory allocation failed in PC.";
					break;
				case PCCSErrors.ECONA_WRONG_THREAD:
					functionReturnValue = "Wrong thread.";
					break;
				case PCCSErrors.ECONA_REGISTER_ALREADY_DONE:
					functionReturnValue = "Notification interface is already registered.";
					break;

				case PCCSErrors.ECONA_CANCELLED:
					functionReturnValue = "Operation cancelled by user.";
					break;
				case PCCSErrors.ECONA_NOTHING_TO_CANCEL:
					functionReturnValue = "No running functions.";
					break;
				case PCCSErrors.ECONA_FAILED_TIMEOUT:
					functionReturnValue = "Operation failed because of timeout.";
					break;
				case PCCSErrors.ECONA_NOT_SUPPORTED_DEVICE:
					functionReturnValue = "Device does not support operation.";
					break;
				case PCCSErrors.ECONA_NOT_SUPPORTED_PC:
					functionReturnValue = "Operation not supported.";
					break;
				case PCCSErrors.ECONA_NOT_FOUND:
					functionReturnValue = "Sync Item was not found.";
					break;
				case PCCSErrors.ECONA_FAILED:
					functionReturnValue = "The called operation failed.";
					break;

				case PCCSErrors.ECONA_API_NOT_FOUND:
					functionReturnValue = "Needed API module was not found.";
					break;
				case PCCSErrors.ECONA_API_FUNCTION_NOT_FOUND:
					functionReturnValue = "Function not found.";
					break;

				// Device manager and device connection related errors:
				case PCCSErrors.ECONA_DEVICE_NOT_FOUND:
					functionReturnValue = "Device not found.";
					break;
				case PCCSErrors.ECONA_NO_CONNECTION_VIA_MEDIA:
					functionReturnValue = "No connection via media.";
					break;
				case PCCSErrors.ECONA_NO_CONNECTION_VIA_DEVID:
					functionReturnValue = "Not connection via device id.";
					break;
				case PCCSErrors.ECONA_INVALID_CONNECTION_TYPE:
					functionReturnValue = "Connection type was invalid.";
					break;
				case PCCSErrors.ECONA_NOT_SUPPORTED_CONNECTION_TYPE:
					functionReturnValue = "Device does not support connection type.";
					break;
				case PCCSErrors.ECONA_CONNECTION_BUSY:
					functionReturnValue = "Other application has reserved connection.";
					break;
				case PCCSErrors.ECONA_CONNECTION_LOST:
					functionReturnValue = "Connection lost to device.";
					break;
				case PCCSErrors.ECONA_CONNECTION_REMOVED:
					functionReturnValue = "Connection removed.";
					break;
				case PCCSErrors.ECONA_CONNECTION_FAILED:
					functionReturnValue = "Connection failed, unknown reason.";
					break;
				case PCCSErrors.ECONA_SUSPEND:
					functionReturnValue = "Connection suspended.";
					break;
				case PCCSErrors.ECONA_NAME_ALREADY_EXISTS:
					functionReturnValue = "Friendly name already exists.";
					break;
				case PCCSErrors.ECONA_MEDIA_IS_NOT_WORKING:
					functionReturnValue = "Target media is not working.";
					break;
				case PCCSErrors.ECONA_CACHE_IS_NOT_AVAILABLE:
					functionReturnValue = "Cache is not available.";
					break;
				case PCCSErrors.ECONA_MEDIA_IS_NOT_ACTIVE:
					functionReturnValue = "Target media is not ready.";
					break;
				case PCCSErrors.ECONA_PORT_OPEN_FAILED:
					functionReturnValue = "Cannot open the port.";
					break;

				// Device pairing releated errors:
				case PCCSErrors.ECONA_DEVICE_PAIRING_FAILED:
					functionReturnValue = "Pairing failed.";
					break;
				case PCCSErrors.ECONA_DEVICE_PASSWORD_WRONG:
					functionReturnValue = "Wrong password on device.";
					break;
				case PCCSErrors.ECONA_DEVICE_PASSWORD_INVALID:
					functionReturnValue = "Password is invalid or missing.";
					break;

				// File System errors:
				case PCCSErrors.ECONA_ALL_LISTED:
					functionReturnValue = "All items are listed.";
					break;
				case PCCSErrors.ECONA_MEMORY_FULL:
					functionReturnValue = "Device memory full.";
					break;

				// File System errors for file functions:
				case PCCSErrors.ECONA_FILE_NAME_INVALID:
					functionReturnValue = "File name is invalid.";
					break;
				case PCCSErrors.ECONA_FILE_NAME_TOO_LONG:
					functionReturnValue = "File name too long.";
					break;
				case PCCSErrors.ECONA_FILE_ALREADY_EXIST:
					functionReturnValue = "File already exists.";
					break;
				case PCCSErrors.ECONA_FILE_NOT_FOUND:
					functionReturnValue = "File not found.";
					break;
				case PCCSErrors.ECONA_FILE_NO_PERMISSION:
					functionReturnValue = "No permissions.";
					break;
				case PCCSErrors.ECONA_FILE_COPYRIGHT_PROTECTED:
					functionReturnValue = "Copyright protected.";
					break;
				case PCCSErrors.ECONA_FILE_BUSY:
					functionReturnValue = "File busy.";
					break;
				case PCCSErrors.ECONA_FILE_TOO_BIG_DEVICE:
					functionReturnValue = "File size is too big.";
					break;
				case PCCSErrors.ECONA_FILE_TYPE_NOT_SUPPORTED:
					functionReturnValue = "File has unsupported type.";
					break;
				case PCCSErrors.ECONA_FILE_NO_PERMISSION_ON_PC:
					functionReturnValue = "No permissions on PC.";
					break;
				case PCCSErrors.ECONA_FILE_EXIST:
					functionReturnValue = "File exists.";
					break;
				case PCCSErrors.ECONA_FILE_CONTENT_NOT_FOUND:
					functionReturnValue = "Content not found.";
					break;
				case PCCSErrors.ECONA_FILE_OLD_FORMAT:
					functionReturnValue = "File has old format.";
					break;
				case PCCSErrors.ECONA_FILE_INVALID_DATA:
					functionReturnValue = "Specified file data is invalid.";
					break;

				// File System errors for folder functions:
				case PCCSErrors.ECONA_INVALID_DATA_DEVICE:
					functionReturnValue = "Device's folder contains invalid data.";
					break;
				case PCCSErrors.ECONA_CURRENT_FOLDER_NOT_FOUND:
					functionReturnValue = "Current folder is invalid in device.";
					break;
				case PCCSErrors.ECONA_FOLDER_PATH_TOO_LONG:
					functionReturnValue = "Maximum folder path exceeds.";
					break;
				case PCCSErrors.ECONA_FOLDER_NAME_INVALID:
					functionReturnValue = "Folder name is invalid.";
					break;
				case PCCSErrors.ECONA_FOLDER_ALREADY_EXIST:
					functionReturnValue = "Folder already exists in target folder.";
					break;
				case PCCSErrors.ECONA_FOLDER_NOT_FOUND:
					functionReturnValue = "Folder does not exist in target folder.";
					break;
				case PCCSErrors.ECONA_FOLDER_NO_PERMISSION:
					functionReturnValue = "Folder has no permissions.";
					break;
				case PCCSErrors.ECONA_FOLDER_NOT_EMPTY:
					functionReturnValue = "Folder is empty.";
					break;
				case PCCSErrors.ECONA_FOLDER_NO_PERMISSION_ON_PC:
					functionReturnValue = "Folder has no permissions on PC.";
					break;

				// Application installation error:
				case PCCSErrors.ECONA_DEVICE_INSTALLER_BUSY:
					functionReturnValue = "Cannot start device's installer.";
					break;

				// Syncronization specific error codes:
				case PCCSErrors.ECONA_UI_NOT_IDLE_DEVICE:
					functionReturnValue = "Device's UI is not in idle state.";
					break;
				case PCCSErrors.ECONA_SYNC_CLIENT_BUSY_DEVICE:
					functionReturnValue = "Device's SA sync client is busy.";
					break;
				case PCCSErrors.ECONA_UNAUTHORIZED_DEVICE:
					functionReturnValue = "Unauthorized device.";
					break;
				case PCCSErrors.ECONA_DATABASE_LOCKED_DEVICE:
					functionReturnValue = "Device database is locked.";
					break;
				case PCCSErrors.ECONA_SETTINGS_NOT_OK_DEVICE:
					functionReturnValue = "Settings in Sync profile are wrong on Device.";
					break;
				case PCCSErrors.ECONA_SYNC_ITEM_TOO_BIG:
					functionReturnValue = "Sync item has too big size.";
					break;
				case PCCSErrors.ECONA_SYNC_ITEM_REJECT:
					functionReturnValue = "Sync item rehected.";
					break;
				case PCCSErrors.ECONA_SYNC_INSTALL_PLUGIN_FIRST:
					functionReturnValue = "Install plugin first before sync.";
					break;

				// Versit conversion specific error codes :			
				case PCCSErrors.ECONA_VERSIT_INVALID_PARAM:
					functionReturnValue = "Invalid parameters passed to versit.";
					break;
				case PCCSErrors.ECONA_VERSIT_UNKNOWN_TYPE:
					functionReturnValue = "Versit formats not supported.";
					break;
				case PCCSErrors.ECONA_VERSIT_INVALID_VERSIT_OBJECT:
					functionReturnValue = "Invalid Verisit object.";
					break;

				// Database specific error codes:
				case PCCSErrors.ECONA_DB_TRANSACTION_ALREADY_STARTED:
					functionReturnValue = "Another transaction is already in progress";
					break;
				case PCCSErrors.ECONA_DB_TRANSACTION_FAILED:
					functionReturnValue = "Transaction was rolled back";
					break;

				// Backup specific error codes:
				case PCCSErrors.ECONA_DEVICE_BATTERY_LEVEL_TOO_LOW:
					functionReturnValue = "Device's battery level is low.";
					break;
				case PCCSErrors.ECONA_DEVICE_BUSY:
					functionReturnValue = "Device's backup server busy.";
					break;

				default:
					functionReturnValue = "Undefined error code"; // shouldn't occur
					break;
			}
			return functionReturnValue;
		}

		//===================================================================
		// ErrorMessageDlg --  Show an errormessage
		//
		//
		//===================================================================
		public static void ShowErrorMessage(string strError, int errorCode)
		{
			string strMessage = strError + "\n" + "\n" + string.Format("Error: {0:x2}", errorCode) + "\n" + CONAError2String(errorCode);
			System.Windows.MessageBox.Show(strMessage);
		}
	}
}