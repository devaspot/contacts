//-------------------------------------------------------------------------- 
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: FileSyncConfig.cs
//          
//  Description: Provides read/write access to the configuration information 
//    stored locally in a disk file.
//
//-------------------------------------------------------------------------- 

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Synrc
{
    /// <summary>
    /// Provides read and write access to the configuration information
    /// stored locally in a disk file.
    /// </summary>
    public class SynrcSyncConfig
    {
        /// <summary>
        /// The folder that files will be synchronized to.
        /// </summary>
        public string ClientFolder = @"C:\AClient";

        /// <summary>
        /// The folder from which files will be synchronized.
        /// </summary>
        public string ServerFolder = @"C:\AServer";

        /// <summary>
        /// The name given to the synchronization item.
        /// </summary>
        public string SyncItemName = "1FileSync Item";

        /// <summary>
        /// The comment that will be added to the registry entry.
        /// </summary>
        public string RegistryComment = "1FileSync Handler";

        /// <summary>
        /// The name that will be given to the synchronization handler.
        /// </summary>
        public string SyncHandlerName = "1FileSync Handler";

        /// <summary>
        /// The time that the files were synchronized.
        /// </summary>
        public DateTime LastUpdated = DateTime.MinValue;

        //
        // GetConfig
        //
        // Load the configuration data structure from the config file.
        //
        internal static SynrcSyncConfig GetConfig()
        {
            FileStream fs = null;
            SynrcSyncConfig config = null;
            try
            {
                if (File.Exists(GetConfigFilePath()))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(SynrcSyncConfig));
                    fs = new FileStream(GetConfigFilePath(), FileMode.Open, FileAccess.Read, FileShare.Read);
                    config = (SynrcSyncConfig) xmlSerializer.Deserialize(fs);
                }
                else
                {
                    // If no config found, use default values.
                    config = new SynrcSyncConfig();  
                }
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
            return config;
        }
    
        //
        // WriteConfig
        //
        // Save configuration data to disk.
        //
        internal void WriteConfig()
        {
            StreamWriter sw = null;
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(SynrcSyncConfig));
                sw = new StreamWriter(GetConfigFilePath());
                xmlSerializer.Serialize(sw, this);
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
            }
        }
    
        //
        // GetConfigFilePath                                                             
        //
        // Find the location of the dll that is registered to the CLSID
        //  in order to locate the configuration file no matter
        //  whether the assembly was invoked directly or through SyncMgr.
        //
        private static string GetConfigFilePath()
        {
            RegistryKey rk = Registry.ClassesRoot;
            rk = rk.OpenSubKey(@"CLSID\{" + SynrcSyncMgrHandler.SyncHandlerId + @"}\InprocServer32");
            string s = (string) rk.GetValue("CodeBase");

            // Convert from URI to UNC format.
            Uri uri = new Uri(s);
            s = uri.LocalPath;

            s = Path.GetDirectoryName(s);
            return Path.Combine(s, "FileSync.config");
        }
    }
}