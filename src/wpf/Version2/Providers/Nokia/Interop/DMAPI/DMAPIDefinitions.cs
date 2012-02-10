//Filename    : CONADefinitions.cs
//Part of     : PCSAPI C# examples
//Description : Connectivity API data definitions, converted from DMAPIDefinitions.h
//Version     : 3.2
//
//This example is only to be used with PC Connectivity API version 3.2.
//Compability ("as is") with future versions is not quaranteed.
//
//Copyright (c) 2007 Nokia Corporation.
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

using System.Runtime.InteropServices;

namespace Synrc
{
    public class CONADefinitions
    {

        //=========================================================
        // Common definitions used in Connectivity API
        //
        // Values used in callback registering methods
        public const short API_REGISTER = 0x10;
        public const short API_UNREGISTER = 0x20;

        //=========================================================
        // Device definitions used in Connectivity API
        //
        // Media types
        public const short API_MEDIA_ALL = 1;
        public const short API_MEDIA_IRDA = 2;
        public const short API_MEDIA_SERIAL = 4;
        public const short API_MEDIA_BLUETOOTH = 8;
        public const short API_MEDIA_USB = 16;

        //Connection info structure
        public struct CONAPI_CONNECTION_INFO
        {
            public int iDeviceID;
            public int iMedia;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrDeviceName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrAddress;
            public int iState;
        }

        //Device info structure
        public struct CONAPI_DEVICE
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrSerialNumber;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrFriendlyName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrModel;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrManufacturer;
            public int iNumberOfItems;
            //Pointer to CONAPI_CONNECTION_INFO structures
            public System.IntPtr pItems;
        }

        // General device info structure
        public struct CONAPI_DEVICE_GEN_INFO
        {
            public int iSize;
            public int iType;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrTypeName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrSWVersion;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrUsedLanguage;
            public int iSyncSupport;
            public int iFileSystemSupport;
        }

        // Device product info structure
        public struct CONAPI_DEVICE_INFO_PRODUCT
        {
            public int iSize;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrProductCode;
        }

        // Device device icon structure
        public struct CONAPI_DEVICE_INFO_ICON
        {
            // [in] Size
            public int iSize;
            // [in] Reserved for future use. Must be 0.
            public int iParam;
            [MarshalAs(UnmanagedType.LPWStr)]
            // [in] Target drive info. Must include memory type (e.g. "MMC" or "DEV").
            public string pstrTarget;
            // [out]Icon data length.
            public int iDataLength;
            // [out]Pointer to icon data.
            public System.IntPtr pData;
        }

        // Device property info structure
        public struct CONAPI_GET_PROPERTY
        {
            // [in] Size
            public int iSize;
            // [in] Target property type
            public int iTargetPropertyType;
            [MarshalAs(UnmanagedType.LPWStr)]
            // [in] Target Property name
            public string pstrPropertyName;
            // [out] Result code. CONA_OK if succeeded, otherwise error code
            public int iResult;
            [MarshalAs(UnmanagedType.LPWStr)]
            // [out] Result string. If not found pointer is NULL 
            public string pstrPropertyValue;
        }

        public struct CONAPI_DEVICE_INFO_PROPERTIES
        {
            // [in] Size
            public int iSize;
            // [in] Count of CONAPI_GET_PROPERTY struct
            public int iNumberOfStructs;
            // [in] Pointer toCONAPI_GET_PROPERTY structs
            public System.IntPtr pGetPropertyInfoStructs;
        }

        // ----------------------------------------------------

        // Search definitions used with CONASearchDevices function:
        // Device is not working or unsupported device.
        public const int CONAPI_DEVICE_NOT_FUNCTIONAL = 0;
        // Device is not paired
        public const int CONAPI_DEVICE_UNPAIRED = 1;
        // Device is paired
        public const int CONAPI_DEVICE_PAIRED = 2;
        // Device is PC Suite trusted
        public const int CONAPI_DEVICE_PCSUITE_TRUSTED = 4;
        // Device is connected in wrong mode.
        public const int CONAPI_DEVICE_WRONG_MODE = 8;

        // Get all devices from cache if available
        public const int CONAPI_ALLOW_TO_USE_CACHE = 4096;
        // Get all phones from target media
        public const int CONAPI_GET_ALL_PHONES = 8192;
        // Get all paired phones from target media
        public const int CONAPI_GET_PAIRED_PHONES = 16384;
        // Get all PC Suite trusted phones from target media.
        public const int CONAPI_GET_TRUSTED_PHONES = 32768;

        // Search macros used to check device's trusted/paired state: 
        public static int CONAPI_IS_DEVICE_UNPAIRED(int iState)
        {
            return (iState & 1); // Returns 1 if true
        }
        public static int CONAPI_IS_DEVICE_PAIRED(int iState)
        {
            return ((iState >> 1) & 1); // Returns 1 if true
        }
        public static int CONAPI_IS_PCSUITE_TRUSTED(int iState)
        {
            return ((iState >> 2) & 1); // Returns 1 if true
        }

        // Definitions used with CONAChangeDeviceTrustedState function:
        // Pair device
        public const int CONAPI_PAIR_DEVICE = 256;
        // Unpair device
        public const int CONAPI_UNPAIR_DEVICE = 512;
        // Set device to PC Suite trusted 
        public const int CONAPI_SET_PCSUITE_TRUSTED = 1024;
        // Remove PC Suite trusted information.
        public const int CONAPI_SET_PCSUITE_UNTRUSTED = 2048;

        // Definitions used with CONAGetDeviceInfo function:
        // Get CONAPI_DEVICE_GEN_INFO struct.
        public const int CONAPI_DEVICE_GENERAL_INFO = 65536;
        // Get CONAPI_DEVICE_INFO_PRODUCT struct.
        public const int CONAPI_DEVICE_PRODUCT_INFO = 1048576;
        // Get CONAPI_DEVICE_INFO_PROPERTIES struct.
        public const int CONAPI_DEVICE_PROPERTIES_INFO = 16777216;
        // Get CONAPI_DEVICE_ICON struct.
        public const int CONAPI_DEVICE_ICON_INFO = 268435456;

        // Definitions used with CONAPI_DEVICE_INFO_PROPERTIES struct
        // Get value from configuration file.
        public const int CONAPI_DEVICE_GET_PROPERTY = 1;
        // Check is the application supported in configuration file.
        // pstrPropertyName must be include target property name.
        public const int CONAPI_DEVICE_IS_APP_SUPPORTED = 2;
        // The next properties are returned from device's OBEX Capability object:
        // Get Current Network string.
        // pstrPropertyName must be include target application name.
        public const int CONAPI_DEVICE_GET_CURRENT_NETWORK = 16777220;
        // Get Country Code string.
        public const int CONAPI_DEVICE_GET_COUNTRY_CODE = 33554436;
        // Get Network ID string.
        public const int CONAPI_DEVICE_GET_NETWORK_ID = 50331652;
        // Get Version string from CONAPI_CO_xxx_SERVICE Service.
        public const int CONAPI_DEVICE_GET_VERSION = 1048580;
        // Get UUID string from CONAPI_CO_xxx_SERVICE Service.
        public const int CONAPI_DEVICE_GET_UUID = 2097156;
        // Get Object type string from CONAPI_CO_xxx_SERVICE Service.
        public const int CONAPI_DEVICE_GET_OBJTYPE = 3145732;
        // Get file path string from CONAPI_CO_xxx_SERVICE Service.
        public const int CONAPI_DEVICE_GET_FILEPATH = 4194308;
        // Get folder path string from CONAPI_CO_xxx_SERVICE Service.
        // pstrPropertyName must be include type of file.
        public const int CONAPI_DEVICE_GET_FOLDERPATH = 5242884;
        // Get folder memory type string from CONAPI_CO_xxx_SERVICE Service. 
        // pstrPropertyName must be include type of folder (e.g. "Images").
        public const int CONAPI_DEVICE_GET_FOLDERMEMTYPE = 6291460;
        // Get folder exclude path string from CONAPI_CO_xxx_SERVICE Service.
        // pstrPropertyName must be include type of folder.
        public const int CONAPI_DEVICE_GET_FOLDEREXCLUDE = 7340036;
        // Get all values from CONAPI_CO_xxx_SERVICE Service. Values are separated with hash mark (#).
        // pstrPropertyName must be include type of folder.
        public const int CONAPI_DEVICE_GET_ALL_VALUES = 8388612;
        // Definitions for Services

        // Data Synchronication Service
        // pstrPropertyName must be include type of item.
        public const int CONAPI_DS_SERVICE = 4096;
        // Device Management Service
        public const int CONAPI_DM_SERVICE = 8192;
        // NEF Service
        public const int CONAPI_NEF_SERVICE = 12288;
        // Data Synchronication SMS Service
        public const int CONAPI_DS_SMS_SERVICE = 16384;
        // Data Synchronication MMS Service
        public const int CONAPI_DS_MMS_SERVICE = 20480;
        // Data Synchronication Bookmarks Service
        public const int CONAPI_DS_BOOKMARKS_SERVICE = 24576;
        // Folder-Browsing Service
        public const int CONAPI_FOLDER_BROWSING_SERVICE = 28672;
        // User defined Service. The service name must be set to pstrPropertyName. 
        public const int CONAPI_USER_DEFINED_SERVICE = 32768;

        // Definitions used with General device info structure
        // Device types:

        // Unknown device.
        public const int CONAPI_UNKNOWN_DEVICE = 0;
        // Series 40 device
        public const int CONAPI_SERIES40_DEVICE = 16777217;
        // Series 60 the 2nd edition device.
        public const int CONAPI_SERIES60_2ED_DEVICE = 33554448;
        // Series 60 the 3nd edition device.
        public const int CONAPI_SERIES60_3ED_DEVICE = 33554464;
        // Series 80 device.
        public const int CONAPI_SERIES80_DEVICE = 33554688;
        // Nokia 7710 device.
        public const int CONAPI_NOKIA7710_DEVICE = 33558528;

        // Synchronication support:
        // Device is not supporting synchronication.
        public const int CONAPI_SYNC_NOT_SUPPORTED = 0;
        // Device is supporting Server Alerted (SA) Data Synchronication. 
        public const int CONAPI_SYNC_SA_DS = 1;
        // Device is supporting Server Alerted (SA) Device Management. 
        public const int CONAPI_SYNC_SA_DM = 2;
        // Device is supporting Client Initated (CI) Data Synchronication.
        public const int CONAPI_SYNC_CI_DS = 16;

        // File System support: 
        // Device is not support file system.
        public const int CONAPI_FS_NOT_SUPPORTED = 0;
        // Device is support file system.
        public const int CONAPI_FS_SUPPORTED = 1;
        // Device is supporting Java MIDlet installation.
        public const int CONAPI_FS_INSTALL_JAVA_APPLICATIONS = 16;
        // Device is supporting SIS applications installation. 
        public const int CONAPI_FS_INSTALL_SIS_APPLICATIONS = 32;
        // Device supports SISX applications' installation. 
        public const int CONAPI_FS_INSTALL_SISX_APPLICATIONS = 64;
        // Device is supporting file conversion.
        public const int CONAPI_FS_FILE_CONVERSION = 256;
        // Device supports installed applications' listing.
        public const int CONAPI_FS_LIST_APPLICATIONS = 512;
        // Device supports installed applications' uninstallation.
        public const int CONAPI_FS_UNINSTALL_APPLICATIONS = 1024;
        // Device supports extended File System operations (e.g. Copy folder).
        public const int CONAPI_FS_EXTENDED_OPERATIONS = 2048;

        // Definitions used in CONASetDeviceListOption function

        // Option types:
        // pstrValue contains the manufacturer name
        public const int DMAPI_OPTION_SET_MANUFACTURER = 1;

        // ----------------------------------------------------
        // DeviceNotifyCallbackFunction
        //
        //	This is the function prototype of the callback method
        //
        //	DWORD DeviceNotifyCallbackFunction(	DWORD dwStatus, WCHAR* pstrSerialNumber);
        //	
        //	Status value uses the following format:
        //
        //		----------------DWORD------------------
        //		WORD for info		WORD for status
        //		0000 0000 0000 0000 0000 0000 0000 0000
        //
        //	Status value is the one of the values defined below describing main reason for the notification.
        //	Info part consist of two parts:
        //		LOBYTE: Info part contains change info value. See info values below.
        //		HIBYTE:	Info data value. Depends of info value.
        //	See info value definitions for more information.
        //	Use predefined macros to extract needed part from the status value.
        //
        public delegate int DeviceNotifyCallbackDelegate(
            int iStatus,
            [MarshalAs(UnmanagedType.LPWStr)] string pstrSerialNumber
            );

        //Device callback status values
        // List is updated. No any specific information.
        public const int CONAPI_DEVICE_LIST_UPDATED = 0;
        // A new device is added to the list.
        public const int CONAPI_DEVICE_ADDED = 1;
        // Device is removed from the list.
        public const int CONAPI_DEVICE_REMOVED = 2;
        // Device is updated. A connection is added or removed
        public const int CONAPI_DEVICE_UPDATED = 4;
        // Device callback info values
        // Note! HIBYTE == media, LOBYTE == CONAPI_CONNECTION_ADDED
        public const int CONAPI_CONNECTION_ADDED = 1;
        // Note! HIBYTE == media, LOBYTE == CONAPI_CONNECTION_REMOVED
        public const int CONAPI_CONNECTION_REMOVED = 2;
        // Friendly name of the device is changed
        public const int CONAPI_DEVICE_RENAMED = 4;

        // Device callback macros
        public static int GET_CONAPI_CB_STATUS(int iStatus)
        {
            return (65535 & iStatus);
        }
        public static int GET_CONAPI_CB_INFO(int iStatus)
        {
            return ((16711680 & iStatus) >> 16);
        }
        public static int GET_CONAPI_CB_INFO_DATA(int iStatus)
        {
            return ((-16777216 + iStatus) >> 24);
        }

        // ----------------------------------------------------------------------
        // DeviceSearchOperationCallbackFunction
        //
        // Description
        // Device Search operation callback functions are defined as: 
        //	DWORD (DeviceSearchOperationCallbackFunction)(DWORD dwState, 
        //					CONAPI_CONNECTION_INFO* pConnInfoStructure)
        //
        //	The Connectivity API calls this function at least every time period 
        //	(or if the System has found the device during this time) and adds one 
        //	to the function state value. The used time period counted by using 
        //	dwSearchTime parameter. E.g. If dwSearchTime paramater value is 240,
        //	time period  (240/100) is 2.4 seconds.
        //	If the function state is 100 and any device does not have found during 
        //	this (dwSearchTime) time the CONASearchDevices function fails with the 
        //	error code ECONA_FAILED_TIMEOUT.
        //
        // Parameters
        //	dwState				[in] Function state (0-100%).
        //	pConnInfoStructure	[in] Reserved for future use, the value is NULL.
        //
        // Return values
        // The Connectivity API-user must return the CONA_OK value. If the callback 
        // function returns the error code ECONA_CANCELLED to the Connectivity API, 
        // the CONASearchDevices function will be cancelled with the error code ECONA_CANCELLED.
        //
        // Type definition: 
        public delegate int SearchCallbackDelegate(
            int iState,
            System.IntPtr pConnInfoStructure
            );

        //================================
        // File system API definitions
        //===============================

        //Used for changing current folder:
        public const string GO_TO_ROOT_FOLDER = "\\\\";
        public const string GO_TO_PARENT_FOLDER = "..\\";
        public const string FOLDER_SEPARATOR = "\\";

        //Options for CONADeleteFolder:
        public const int CONA_DELETE_FOLDER_EMPTY = 0;
        public const int CONA_DELETE_FOLDER_WITH_FILES = 1;

        //Direction options for CONACopyFile and CONAMoveFile:
        public const int CONA_DIRECT_PHONE_TO_PC = 2;
        public const int CONA_DIRECT_PC_TO_PHONE = 4;
        // Not used at the moment.
        public const int CONA_DIRECT_PHONE_TO_PHONE = 8;

        //Other options for CONACopyFile and CONAMoveFile:
        public const int CONA_OVERWRITE = 16;
        // Used only with CONACopyFile
        public const int CONA_RENAME = 32;
        // Not used at the moment.
        public const int CONA_TRANSFER_ALL = 64;

        //Options for CONAFindBegin:
        public const int CONA_FIND_USE_CACHE = 128;

        //Attribute defines for CONAPI_FOLDER_INFO and CONAPI_FILE_INFO structures:
        //Both structure
        public const int CONA_FPERM_READ = 256;
        //Both structure
        public const int CONA_FPERM_WRITE = 512;
        //Both structure
        public const int CONA_FPERM_DELETE = 1024;
        //Only for CONAPI_FOLDER_INFO
        public const int CONA_FPERM_FOLDER = 2048;
        //Only for CONAPI_FOLDER_INFO
        public const int CONA_FPERM_DRIVE = 4096;
        // Only for CONAPI_FOLDER_INFO2
        public const int CONA_FPERM_HIDDEN = 8192;
        // Only for CONAPI_FOLDER_INFO2
        public const int CONA_FPERM_ROOT = 16384;

        //Options for CONAGetFolderInfo
        // Gets target folder info
        public const int CONA_GET_FOLDER_INFO = 1;
        // Gets target folder info and contents
        public const int CONA_GET_FOLDER_CONTENT = 2;
        // Gets target folder info, content and sub folder(s) contents also
        public const int CONA_GET_FOLDER_AND_SUB_FOLDERS_CONTENT = 4;
        // Compare exist folder content. If change has happened, updates content
        public const int CONA_COMPARE_AND_UPDATE_IF_NEEDED = 256;
        //Used only with CONAInstallApplication
        // and returns CONA_OK_UPDATED. If no change, returns CONA_OK.
        public const int CONA_DEFAULT_FOLDER = 65536;
        //Used only with CONAInstallApplication
        public const int CONA_INFORM_IF_USER_ACTION_NEEDED = 131072;
        //Used only with CONAInstallApplication
        public const int CONA_WAIT_THAT_USER_ACTION_IS_DONE = 262144;

        // Used only with CONAReadFileInBlocks and CONAWriteFileInBlocks
        public const int CONA_USE_IF_NOTICATION = 16777216;
        // Used only with CONAReadFileInBlocks and CONAWriteFileInBlocks
        public const int CONA_USE_CB_NOTICATION = 33554432;
        // Used only with CONAReadFileInBlocks
        public const int CONA_NOT_SET_FILE_DETAILS = 67108864;
        // Used only with IFSAPIBlockNotify and CONABlockDataCallbackFunction
        public const int CONA_ALL_DATA_SENT = 134217728;

        //Used only with IFSAPIBlockNotify and CONABlockDataCallbackFunction
        public int CONA_IS_ALL_DATA_RECEIVED(int iState)
        {
            return ((iState >> 27) & 1);
        }

        //Options for CONAGetFileMetadata 

        // Used only with CONAGetFileMetadata
        public const int CONAPI_GET_METADATA = 8;
        // Used only with CONAGetFileMetadata and CONAFreeFileMetadataStructure
        public const int CONA_TYPE_OF_AUDIO_METADATA = 16;
        // Used only with CONAGetFileMetadata and CONAFreeFileMetadataStructure
        public const int CONA_TYPE_OF_IMAGE_METADATA = 32;


        // ----------------------------------------------------
        // Folder info structure
        public struct CONAPI_FOLDER_INFO
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            // Folder or Drive name
            public string pstrName;
            // Folder or Drive type and permission 
            public int iAttributes;
            // Folder time
            public System.Runtime.InteropServices.ComTypes.FILETIME tFolderTime;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Drive lable name 
            public string pstrLabel;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Folder or Drive memory type
            public string pstrMemoryType;
        }

        // File info structure
        public struct CONAPI_FILE_INFO
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            // File name
            public string pstrName;
            // File permission
            public int iAttributes;
            // File modified time
            public System.Runtime.InteropServices.ComTypes.FILETIME tFileTime;
            // File size
            public int iFileSize;
            [MarshalAs(UnmanagedType.LPWStr)]
            // File MIME type
            public string pstrMIMEType;
        }

        // Folder info structure
        public struct CONAPI_FOLDER_INFO2
        {
            // Size of struct
            public int iSize;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Folder or Drive name
            public string pstrName;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Absolute location path to folder or drive
            public string pstrLocation;
            // Folder or Drive type and permission 
            public int iAttributes;
            // Folder time
            public System.Runtime.InteropServices.ComTypes.FILETIME tFolderTime;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Drive lable name 
            public string pstrLabel;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Folder or Drive memory type
            public string pstrMemoryType;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Identification ID
            public string pstrID;
            // Free memory in drive
            public long dlFreeMemory;
            // Total memory in drive
            public long dlTotalMemory;
            // Used memory in drive
            public long dlUsedMemory;
            // Number of files in target folder or drive
            public int iContainFiles;
            // Number of folders in target folder or drive
            public int iContainFolders;
            // Size of folder content (including content of subfolders)
            public long dlTotalSize;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Reserved for future
            public string pstrValue;
        }

        // Folder content structure
        public struct CONAPI_FOLDER_CONTENT
        {
            // Size of struct
            public int iSize;
            // CONAPI_FOLDER_INFO2*	pFolderInfo;				 Folder info struct
            public System.IntPtr pFolderInfo;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Absolute path of sub files and sub folders
            public string pstrPath;
            // Number of file structs
            public int iNumberOfFileInfo;
            // CONAPI_FILE_INFO*		pFileInfo;					 File structs
            public System.IntPtr pFileInfo;
            // Number of file structs
            public int iNumberOfSubFolderContent;
            // CONAPI_FOLDER_CONTENT*  pSubFolderContent;			 File structs
            public System.IntPtr pSubFolderContent;
            // CONAPI_FOLDER_CONTENT*	pParentFolder;				 Pointer to the parent folder content struct
            public System.IntPtr pParentFolder;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Reserved for future	
            public string pstrValue;
        }

        // File CONAPI_FILE_AUDIO_METADATA structure
        public struct CONAPI_FILE_AUDIO_METADATA
        {
            //Size of struct
            public int iSize;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Album	
            public string pstrAlbum;
            [MarshalAs(UnmanagedType.LPWStr)]
            //AlbumTrack
            public string pstrAlbumTrack;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Artist		
            public string pstrArtist;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Comment
            public string pstrComment;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Composer
            public string pstrComposer;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Copyright
            public string pstrCopyright;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Date
            public string pstrDate;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Duration
            public string pstrDuration;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Genre
            public string pstrGenre;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Original Artist
            public string pstrOriginalArtist;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Rating
            public string pstrRating;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Title
            public string pstrTitle;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Unique File Identifier
            public string pstrUniqueFileId;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Url
            public string pstrUrl;
            [MarshalAs(UnmanagedType.LPWStr)]
            //User Url
            public string pstrUserUrl;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Vendor
            public string pstrVendor;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Year
            public string pstrYear;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Reserved for future
            public string pstrValue;
            //Version (0 = NonID3, 1 = Ver1, 2 = Ver2)
            public int iID3Version;
            //Reserved for future
            public int iValue;
            //Jpeg data lenght
            public int iJpegLenght;
            //Jpeg data
            public System.IntPtr pJpeg;
        }

        // File CONAPI_FILE_IMAGE_METADATA structure
        public struct CONAPI_FILE_IMAGE_METADATA
        {
            // Size of struct
            public int iSize;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Copyright
            public string pstrCopyright;
            [MarshalAs(UnmanagedType.LPWStr)]
            // DateTime
            public string pstrDateTime;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Date Time Digitized
            public string pstrDateTimeDigitized;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Date Time Original
            public string pstrDateTimeOriginal;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Image description
            public string pstrImageDescription;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Make
            public string pstrMake;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Maker Note
            public string pstrMakerNote;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Model
            public string pstrModel;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Iso Speed Ratings
            public string pstrIsoSpeedRatings;
            [MarshalAs(UnmanagedType.LPWStr)]
            //Related Sound File
            public string pstrRelatedSoundFile;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Software
            public string pstrSoftware;
            [MarshalAs(UnmanagedType.LPWStr)]
            // User Comment
            public string pstrUserComment;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Reserved for future
            public string pstrValue;
            [MarshalAs(UnmanagedType.LPWStr)]
            // Thumbnail image data lenght
            public string dwThumbnailLenght;
            // Thumbnail image data
            public System.IntPtr pThumbnail;
            // Infomation bit-mask for DWORD value which are exist & set.
            public long lExistValuesMask;
            // Aperture Value numerator.			If exist, dlExistValuesMask includes bit 0x0000000000000001.
            public int iApertureValueNum;
            // Aperture Value denominator.		If exist, dlExistValuesMask includes bit 0x0000000000000002.
            public int iApertureValueDen;
            // Brightness Value numerator.		If exist, dlExistValuesMask includes bit 0x0000000000000004.
            public int iBrightnessValueNum;
            // Brightness Value denominator.		If exist, dlExistValuesMask includes bit 0x0000000000000008.
            public int iBrightnessValueDen;
            // Color Space. 					    If exist, dlExistValuesMask includes bit 0x0000000000000010.
            public int iColorSpace;
            // Components Configuration (4 x 8bit values). If exist, dlExistValuesMask includes bit 0x0000000000000020.
            public int iComponentsConf;
            // Contrast. 							If exist, dlExistValuesMask includes bit 0x0000000000000040.
            public int iContrast;
            // Custom Rendered. 					If exist, dlExistValuesMask includes bit 0x0000000000000080.
            public int iCustomRendered;
            // Digital Zoom Ratio numerator.		If exist, dlExistValuesMask includes bit 0x0000000000000100.
            public int iDigitalZoomRatioNum;
            // Digital Zoom Ratio denominator. 	If exist, dlExistValuesMask includes bit 0x0000000000000200.
            public int iDigitalZoomRatioDen;
            // Exif Version. 						If exist, dlExistValuesMask includes bit 0x0000000000000400.
            public int iExifVersion;
            // Exposure Bias Value numerator. 	If exist, dlExistValuesMask includes bit 0x0000000000000800.
            public int iExposureBiasNum;
            // Exposure Bias Value denominator. 	If exist, dlExistValuesMask includes bit 0x0000000000001000.
            public int iExposureBiasDen;
            // Exposure Mode. 					If exist, dlExistValuesMask includes bit 0x0000000000002000.
            public int iExposureMode;
            // Exposure Program. 					If exist, dlExistValuesMask includes bit 0x0000000000004000.
            public int iExposureProgram;
            // Exposure Time 1.					If exist, dlExistValuesMask includes bit 0x0000000000008000.
            public int iExposureTime1;
            // Exposure Time 2.					If exist, dlExistValuesMask includes bit 0x0000000000010000.
            public int iExposureTime2;
            // File Source.						If exist, dlExistValuesMask includes bit 0x0000000000020000.
            public int iFileSource;
            // Flash.								If exist, dlExistValuesMask includes bit 0x0000000000040000.
            public int iFlash;
            // Flash Pix Version. 				If exist, dlExistValuesMask includes bit 0x0000000000080000.
            public int iFlashPixVersion;
            // Gain Control.						If exist, dlExistValuesMask includes bit 0x0000000000100000.
            public int iGainControl;
            // Gps Version.						If exist, dlExistValuesMask includes bit 0x0000000000200000.
            public int iGpsVersion;
            // Light Source.						If exist, dlExistValuesMask includes bit 0x0000000000400000.
            public int iLightSource;
            // Metering Mode.						If exist, dlExistValuesMask includes bit 0x0000000000800000.
            public int iMeteringMode;
            // Orientation.						If exist, dlExistValuesMask includes bit 0x0000000001000000.
            public int iOrientation;
            // Pixel X Dimension. 				If exist, dlExistValuesMask includes bit 0x0000000002000000.
            public int iPixelXDimension;
            // Pixel Y Dimension. 				If exist, dlExistValuesMask includes bit 0x0000000004000000.
            public int iPixelYDimension;
            // Resolution Unit.					If exist, dlExistValuesMask includes bit 0x0000000008000000.
            public int iResolutionUnit;
            // Saturation.						If exist, dlExistValuesMask includes bit 0x0000000010000000.
            public int iSaturation;
            // SceneCapture Type. 				If exist, dlExistValuesMask includes bit 0x0000000020000000.
            public int iSceneCaptureType;
            // Sharpness.							If exist, dlExistValuesMask includes bit 0x0000000040000000.
            public int iSharpness;
            // Shutter Speed Value numerator.		If exist, dlExistValuesMask includes bit 0x0000000080000000.
            public int iShutterSpeedValueNum;
            // Shutter Speed Value denominator.	If exist, dlExistValuesMask includes bit 0x0000000100000000.
            public int iShutterSpeedValueDen;
            // Thumbnail Compression.				If exist, dlExistValuesMask includes bit 0x0000000200000000.
            public int iThumbnailCompression;
            // Thumbnail Jpeg Interchange Format. If exist, dlExistValuesMask includes bit 0x0000000400000000.
            public int iThumbnailJpegIFormat;
            // Thumbnail Jpeg Interchange Format Length. If exist, dlExistValuesMask includes bit 0x0000000800000000.
            public int iThumbnailJpegIFormatLen;
            // Thumbnail ResolutionUnit.			If exist, dlExistValuesMask includes bit 0x0000001000000000.
            public int iThumbnailResUnit;
            // Thumbnail XResolution numerator.	If exist, dlExistValuesMask includes bit 0x0000002000000000.
            public int iThumbnailXResNum;
            // Thumbnail XResolution denominator. If exist, dlExistValuesMask includes bit 0x0000004000000000.
            public int iThumbnailXResDen;
            // Thumbnail XResolution numerator.	If exist, dlExistValuesMask includes bit 0x0000008000000000.
            public int iThumbnailYResNum;
            // Thumbnail YResolution denominator. If exist, dlExistValuesMask includes bit 0x0000010000000000.
            public int iThumbnailYResDen;
            // White Balance.						If exist, dlExistValuesMask includes bit 0x0000020000000000.
            public int iWhiteBalance;
            // X Resolution numerator.			If exist, dlExistValuesMask includes bit 0x0000040000000000.
            public int iXResolutionNum;
            // X Resolution denominator.			If exist, dlExistValuesMask includes bit 0x0000080000000000.
            public int iXResolutionDen;
            // Y Resolution numerator.			If exist, dlExistValuesMask includes bit 0x0000100000000000.
            public int iYResolutionNum;
            // Y Resolution denominator.			If exist, dlExistValuesMask includes bit 0x0000200000000000.
            public int iYResolutionDen;
            // YCbCr Positioning data.			If exist, dlExistValuesMask includes bit 0x0000400000000000.
            public int iYCbCrPosData;
            // Reserved for future
            public int iValue;
        }

        // ----------------------------------------------------
        // FSNotifyCallbackDelegate function:
        //
        // ----------------------------------------------------
        //	This is the function prototype of the callback method
        //
        public delegate int FSNotifyCallbackDelegate(
            int iOperation,
            int iStatus,
            int iTransferredBytes,
            int iAllBytes
            );

        // ----------------------------------------------------
        // FSFolderInfoCallbackDelegate function:
        //
        // ----------------------------------------------------
        //	This is the function prototype of the callback method
        //
        // typedef DWORD (CALLBACK *PFN_CONA_FS_FOLDERINFO_CALLBACK)(LPCONAPI_FOLDER_INFO2 pFolderInfo);
        public delegate int FSFolderInfoCallbackDelegate(System.IntPtr pFolderInfo);

        // ----------------------------------------------------
        // CONABlockDataCallbackFunction function:
        //
        // Callback function prototype:
        // typedef DWORD (CALLBACK *PFN_CONA_FS_BLOCKDATA_CALLBACK)(
        //								DWORD dwFSFunction,
        //								DWORD *pdwState,
        //								const DWORD dwSizeOfFileDataBlockBuffer,
        //								DWORD *pdwFileDataBlockLenght,
        //								unsigned char* pFileDataBlock);
        public delegate int FSBlockDataCallbackDelegate(
            int iFSFunction,
            ref int iState,
            int iSizeOfFileDataBlockBuffer,
            ref int iFileDataBlockLenght,
            System.IntPtr pFileDataBlock);


        // ----------------------------------------------------
        // FSFunction values:
        public const int CONARefreshDeviceMemoryValuesNtf = 1;
        public const int CONASetCurrentFolderNtf = 2;
        public const int CONAFindBeginNtf = 4;
        public const int CONACreateFolderNtf = 8;
        public const int CONADeleteFolderNtf = 16;
        public const int CONARenameFolderNtf = 32;
        public const int CONAGetFileInfoNtf = 64;
        public const int CONADeleteFileNtf = 128;
        public const int CONAMoveFileNtf = 256;
        public const int CONACopyFileNtf = 512;
        public const int CONARenameFileNtf = 1024;
        public const int CONAReadFileNtf = 2048;
        public const int CONAWriteFileNtf = 4096;
        public const int CONAConnectionLostNtf = 8192;
        public const int CONAInstallApplicationNtf = 16384;
        public const int CONAConvertFileNtf = 32768;
        public const int CONAGetFolderInfoNtf = 65536;
        public const int CONAListApplicationNtf = 131072;
        public const int CONAUninstallApplicationNtf = 262144;
        public const int CONAReadFileInBlocksNtf = 524288;
        public const int CONAWriteFileInBlocksNtf = 1048576;
        public const int CONAMoveFolderNtf = 2097152;
        public const int CONACopyFolderNtf = 4194304;
        public const int CONAGetFileMetadataNtf = 8388608;

        // The next function do not send notifications:
        //	CONAOpenFS					
        //	CONACloseFS				
        //	CONARegisterFSNotifyCallback
        //	CONAGetMemoryTypes 			
        //	CONAGetMemoryValues			
        //	CONAGetCurrentFolder	
        //	CONAFindNextFolder		
        //	CONAFindNextFile		
        //	CONAFindEnd					
        //	CONACancel				

        // Possible error codes value in dwStatus parameter when 
        // FSFunction value is CONAConnectionLost:
        //   ECONA_CONNECTION_LOST
        //   ECONA_CONNECTION_REMOVED
        //   ECONA_CONNECTION_FAILED
        //   ECONA_SUSPEND

        // ----------------------------------------------------
        // CONAMediaCallback
        //
        //	This is the function prototype of the callback method
        //
        //	DWORD CALLBACK CONAMediaCallback(DWORD  dwStatus, API_MEDIA* pMedia);

        public delegate int MediaCallbackDelegate(
            int iStatus,
            System.IntPtr pMedia
            );
        // ----------------------------------------------------
        // Media info structure
        public struct API_MEDIA
        {
            public int iSize;
            public int iMedia;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrDescription;
            public int iState;
            public int iOptions;
            public int iMediaData;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrID;
        }

        //Synchronication support:

        // Media is active.
        public const int API_MEDIA_ACTIVE = 1;
        // Media is not active. 
        public const int API_MEDIA_NOT_ACTIVE = 2;
        // Media is supporting incoming connections. 
        public const int API_MEDIA_IC_SUPPORTED = 16;
        // Media is not supporting incoming connections.
        public const int API_MEDIA_IC_NOT_SUPPORTED = 32;

        public static int CONAPI_GET_MEDIA_TYPE(int iMedia)
        {
            return 255 & iMedia;
        }

        public static int CONAPI_IS_MEDIA_ACTIVE(int iState)
        {
            return 1 & iState;
        }

        public static int CONAPI_IS_MEDIA_UNACTIVE(int iState)
        {
            return (2 & iState) >> 1;
        }
        public static int CONAPI_IS_IC_SUPPORTED(int iOptions)
        {
            return (16 & iOptions) >> 4;
        }

        public static int CONAPI_IS_IC_UNSUPPORTED(int iOptions)
        {
            return (32 & iOptions) >> 5;
        }
    }
}