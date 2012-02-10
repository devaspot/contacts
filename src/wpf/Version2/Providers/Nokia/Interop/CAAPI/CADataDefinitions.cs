//==============================================================================
// PC Connectivity API 3.2
//
// Filename    : CADataDefinitions.cs
// Description : Content acces data definitions 
// Version     : 3.0
//
// Copyright (c) 2007 Nokia Corporation.
// This software, including but not limited to documentation and any related 
// computer programs ("Software"), is protected by intellectual property rights 
// of Nokia Corporation and/or its licensors. All rights are reserved. By using 
// the Software you agree to the terms and conditions hereunder. If you do not 
// agree you must cease using the software immediately.
// Reproducing, disclosing, modifying, translating, or distributing any or all 
// of the Software requires the prior written consent of Nokia Corporation. 
// Nokia Corporation retains the right to make changes to the Software at any 
// time without notice.
//
// A copyright license is hereby granted to use of the Software to make, publish, 
// distribute, sub-license and/or sell new Software utilizing this Software. 
// The Software may not constitute the primary value of any new software utilizing 
// this software. No other license to any other intellectual property rights of 
// Nokia or a third party is granted. 
// 
// THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS 
// OR IMPLIED, INCLUDING WITHOUT LIMITATION, ANY WARRANTY OF NON-INFRINGEMENT, 
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. IN NO EVENT SHALL
// NOKIA CORPORATION BE LIABLE FOR ANY DIRECT, INDIRECT, SPECIAL, INCIDENTAL, 
// OR CONSEQUENTIAL LOSS OR DAMAGES, INCLUDING BUT NOT LIMITED TO, LOST PROFITS 
// OR REVENUE, LOSS OF USE, COST OF SUBSTITUTE PROGRAM, OR LOSS OF DATA OR EQUIPMENT 
// ARISING OUT OF THE USE OR INABILITY TO USE THE MATERIAL, EVEN IF 
// NOKIA CORPORATION HAS BEEN ADVISED OF THE LIKELIHOOD OF SUCH DAMAGES OCCURRING.
// ==============================================================================

namespace Synrc
{
    using System.Runtime.InteropServices;
    using System;

    public class CADataDefinitions
    {

        //=========================================================
        //Constants and definitions for API 
        //=========================================================

        //////////////////////////////////////////////////////
        // Available CA connection targets
        //
        // Info: 
        // PIM connection targets
        // 
        // 
        public const int CA_TARGET_CONTACTS = 1;
        public const int CA_TARGET_CALENDAR = 2;
        public const int CA_TARGET_NOTES = 3;
        public const int CA_TARGET_SMS_MESSAGES = 4;
        public const int CA_TARGET_MMS_MESSAGES = 5;
        public const int CA_TARGET_BOOKMARKS = 6;

        //////////////////////////////////////////////////////
        // Available data formats 
        //
        // Info: 
        // Data formats. 	
        // 
        public const int CA_DATA_FORMAT_STRUCT = 1;
        public const int CA_DATA_FORMAT_VERSIT = 2;


        //////////////////////////////////////////////////////
        //
        // Info
        // Macros for data handling in CA_DATA_MSG 
        //
        public static int CA_GET_DATA_FORMAT(int Info)
        {
            return Info & 15;
        }

        public static int CA_GET_DATA_CODING(int Info)
        {
            return Info & 240;
        }

        public static int CA_GET_MESSAGE_STATUS(int Info)
        {
            return (Info & 3840) >> 8;
        }

        public static int CA_GET_MESSAGE_TYPE(int Info)
        {
            return (Info & 61440) >> 12;
        }

        public static void CA_SET_DATA_FORMAT(ref int Info, int Value)
        {
            Info = Info | (Value & 15);
        }

        public static void CA_SET_DATA_CODING(ref int Info, int Value)
        {
            Info = Info | (Value & 240);
        }

        public static void CA_SET_MESSAGE_STATUS(ref int Info, int Value)
        {
            Info = Info | ((Value & 15) << 8);
        }

        public static void CA_SET_MESSAGE_TYPE(ref int Info, int Value)
        {
            Info = Info | ((Value & 15) << 12);
        }

        /////////////////////////////////////////////////////
        //
        // Info
        // Macros for data dwRecurrence in CA_DATA_CALENDAR
        //
        public static int CA_GET_RECURRENCE(int iInfo)
        {
            return iInfo & 255;
        }

        public static void CA_SET_RECURRENCE(ref int iInfo, int iValue)
        {
            iInfo = iInfo | iValue;
        }

        public static int CA_GET_RECURRENCE_INTERVAL(int iInfo)
        {
            return (iInfo >> 8) & 255;
        }

        public static void CA_SET_RECURRENCE_INTERVAL(ref int iInfo, int iValue)
        {
            iInfo = iInfo | (iValue << 8);
        }

        //////////////////////////////////////////////////////
        // Input data format definitions
        //
        // Info: 
        // This format is used in CA_DATA_MSG structure's 
        // dwInfoField parameter to tell the format of the data
        // in data buffer
        //
        // Also used in CA_DATA_VERSIT structure's dwInfoField	
        // to inform about format of the data in structure
        // 
        public const int CA_DATA_FORMAT_UNICODE = 1;
        public const int CA_DATA_FORMAT_DATA = 2;

        //////////////////////////////////////////////////////
        // Input data coding definitions 
        //
        // Info: 
        // This format is used in CA_DATA_MSG structure 
        // to inform about the coding of the data 
        // 
        public const int CA_DATA_CODING_7BIT = 16;
        public const int CA_DATA_CODING_8BIT = 32;
        public const int CA_DATA_CODING_UNICODE = 64;

        //////////////////////////////////////////////////////
        // Message type definitions
        //
        // Info: 
        // Message type definitions 
        // 
        public const int CA_SMS_DELIVER = 1;
        public const int CA_SMS_SUBMIT = 3;

        ////////////////////////////////////////////////////
        // Message folder definitions
        //
        // Info: 
        // Message folder definitions. 
        // 
        // Folder ID definitions 
        public const int CA_MESSAGE_FOLDER_INBOX = 1;
        public const int CA_MESSAGE_FOLDER_OUTBOX = 2;
        public const int CA_MESSAGE_FOLDER_SENT = 3;
        public const int CA_MESSAGE_FOLDER_ARCHIVE = 4;
        public const int CA_MESSAGE_FOLDER_DRAFTS = 5;
        public const int CA_MESSAGE_FOLDER_TEMPLATES = 6;
        public const int CA_MESSAGE_FOLDER_USER_FOLDERS = 16;

        ////////////////////////////////////////////////////
        // Phonebook memory definitions 
        //
        // Info: 
        // Phonebook memory definitions 
        // 
        // Folder ID definitions 
        public const int CA_PHONEBOOK_MEMORY_PHONE = 1;
        public const int CA_PHONEBOOK_MEMORY_SIM = 2;

        //////////////////////////////////////////////////////
        // Message status definitions ..
        //
        // Info: 
        // SMS Message status definitions .. 
        // 
        // Message has been sent 
        public const int CA_MESSAGE_STATUS_SENT = 2;
        // Message hasn't been read 
        public const int CA_MESSAGE_STATUS_UNREAD = 4;
        // Message has been read 
        public const int CA_MESSAGE_STATUS_READ = 5;
        // Message is a draft 
        public const int CA_MESSAGE_STATUS_DRAFT = 6;
        // Message is pending 
        public const int CA_MESSAGE_STATUS_PENDING = 7;
        // Message has been delivered 
        public const int CA_MESSAGE_STATUS_DELIVERED = 9;
        // Message is being sent
        public const int CA_MESSAGE_STATUS_SENDING = 12;

        //////////////////////////////////////////////////////
        // Address type definitions 
        //
        // Info: 
        // Type of address passed in address structure
        // 
        public const int CA_MSG_ADDRESS_TYPE_EMAIL = 1;
        public const int CA_MSG_ADDRESS_TYPE_NUMBER = 2;

        //////////////////////////////////////////////////////
        // Calendar item type definitions 
        //
        // Info: 
        // Defines different calendar items. Used in CA_DATA_CALENDAR in dwInfoField.
        // 
        public const int CA_CALENDAR_ITEM_MEETING = 1;
        public const int CA_CALENDAR_ITEM_CALL = 2;
        public const int CA_CALENDAR_ITEM_BIRTHDAY = 3;
        public const int CA_CALENDAR_ITEM_MEMO = 4;
        public const int CA_CALENDAR_ITEM_REMINDER = 5;
        public const int CA_CALENDAR_ITEM_NOTE = 6;
        public const int CA_CALENDAR_ITEM_TODO = 7;


        ////////////////////////////////////////////////////////
        // Field type definitions 
        //
        // Info: 
        // Field type values for data items
        //	
        // For contacts
        // Personal information
        public const int CA_FIELD_TYPE_CONTACT_PI = 1;
        // Number information
        public const int CA_FIELD_TYPE_CONTACT_NUMBER = 2;
        // Address information
        public const int CA_FIELD_TYPE_CONTACT_ADDRESS = 3;
        // General information
        public const int CA_FIELD_TYPE_CONTACT_GENERAL = 4;
        // For calendar
        // Calendar item
        public const int CA_FIELD_TYPE_CALENDAR = 16;

        ////////////////////////////////////////////////////////
        //Recurrence values for calendar items 
        //
        //Info: 
        //Recurrence values to be used in calendar interfaces
        //		
        public const int CA_CALENDAR_RECURRENCE_NONE = 0;
        public const int CA_CALENDAR_RECURRENCE_DAILY = 1;
        public const int CA_CALENDAR_RECURRENCE_WEEKLY = 2;
        public const int CA_CALENDAR_RECURRENCE_MONTHLY = 3;
        public const int CA_CALENDAR_RECURRENCE_YEARLY = 4;

        //////////////////////////////////////////////////////
        // Calendar TODO item priority values 
        //
        // Info: 
        //
        public const int CA_CALENDAR_TODO_PRIORITY_HIGH = 1;
        public const int CA_CALENDAR_TODO_PRIORITY_NORMAL = 2;
        public const int CA_CALENDAR_TODO_PRIORITY_LOW = 3;

        ////////////////////////////////////////////////////
        // Calendar TODO item status definitions
        //
        // Info: 
        //
        public const int CA_CALENDAR_TODO_STATUS_NEEDS_ACTION = 1;
        public const int CA_CALENDAR_TODO_STATUS_COMPLETED = 2;

        ////////////////////////////////////////////////////
        // Calendar Alarm State value definitions
        //
        // Info: 
        // CA_CALENDAR_ALARM_SILENT value is not supported 
        // in S60 devices.
        //
        public const int CA_CALENDAR_ALARM_NOT_SET = 1;
        public const int CA_CALENDAR_ALARM_SILENT = 2;
        public const int CA_CALENDAR_ALARM_WITH_TONE = 3;

        //////////////////////////////////////////////////////
        // Field sub type definitions 
        //
        // Info: 
        //	Field sub type definitions for data items.
        // ___Description_________|  ___Valid CA_DATA_ITEM member_______	
        // Personal information	  |															|
        // Name field			  |     pstrText
        public const int CA_FIELD_SUB_TYPE_NAME = 1;
        // First name			  |	    pstrText
        public const int CA_FIELD_SUB_TYPE_FN = 2;
        // Midle name			  |	    pstrText
        public const int CA_FIELD_SUB_TYPE_MN = 3;
        // Last name			  |	    pstrText
        public const int CA_FIELD_SUB_TYPE_LN = 4;
        // Title				  |	    pstrText
        public const int CA_FIELD_SUB_TYPE_TITLE = 5;
        // Suffix				  |	    pstrText
        public const int CA_FIELD_SUB_TYPE_SUFFIX = 6;
        // Company				  |	    pstrText
        public const int CA_FIELD_SUB_TYPE_COMPANY = 7;
        // Job title			  |	    pstrText
        public const int CA_FIELD_SUB_TYPE_JOB_TITLE = 8;
        // Birthday				  |	    pCustomData as CA_DATA_DATE
        public const int CA_FIELD_SUB_TYPE_BIRTHDAY = 9;
        // Picture				  |	    pCustomData as CA_DATA_PICTURE
        public const int CA_FIELD_SUB_TYPE_PICTURE = 10;
        // Nickname				  |	    pstrText
        public const int CA_FIELD_SUB_TYPE_NICKNAME = 11;
        // Formal name			  | 	pstrText
        public const int CA_FIELD_SUB_TYPE_FORMAL_NAME = 12;
        // Pronunciation field	  | 	pstrText
        public const int CA_FIELD_SUB_TYPE_GIVEN_NAME_PRONUNCIATION = 13;
        // Pronunciation field	  | 	pstrText
        public const int CA_FIELD_SUB_TYPE_FAMILY_NAME_PRONUNCIATION = 14;
        // Pronunciation field	  | 	pstrText
        public const int CA_FIELD_SUB_TYPE_COMPANY_NAME_PRONUNCIATION = 15;

        // Numbers
        // Telephone			  |	    pstrText
        public const int CA_FIELD_SUB_TYPE_TEL = 32;
        // Home number			  | 	pstrText
        public const int CA_FIELD_SUB_TYPE_TEL_HOME = 33;
        // Work number			  |	    pstrText
        public const int CA_FIELD_SUB_TYPE_TEL_WORK = 34;
        // Preferred number		  |	    pstrText
        public const int CA_FIELD_SUB_TYPE_TEL_PREF = 35;
        // Car number			  | 	pstrText
        public const int CA_FIELD_SUB_TYPE_TEL_CAR = 36;
        // Data number			  | 	pstrText
        public const int CA_FIELD_SUB_TYPE_TEL_DATA = 37;
        // Pager				  | 	pstrText
        public const int CA_FIELD_SUB_TYPE_PAGER = 48;

        // Mobile				 |	    pstrText
        public const int CA_FIELD_SUB_TYPE_MOBILE = 64;
        // Mobile				 |      pstrText
        public const int CA_FIELD_SUB_TYPE_MOBILE_HOME = 65;
        // Mobile				 |      pstrText	
        public const int CA_FIELD_SUB_TYPE_MOBILE_WORK = 66;
        // Fax					 |	    pstrText
        public const int CA_FIELD_SUB_TYPE_FAX = 80;
        // Fax					 |      pstrText
        public const int CA_FIELD_SUB_TYPE_FAX_HOME = 81;
        // Fax					 |      pstrText
        public const int CA_FIELD_SUB_TYPE_FAX_WORK = 82;

        // Video call number	 |      pstrText
        public const int CA_FIELD_SUB_TYPE_VIDEO = 96;
        // Video call number	 |      pstrText
        public const int CA_FIELD_SUB_TYPE_VIDEO_HOME = 97;
        // Video call number	 |      pstrText
        public const int CA_FIELD_SUB_TYPE_VIDEO_WORK = 98;
        // Voice Over IP		 |      pstrText
        public const int CA_FIELD_SUB_TYPE_VOIP = 112;
        // Voice Over IP		 |      pstrText
        public const int CA_FIELD_SUB_TYPE_VOIP_HOME = 113;
        // Voice Over IP		 |      pstrText
        public const int CA_FIELD_SUB_TYPE_VOIP_WORK = 114;

        // Addresses
        // Postal address		 |	    pCustomData as CA_DATA_POSTAL_ADDRESS
        public const int CA_FIELD_SUB_TYPE_POSTAL = 256;
        // Business address	     |	    pCustomData as CA_DATA_POSTAL_ADDRESS
        public const int CA_FIELD_SUB_TYPE_POSTAL_BUSINESS = 257;
        // Private address		 |	    pCustomData as CA_DATA_POSTAL_ADDRESS
        public const int CA_FIELD_SUB_TYPE_POSTAL_PRIVATE = 258;
        // Email address		 |	    pstrText
        public const int CA_FIELD_SUB_TYPE_EMAIL = 259;
        // Email address		 |	    pstrText
        public const int CA_FIELD_SUB_TYPE_EMAIL_HOME = 260;
        // Email address		 |	    pstrText
        public const int CA_FIELD_SUB_TYPE_EMAIL_WORK = 261;
        // Web address			 |	    pstrText
        public const int CA_FIELD_SUB_TYPE_WEB = 272;
        // Web address			 |	    pstrText
        public const int CA_FIELD_SUB_TYPE_WEB_HOME = 273;
        // Web address			 |	    pstrText	
        public const int CA_FIELD_SUB_TYPE_WEB_WORK = 274;
        // PTT address			 |	    pstrText
        public const int CA_FIELD_SUB_TYPE_PTT = 288;
        // SIP for Video sharing |	    pstrText
        public const int CA_FIELD_SUB_TYPE_SIP_FOR_VIDEO = 289;
        // SWIS				     |      pstrText
        public const int CA_FIELD_SUB_TYPE_SWIS = 304;

        // General fields
        // Note field			 |	    pstrText
        public const int CA_FIELD_SUB_TYPE_NOTE = 512;
        // DTMF field			 |	    pstrText
        public const int CA_FIELD_SUB_TYPE_DTMF = 513;
        // UID field			 |	    pstrText
        public const int CA_FIELD_SUB_TYPE_UID = 514;
        // Village user ID	     |	    pstrText
        public const int CA_FIELD_SUB_TYPE_WIRELESS_VILLAGE = 515;

        // Calandar sub types
        // Description			 |	    pstrText
        public const int CA_FIELD_SUB_TYPE_DESCRIPTION = 768;
        // Location				 |	    pstrText
        public const int CA_FIELD_SUB_TYPE_LOCATION = 769;
        // Generic Item data	 |	    pstrText / dwData
        public const int CA_FIELD_SUB_TYPE_ITEM_DATA = 770;
        // Todo priotiy			 |	    dwData
        public const int CA_FIELD_SUB_TYPE_TODO_PRIORITY = 771;
        // Todo status			 |	    dwData
        public const int CA_FIELD_SUB_TYPE_TODO_STATUS = 772;

        // CA_FIELD_SUB_TYPE_ITEM_DATA	usage varies among different calendar item types.
        // In CA_CALENDAR_ITEM_MEETING	Meeting detail information in pstrText parameter
        // in CA_CALENDAR_ITEM_CALL,		Phone number information in pstrText
        // In CA_CALENDAR_ITEM_BIRTHDAY	Birth year information in dwData paramter
        //////////////////////////////////////////////////////
        // CA_DATA_DATE
        //
        // Description:
        // This structure contains date information in separated 
        // members.
        //  
        // Parameters:
        //	dwSize			Size of the structure (must be set!)
        //	wYear			Year information in four digit format (2005) 
        //	bMonth			Month information (1 - 12, january = 1) 
        //	bDay			Day of month (1-31)
        //	bHour			Hours after midnight (0-23)			
        //	bMinute			Minutes after hour (0-59)
        //	bSecond			Seconds after minute (0-59)
        //  lTimeZoneBias	Time zone bias in minutes (+120 for GMT+0200)
        //  lBias			Daylight bias.This contains value for offset in minutes 
        //					for time zone, for example +60. 

        [StructLayout(LayoutKind.Sequential)]
        public struct CA_DATA_DATE
        {
            public int iSize;
            public UInt16 wYear;
            public byte bMonth;
            public byte bDay;
            public byte bHour;
            public byte bMinute;
            public byte bSecond;
            public Int32 lTimeZoneBias;
            public Int32 lBias;
        }

        ///////////////////////////////////////////////////////
        // CA_DATA_ADDRESS
        //
        // Description:
        // This structure contains address information of the message. 
        //  
        // Parameters:
        //	dwSize			Size of the structure (must be set!)
        //  dwAddressInfo	Contains type information of address 
        //					See "Address types" definition on top
        //  pstrAddress		Address data in unicode format
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct CA_DATA_ADDRESS
        {
            public int iSize;
            public int iAddressInfo;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrAddress;
        }

        ///////////////////////////////////////////////////////
        // CA_DATA_POSTAL_ADDRESS
        //
        // Description:
        // This structure contains postal address information. 
        //  
        // Parameters:
        //	dwSize				Size of the structure (must be set!)
        //	pstrPOBox			PO Box address
        //	pstrStreet			Street address
        //	pstrPostalCode		Postal code information
        //	pstrCity			City 
        //	pstrState			State
        //	pstrCountry			Country 
        //	pstrExtendedData	Extended address information
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct CA_DATA_POSTAL_ADDRESS
        {
            public int iSize;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrPOBox;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrStreet;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrPostalCode;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrCity;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrState;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrCountry;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrExtendedData;
        }

        ///////////////////////////////////////////////////////
        // CA_DATA_PICTURE
        //
        // Description:
        // This structure contains picture information and data
        //  
        // Parameters:
        //	dwSize			Size of the structure (must be set!)
        //	pstrFileName	Picture file name/picture format  
        //	dwDataLen		Picture data buffer length
        //	pbData			picture data
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct CA_DATA_PICTURE
        {
            public int iSize;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrFileName;
            public int iDataLen;
            public IntPtr pbData;
        }

        ///////////////////////////////////////////////////////
        // CA_DATA_ITEM
        //
        // Info: 
        // Generic data structure used with contacts and calendar item
        // dwFieldType defines the format of the data (which union member
        // contains valid data) . 
        //
        // Parameters:
        //	dwSize			Size of the structure (must be set!)
        //	dwFieldId		Field specific ID used in field access operations in content Access API. 
        //					Identifies certain field in content item. This is returned in CAReadItem. 
        //	dwFieldType		For different field types:
        //					see "Field type definitions " on top of this header
        //	dwFieldSubType	For different field sub types:
        //					see "Field sub type definitions" on top of this header
        //	ItemData		According to defined field sub type parameter, this union is 
        //					filled with valid data. See details in table
        //					"Field sub type definitions "
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct CA_DATA_ITEM
        {
            public int iSize;
            public int iFieldId;
            public int iFieldType;
            public int iFieldSubType;
            public IntPtr pCustomData;
            //union
            //{
            //	DWORD	dwData;
            //	WCHAR*	pstrText;			
            //	LPVOID	pCustomData;
            //} ItemData;
        }

        ///////////////////////////////////////////////////////
        // CA_DATA_CONTACT
        //
        // Info: 
        // Contact data item. 
        //
        // Parameters:
        //	dwSize			Size of the structure (must be set!)
        //	bPICount		Amount of personal information fields 
        //	pPIFields		Personal information field data
        //	bNumberCount	Amount of contact number information fields 
        //	pNumberFields	Contact number information data
        //	bAddressCount	Amount of address fields 
        //	pAddressFields	Address field data 
        //	bGeneralCount	Amount of general fields 
        //	pGeneralFields	General field data 
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct CA_DATA_CONTACT
        {
            public int iSize;
            public byte bPICount;
            public IntPtr pPIFields;
            public byte bNumberCount;
            public IntPtr pNumberFields;
            public byte bAddressCount;
            public IntPtr pAddressFields;
            public byte bGeneralCount;
            public IntPtr pGeneralFields;
        }

        ///////////////////////////////////////////////////////
        // CA_DATA_CALENDAR
        //
        // Info: 
        // Calendar item structure
        //
        // Parameters:
        //	dwSize				Size of the structure (must be set!)
        //	dwInfoField			Type of the calendar item
        //	noteStartDate		Start date of the note
        //	noteEndDate			End date of the note
        //   dwAlarmState		    For possible values , see "Calendar Alarm State value definitions"
        //	noteAlarmTime		Alarm time of the note (defines also if no alarm for note)
        //	dwRecurrence		Requrrency of the note
        //	recurrenceEndDate	End date if note has requrrence
        //	bItemCount			Amount of items belonging to note
        //	pDataItems			Calendar data items
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct CA_DATA_CALENDAR
        {
            public int iSize;
            public int iInfoField;
            public CA_DATA_DATE noteStartDate;
            public CA_DATA_DATE noteEndDate;
            public int iAlarmState;
            public CA_DATA_DATE noteAlarmTime;
            public int iRecurrence;
            public CA_DATA_DATE recurrenceEndDate;
            public byte bItemCount;
            public IntPtr pDataItems;
            public byte bRecurrenceExCount;
            public IntPtr pRecurrenceExceptions;
        }

        ///////////////////////////////////////////////////////
        // CA_DATA_MSG
        //
        // Description:
        // CA data stucture for message data
        // 
        // 
        //  
        // Parameters:
        //	dwSize			Size of the structure (must be set!), use sizeof(CA_DATA_MSG)
        //  dwInfoField		Contains status information of message (used encoding,input data 
        //					format, message status 
        //					In dwInfoField following definitions are used now : 
        //					  = &H  = &H value defines format of input data
        //					  = &HX0 value defines how input data is passed to the phone
        //					  = &H  = &H00 value defines status of the message (read / unread...)
        //					  = &HX000 value defines message type (submit / deliver )
        // 			
        //  dwDataLength	size of pbData byte array 
        //  pbData			Actual user data 
        //  bAddressCount	Amount of addresses included in pAddress array  (currently only one address supported)
        //	pAddress		Pointer to addesses.
        //	messageDate		This struct is used when generating time stamps for the message
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct CA_DATA_MSG
        {
            public int iSize;
            public int iInfoField;
            public int iDataLength;
            public IntPtr pbData;
            public byte bAddressCount;
            public IntPtr pAddress;
            public CA_DATA_DATE messageDate;
        }

        ///////////////////////////////////////////////////////
        // CA_DATA_VERSIT 
        //
        // Description:
        // CA data stucture for versit data
        //  
        // Parameters:
        //	dwSize			Size of the structure (must be set!)
        //	dwInfoField		Contains status information of versit data object
        //					In dwInfoField following definitions are used now : 
        //					  = &H  = &H value defines format of input data, see "Input data format definitions"
        //	dwDataLenght	Lenght of the data
        //	pbVersitObject	Pointer to versit object data.
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct CA_DATA_VERSIT
        {
            public int iSize;
            public int iInfoField;
            public int iDataLength;
            public IntPtr pbVersitObject;
        }


        //=========================================================
        // CA_MMS_DATA
        //
        // Description:
        // This structure defines MMS message data
        //  
        // Parameters:
        //	dwSize			The total size of this structure in bytes (sizeof(CA_MMS_DATA)).
        //	dwInfoField		Contains status information of message (message status)
        //					In dwInfoField following definitions are used now : 
        //					  = &H  = &H00 value defines status of the message (read / unread...)	
        //  bAddressCount	Amount of addresses in message (currently only one address supported)
        //					Address is returned when reading the message, but in the writing address
        //					is included in binary MMS
        //  pAddress		Address array 
        //	messageDate		Message date
        //	dwDataLength	Size of MMS data buffer
        //	pbData			Actual MMS data 	
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct CA_MMS_DATA
        {
            public int iSize;
            public int iInfoField;
            public byte bAddressCount;
            public IntPtr pAddress;
            public CA_DATA_DATE messageDate;
            public int iDataLength;
            public IntPtr pbData;
        }

        //=========================================================
        // CA_DATA_NOTE
        //
        // Description:
        // This structure defines calendar note data 
        //  
        // Parameters:
        //	dwSize			The total size of this structure in bytes (sizeof(CA_DATA_ITEM)).
        //	pstrText		Note text
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct CA_DATA_NOTE
        {
            public int iSize;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrText;
        }

        //=========================================================
        // CA_DATA_BOOKMARK
        //
        // Info: 
        // Bookmark data structure
        //
        // Parameters:
        //	dwSize			Size of the structure (must be set!)
        //	pstrTitle		Title of bookmark
        //	pstrBookMarkUrl	Bookmark URL 
        //   pstrUrlShortcut Url shortcut
        //	
        [StructLayout(LayoutKind.Sequential)]
        public struct CA_DATA_BOOKMARK
        {
            public int iSize;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrTitle;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrBookMarkUrl;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pstrUrlShortcut;
        }
    }
}
