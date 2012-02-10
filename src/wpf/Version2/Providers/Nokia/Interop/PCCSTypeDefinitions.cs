//==============================================================================
// PC Connectivity API 3.2
//
// Filename    : PCCSTypeDefinitions.cs
// Description : PC Connectivity Solution Type Definitions for all APIs
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
//
using System;
namespace Synrc
{
    public class PCCSTypeDefinitions
    {

        // Values used in API notification registeration methods
        public const short API_REGISTER = 16;
        public const short API_UNREGISTER = 32;

        // Media types used in APIs 
        public const short API_MEDIA_ALL = 1;
        public const short API_MEDIA_IRDA = 2;
        public const short API_MEDIA_SERIAL = 4;
        public const short API_MEDIA_BLUETOOTH = 8;
        public const short API_MEDIA_USB = 16;

        // Type definition for API_DATE_TIME
        public struct API_DATE_TIME
        {
            // Size of the structure. Must be sizeof(API_DATE_TIME).
            public int iSize;
            public UInt16 wYear;
            public byte bMonth;
            public byte bDay;
            public byte bHour;
            public byte bMinute;
            public byte bSecond;
            // Time zone bias in minutes (+120 for GMT+0200)
            public Int32 lTimeZoneBias;
            // Daylight bias
            public Int32 lBias;
        }
    }
}
