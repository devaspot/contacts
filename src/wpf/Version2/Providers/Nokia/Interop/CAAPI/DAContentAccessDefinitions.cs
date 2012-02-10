
//==============================================================================
//* Content Access API 3.2 
//*
//Filename    : DAContentAccessDefinitions.cs
//Description : 
//Version     : 3.2
//
//Copyright (c) 2006,2007 Nokia Corporation.
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

using System.Runtime.InteropServices;
using System;
namespace Synrc
{
    public class DAContentAccessDefinitions
    {

        ////////////////////////////////////////////////////////
        // Content Access operation type defintions 
        //
        // Info: 
        // Operation type definitions are used in notifications 
        // to tell notification type
        //		
        public const int CA_OPERATION_READ = 1;
        public const int CA_OPERATION_WRITE = 2;
        public const int CA_OPERATION_DELETE = 3;
        public const int CA_OPERATION_FIELD_WRITE = 4;
        public const int CA_OPERATION_FIELD_DELETE = 5;
        public const int CA_OPERATION_CREATE_FOLDER = 6;
        public const int CA_OPERATION_DELETE_FOLDER = 7;
        public const int CA_OPERATION_RENAME_FOLDER = 8;

        //////////////////////////////////////////////////////
        // Folder access options 
        //
        // Info: 
        // Definitions for folder and folder content access 
        // rights. These are returned when item paths are 
        // queried from device.
        //
        // Values have following meaning: 
        //		
        // CA_FOLDER_ACCESS_BROWSE		Permission to browse specified folder content
        // CA_FOLDER_ACCESS_CREATE		Permission to create folder to specified folder
        // CA_FOLDER_ACCESS_DELETE		Permission to delete folder from specified folder 
        // CA_FOLDER_ACCESS_SEND 		Permission to use folder for sending items (sms / mms messages)
        // CA_FOLDER_ACCESS_READ_ITEM	Permission to read items from specified folder
        // CA_FOLDER_ACCESS_WRITE_ITEM	Permission to write items to specified folder
        // CA_FOLDER_ACCESS_DELETE_ITEM	Permission to delete items from specified folder

        public const int CA_FOLDER_ACCESS_BROWSE = 1;
        public const int CA_FOLDER_ACCESS_CREATE = 2;
        public const int CA_FOLDER_ACCESS_DELETE = 4;
        public const int CA_FOLDER_ACCESS_SEND = 8;

        public const int CA_FOLDER_ACCESS_READ_ITEM = 256;
        public const int CA_FOLDER_ACCESS_WRITE_ITEM = 512;
        public const int CA_FOLDER_ACCESS_DELETE_ITEM = 1024;

        ////////////////////////////////////////////////////
        // Options for item reading and ID listing 
        //
        // Info: 
        // Defines options for CAReadItem/CAGetIdList method
        // 
        // CA_OPTION_USE_CACHE means that cache will be used 
        // if available. 
        //
        // CA_OPTION_UPDATE_ITEM_STATUS flag is supported for 
        // SMS and MMS target contents. If flag is used, message
        // status is changed from unread to read. 
        //
        // CA_OPTION_USE_ONLY_CACHE flag reads item or ID listing 
        // only from cache DB. Device is not accessed at all when 
        // this option has been selected. 
        // 
        public const int CA_OPTION_USE_CACHE = 1;
        public const int CA_OPTION_UPDATE_ITEM_STATUS = 2;
        public const int CA_OPTION_USE_ONLY_CACHE = 4;

        ////////////////////////////////////////////////////
        // Options for item writing
        //
        // Info: 
        // Defines options for CAWriteItem operation
        // 
        // CA_OPTION_REQUEST_MSG_DELIVERY will request for 
        // delivery reports when sending messages. This option
        // is disabled in other content types. 
        //
        // Currently supported only in Series 40
        // 
        // 
        public const int CA_OPTION_REQUEST_MSG_DELIVERY = 1;

        //=========================================================
        // Structure definitions 
        //=========================================================
        // CA_FOLDER_INFO
        //
        // Description:
        // Folder structure for the specific content type. For some 
        // content types, sub folder structures can be returned also.
        //  
        // Parameters:
        // dwSize				sizeof(CA_FOLDER_INFO), must be set! 
        // dwFolderId			Folder ID 
        // pstrName				Name of the folder 
        // pstrPath				Complete path for the folder
        // dwSubFolderCount		Amount of sub folders in folder
        // pSubFolders			Sub folder array 
        // pParent				Parent folder for current folder
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct CA_FOLDER_INFO
        {
            public int iSize;
            public int iFolderId;
            public int iOptions;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrPath;
            public int iSubFolderCount;
            public IntPtr pSubFolders;
            public IntPtr pParent;
        }

        //=========================================================
        // CA_ITEM_ID
        //
        // Description:
        // ID for the specific content item.
        //  
        // Parameters:
        // dwSize			sizeof(CA_ITEM_ID), must be set! 
        // dwFolderId		Target Folder for the operations, referencing to dwFolderId
        //					member in CA_FOLDER_INFO
        // dwTemporaryID	Temporary operation ID for write and delete operations, can
        //					be used when mapping with operation statuses
        // dwUidLen			Lenght of the UID data 
        // pbUid			UID of the item
        // dwStatus			Status information for the operation
        // 
        [StructLayout(LayoutKind.Sequential)]
        public struct CA_ITEM_ID
        {
            public int iSize;
            public int iFolderId;
            public int iTemporaryID;
            public int iUidLen;
            public IntPtr pbUid;
            public int iStatus;
        }

        //=========================================================
        // CA_ID_LIST
        //
        // Description:
        // Structure containing content specific ID list.
        //  
        // Parameters:
        // dwSize			sizeof(CA_ID_LIST), must be set! 		
        // dwUIDCount		Amount of Id's in pUIDs list.	
        // pUIDs			Actual UID data
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct CA_ID_LIST
        {
            public int iSize;
            public int iUIDCount;
            public IntPtr pUIDs;
        }

        //=========================================================
        // CANotifyCallbackFunction
        //
        //	This is the function prototype of the callback method
        //
        //	DWORD CANotifyCallbackFunction( CAHANDLE hCAHandle, DWORD dwReason,CA_ITEM_ID *pItemID);
        //
        // Parameters:
        // hCAHandle	Handle to opened CA connection
        // dwReason		Reason for notification. 
        // dwParam		For future use, this is set to zero
        // pItemID		Item ID information for notification.
        //	
        public delegate int CANotifyCallbackDelegate(int hCAHandle, int iReason, int iParam, IntPtr pItemID);

        //=========================================================
        // CAOperationCallbackFunction
        //
        //	This is the function prototype of the callback method
        //
        //	DWORD CAOperationCallbackFunction(CAOPERATIONHANDLE hOperHandle,DWORD dwOperation, DWORD dwCurrent, DWORD dwTotalAmount,CA_ITEM_ID * pItemID);
        //
        // Parameters:
        // hOperHandle		Handle to started operation
        // dwOperation		Operation info
        // dwCurrent		Operation progress 
        // dwTotalAmount	Total amount of operations to perform
        // dwStatus			Contains percentage value of the operation progress.
        // pItemID			Item ID information related to this operation
        //		
        public delegate int CAOperationCallbackDelegate(int hOperHandle, int iOperation, int iCurrent, int iTotalAmount, int iStatus, IntPtr pItemID);

        // Reason definitions 
        public const int CA_REASON_ENUMERATING = 1;
        public const int CA_REASON_ITEM_ADDED = 2;
        public const int CA_REASON_ITEM_DELETED = 3;
        public const int CA_REASON_ITEM_UPDATED = 4;
        public const int CA_REASON_ITEM_MOVED = 5;
        public const int CA_REASON_ITEM_REPLACED = 6;
        public const int CA_REASON_CONNECTION_LOST = 7;
        public const int CA_REASON_MSG_DELIVERY = 8;

    }
}
