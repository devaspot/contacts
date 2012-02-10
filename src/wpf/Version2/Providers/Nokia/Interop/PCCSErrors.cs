//
//==============================================================================
// Local Connectivity API 3.2
//
//Filename    : PCCSErrors.cs
//Description : Error Definitions
//Version     : 3.2
//
//Copyright (c) 2005, 2006, 2007 Nokia Corporation.
//This software, including but not limited to documentation and any related 
//computer programs ("Software"), is protected by intellectual property rights 
//of Nokia Corporation and/or its licensors. All rights are reserved. By using 
//the Software you agree to the terms and conditions hereunder. If you do not 
//agree you must cease using the software immediately.
//Reproducing, disclosing, modifying, translating, or distributing any or all 
//of the Software requires the prior written consent of Nokia Corporation. 
//Nokia Corporation retains the right to make changes to the Software at any 
//time without notice.
//
//A copyright license is hereby granted to use of the Software to make, publish, 
//distribute, sub-license and/or sell new Software utilizing this Software. 
//The Software may not constitute the primary value of any new software utilizing 
//this software. No other license to any other intellectual property rights of 
//Nokia or a third party is granted. 
//
//THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS 
//OR IMPLIED, INCLUDING WITHOUT LIMITATION, ANY WARRANTY OF NON-INFRINGEMENT, 
//MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. IN NO EVENT SHALL
//NOKIA CORPORATION BE LIABLE FOR ANY DIRECT, INDIRECT, SPECIAL, INCIDENTAL, 
//OR CONSEQUENTIAL LOSS OR DAMAGES, INCLUDING BUT NOT LIMITED TO, LOST PROFITS 
//OR REVENUE, LOSS OF USE, COST OF SUBSTITUTE PROGRAM, OR LOSS OF DATA OR EQUIPMENT 
//ARISING OUT OF THE USE OR INABILITY TO USE THE MATERIAL, EVEN IF 
//NOKIA CORPORATION HAS BEEN ADVISED OF THE LIKELIHOOD OF SUCH DAMAGES OCCURRING. 
//==============================================================================
namespace Synrc
{
    public class PCCSErrors
    {

        /////////////////////////////////////////////////////////////
        //// Connectivity API errors
        /////////////////////////////////////////////////////////////
        // Everything ok
        public const int CONA_OK = 0;
        // Everything ok, given data is updated because (free, used and total) memory values are changed!
        public const int CONA_OK_UPDATED_MEMORY_VALUES = 0x1;
        // Everything ok, given data is updated because files and memory values are changed!
        public const int CONA_OK_UPDATED_MEMORY_AND_FILES = 2;
        // Everything ok, given data is updated, unknown reason.
        public const int CONA_OK_UPDATED = 4;
        // Everything ok, but operation needs some user action (device side)
        public const int CONA_OK_BUT_USER_ACTION_NEEDED = 256;
        // Operation started ok but other application is reserved connection, please wait.
        public const int CONA_WAIT_CONNECTION_IS_BUSY = 257;
        // This result code comes via FS nofication when ConnAPI is initialized by value 20 or bigger.

        // Common error codes:
        // DLL initialization failed
        public const int ECONA_INIT_FAILED = -2146435072;
        // Failed to get connection to System.
        public const int ECONA_INIT_FAILED_COM_INTERFACE = -2146435070;
        // API is not initialized
        public const int ECONA_NOT_INITIALIZED = -2146435068;
        // Failed, not supported API version
        public const int ECONA_UNSUPPORTED_API_VERSION = -2146435067;
        // Failed, not supported manufacturer
        public const int ECONA_NOT_SUPPORTED_MANUFACTURER = -2146435066;
        // Failed, unknown error
        public const int ECONA_UNKNOWN_ERROR = -2146435056;
        // Failed, unknown error from Device
        public const int ECONA_UNKNOWN_ERROR_DEVICE = -2146435055;
        // Required pointer is invalid
        public const int ECONA_INVALID_POINTER = -2146435054;
        // Invalid Parameter value
        public const int ECONA_INVALID_PARAMETER = -2146435053;
        // Invalid HANDLE
        public const int ECONA_INVALID_HANDLE = -2146435052;
        // Memory allocation failed in PC
        public const int ECONA_NOT_ENOUGH_MEMORY = -2146435051;
        // Failed, Called interface was marshalled for a different thread.
        public const int ECONA_WRONG_THREAD = -2146435050;
        // Failed, notification interface is already registered.
        public const int ECONA_REGISTER_ALREADY_DONE = -2146435049;
        // Operation cancelled by ConnectivityAPI-User
        public const int ECONA_CANCELLED = -2146435040;
        // No running functions, or cancel has called too late.
        public const int ECONA_NOTHING_TO_CANCEL = -2146435039;
        // Operation failed because of timeout
        public const int ECONA_FAILED_TIMEOUT = -2146435038;
        // Device do not support operation
        public const int ECONA_NOT_SUPPORTED_DEVICE = -2146435037;
        // ConnectivityAPI do not support operation (not implemented)
        public const int ECONA_NOT_SUPPORTED_PC = -2146435036;
        // Item was not found
        public const int ECONA_NOT_FOUND = -2146435035;
        // Failed, the called operation failed.
        public const int ECONA_FAILED = -2146435034;

        // Needed API module was not found from the system
        public const int ECONA_API_NOT_FOUND = -2146434816;
        // Called API function was not found from the loaded API module
        public const int ECONA_API_FUNCTION_NOT_FOUND = -2146434815;

        // Device manager and device connection related errors:
        // Given phone is not connected (refresh device list)
        public const int ECONA_DEVICE_NOT_FOUND = -2145386496;
        // Phone is connected but not via given Media
        public const int ECONA_NO_CONNECTION_VIA_MEDIA = -2145386495;
        // Phone is not connected with given DevID
        public const int ECONA_NO_CONNECTION_VIA_DEVID = -2145386494;
        // Connection type was invalid
        public const int ECONA_INVALID_CONNECTION_TYPE = -2145386493;
        // Device do not support connection type
        public const int ECONA_NOT_SUPPORTED_CONNECTION_TYPE = -2145386492;
        // Other application is recerved connection
        public const int ECONA_CONNECTION_BUSY = -2145386491;
        // Connection is lost to Device
        public const int ECONA_CONNECTION_LOST = -2145386490;
        // Connection removed, other application is reserved connection.
        public const int ECONA_CONNECTION_REMOVED = -2145386489;
        // Connection failed, unknown reason
        public const int ECONA_CONNECTION_FAILED = -2145386488;
        // Connection removed, PC goes suspend state
        public const int ECONA_SUSPEND = -2145386487;
        // Friendly name already exist
        public const int ECONA_NAME_ALREADY_EXISTS = -2145386486;
        // Failed, target media is active but it is not working (e.g. BT-hardware stopped or removed)
        public const int ECONA_MEDIA_IS_NOT_WORKING = -2145386485;
        // Failed, cache is not available (CONASearchDevices)
        public const int ECONA_CACHE_IS_NOT_AVAILABLE = -2145386484;
        // Failed, target media is active (or ready yet)
        public const int ECONA_MEDIA_IS_NOT_ACTIVE = -2145386483;
        // Port opening failed (only when media is API_MEDIA_SERIAL and COM port is changed).
        public const int ECONA_PORT_OPEN_FAILED = -2145386482;

        // Device paring releated errors:
        // Failed, pairing failed
        public const int ECONA_DEVICE_PAIRING_FAILED = -2145386240;
        // Failed, wrong password on device. 
        public const int ECONA_DEVICE_PASSWORD_WRONG = -2145386239;
        // Failed, password includes invalid characters or missing. 
        public const int ECONA_DEVICE_PASSWORD_INVALID = -2145386238;

        // File System errors:
        // All items are listed
        public const int ECONA_ALL_LISTED = -2144337920;
        // Device memory full
        public const int ECONA_MEMORY_FULL = -2144337919;

        // File System errors for file functions:
        // File name includes invalid characters in Device or PC
        public const int ECONA_FILE_NAME_INVALID = -2143289343;
        // File name includes too many characters in Device or PC
        public const int ECONA_FILE_NAME_TOO_LONG = -2143289342;
        // File already exists in Device or PC
        public const int ECONA_FILE_ALREADY_EXIST = -2143289341;
        // File does not exist in Device or PC
        public const int ECONA_FILE_NOT_FOUND = -2143289340;
        // Not allowed to perform required operation to file in Device or PC
        public const int ECONA_FILE_NO_PERMISSION = -2143289339;
        // Not allowed to perform required operation to file in Device or PC
        public const int ECONA_FILE_COPYRIGHT_PROTECTED = -2143289338;
        // Other application has reserved file in Device or PC
        public const int ECONA_FILE_BUSY = -2143289337;
        // Device rejected the operation because file size is too big
        public const int ECONA_FILE_TOO_BIG_DEVICE = -2143289336;
        // Device rejected the operation because file unsupported type
        public const int ECONA_FILE_TYPE_NOT_SUPPORTED = -2143289335;
        public const int ECONA_FILE_NO_PERMISSION_ON_PC = -2143289334;
        // File move or rename: File is copied to target path with new name but removing the source file failed. 
        public const int ECONA_FILE_EXIST = -2143289333;
        // Specified file content does not found (e.g. unknown file section or stored position).
        public const int ECONA_FILE_CONTENT_NOT_FOUND = -2143289332;
        // Specified file content supports old engine.
        public const int ECONA_FILE_OLD_FORMAT = -2143289331;
        // Specified file data is invalid.
        public const int ECONA_FILE_INVALID_DATA = -2143289330;

        // File System errors for folder functions:
        // Device's folder contains invalid data
        public const int ECONA_INVALID_DATA_DEVICE = -2142240768;
        // Current folder is invalid in device (e.g MMC card removed).
        public const int ECONA_CURRENT_FOLDER_NOT_FOUND = -2142240767;
        // Current folder max unicode charaters count is limited to 255.
        public const int ECONA_FOLDER_PATH_TOO_LONG = -2142240766;
        // Folder name includes invalid characters in Device or PC
        public const int ECONA_FOLDER_NAME_INVALID = -2142240765;
        // Folder is already exists in target folder
        public const int ECONA_FOLDER_ALREADY_EXIST = -2142240764;
        // Folder does not exists in target folder
        public const int ECONA_FOLDER_NOT_FOUND = -2142240763;
        // Not allowed to perform required operation to folder in Devic
        public const int ECONA_FOLDER_NO_PERMISSION = -2142240762;
        // Not allowed to perform required operation because folder is not empty
        public const int ECONA_FOLDER_NOT_EMPTY = -2142240761;
        // Not allowed to perform required operation to folder in PC
        public const int ECONA_FOLDER_NO_PERMISSION_ON_PC = -2142240760;

        // Application installation error:
        // Cannot start Device's installer
        public const int ECONA_DEVICE_INSTALLER_BUSY = -2141192192;

        //Syncronization specific error codes :
        // Failed, device rejects the operation. Maybe device's UI was not IDLE-state.
        public const int ECONA_UI_NOT_IDLE_DEVICE = -2140143616;
        // Failed, device's SA sync client is busy.
        public const int ECONA_SYNC_CLIENT_BUSY_DEVICE = -2140143615;
        // Failed, device rejects the operation. No permission.
        public const int ECONA_UNAUTHORIZED_DEVICE = -2140143614;
        // Failed, device rejects the operation. Device is locked.
        public const int ECONA_DATABASE_LOCKED_DEVICE = -2140143613;
        // Failed, device rejects the operation. Maybe settings in Sync profile are wrong on Device.
        public const int ECONA_SETTINGS_NOT_OK_DEVICE = -2140143612;
        // 
        public const int ECONA_SYNC_ITEM_TOO_BIG = -2140142335;
        // All commands,Device reject the operation...
        public const int ECONA_SYNC_ITEM_REJECT = -2140142334;
        // 
        public const int ECONA_SYNC_INSTALL_PLUGIN_FIRST = -2140142330;

        // Versit conversion specific error codes :			
        // Invalid parameters passed to versit converter 
        public const int ECONA_VERSIT_INVALID_PARAM = -2139095039;
        // Failed, trying to convert versit formats not supported in VersitConverter
        public const int ECONA_VERSIT_UNKNOWN_TYPE = -2139095038;
        // Failed, validation of versit data not passed, contains invalid data
        public const int ECONA_VERSIT_INVALID_VERSIT_OBJECT = -2139095037;

        // Database specific error codes :
        // Another transaction is already in progress.
        public const int ECONA_DB_TRANSACTION_ALREADY_STARTED = -2139094784;
        // Some of operations within a transaction failed and transaction was rolled back.
        public const int ECONA_DB_TRANSACTION_FAILED = -2139094783;

        // Backup specific error codes
        // Failed, device rejects the restore operation. Device's battery level is low.
        public const int ECONA_DEVICE_BATTERY_LEVEL_TOO_LOW = -2138046464;
        // Failed, device rejects the backup/resore operation. Device's backup server busy.
        public const int ECONA_DEVICE_BUSY = -2138046463;

    }
}