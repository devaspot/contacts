/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Standard
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Microsoft.Win32;
    using Interop;

    internal static class RegistryUtil
    {
        private static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr(unchecked((int)0x80000000));
        private static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(unchecked((int)0x80000001));
        private static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));
        private static readonly IntPtr HKEY_USERS = new IntPtr(unchecked((int)0x80000003));
        private static readonly IntPtr HKEY_PERFORMANCE_DATA = new IntPtr(unchecked((int)0x80000004));
        private static readonly IntPtr HKEY_CURRENT_CONFIG = new IntPtr(unchecked((int)0x80000005));
        private static readonly IntPtr HKEY_DYN_DATA = new IntPtr(unchecked((int)0x80000006));

        public static IntPtr GetNativeRegistryHive(RegistryHive hive)
        {
            switch (hive)
            {
                case RegistryHive.ClassesRoot: return HKEY_CLASSES_ROOT;
                case RegistryHive.CurrentConfig: return HKEY_CURRENT_CONFIG;
                case RegistryHive.CurrentUser: return HKEY_CURRENT_USER;
                case RegistryHive.DynData: return HKEY_DYN_DATA;
                case RegistryHive.LocalMachine: return HKEY_LOCAL_MACHINE;
                case RegistryHive.PerformanceData: return HKEY_PERFORMANCE_DATA;
                case RegistryHive.Users: return HKEY_USERS;
                default:
                    Assert.Fail();
                    throw new ArgumentException("Invalid registry hive", "hive");
            }
        }
    }

    internal class RegistryListener : IDisposable
    {
        private readonly IntPtr _hive;
        private readonly string _keyName;
        private Thread _backgroundListener;
        private ManualResetEvent _stopEvent;

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "err", Justification="Value used in Debug builds")]
        private static void _SafeRegCloseKey(ref IntPtr hKey)
        {
            IntPtr ptr = hKey;
            hKey = IntPtr.Zero;
            if (IntPtr.Zero != ptr)
            {
                Win32Error err = NativeMethods.RegCloseKey(ptr);
                Assert.AreEqual(Win32Error.ERROR_SUCCESS, err);
            }
        }

        public event Action<RegistryListener> KeyChanged;

        public RegistryListener(RegistryHive registryHive, string subKey)
        {
            _hive = RegistryUtil.GetNativeRegistryHive(registryHive);
            _keyName = subKey;

            _stopEvent = new ManualResetEvent(false);
            _backgroundListener = new Thread(_BackgroundListenerTask)
            {
                IsBackground = true
            };
            _backgroundListener.Start();
        }

        private void _BackgroundListenerTask()
        {
            const RegNotifyChangeFilter filter = RegNotifyChangeFilter.All;

            IntPtr hKey = IntPtr.Zero;
            try
            {
                while (!_stopEvent.WaitOne(0, true))
                {
                    RegDisposition disposition;
                    _SafeRegCloseKey(ref hKey);
                    Win32Error err = NativeMethods.RegCreateKeyEx(_hive, _keyName, 0, null, RegOptions.NonVolatile, RegSecurityAndAccessMask.KeyRead, IntPtr.Zero, out hKey, out disposition);
                    // On success the disposition should be either a new key or an opened key.
                    Assert.Implies(Win32Error.ERROR_SUCCESS == err, RegDisposition.CreatedNewKey == disposition || RegDisposition.OpenedExistingKey == disposition);
                    Assert.AreEqual(Win32Error.ERROR_SUCCESS, err);
                    ((HRESULT)err).ThrowIfFailed();
                    
                    var notifyEvent = new AutoResetEvent(false);
                    var waitHandles = new WaitHandle[] { notifyEvent, _stopEvent };
                    while (!_stopEvent.WaitOne(0, true))
                    {
                        err = NativeMethods.RegNotifyChangeKeyValue(hKey, true, filter, notifyEvent.SafeWaitHandle, true);
                        if (err == Win32Error.ERROR_KEY_DELETED)
                        {
                            // If the root key was deleted and we're listening for changes under the value then re-create it.
                            // We create at the beginning of this function if it doesn't exist anyways.
                            break;
                        }
                        ((HRESULT)err).ThrowIfFailed();
                        if (WaitHandle.WaitAny(waitHandles) == 0)
                        {
                            Action<RegistryListener> localKeyChanged = KeyChanged;
                            if (null != localKeyChanged)
                            {
                                localKeyChanged(this);
                            }
                        }
                    }
                }
            }        
            finally
            {
                _SafeRegCloseKey(ref hKey);
            }
        }

        #region IDisposable Pattern

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_stopEvent")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Thread thread = _backgroundListener;
                _backgroundListener = null;
                if (thread != null)
                {
                    _stopEvent.Set();
                    thread.Join();
                }
                Utility.SafeDispose(ref _stopEvent);
            }
        }

        #endregion

    }
}
