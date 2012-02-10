/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;
    using System.Windows.Threading;
    using Standard;

    using Timer = System.Windows.Threading.DispatcherTimer;
    using System.Windows;

    internal delegate void ContactWatcherEventCallback(ContactCollectionChangedEventArgs e);

    // ContactWatcherSingular is only responsible for a single extension.
    // This class aggregates multiple watchers, one for each extension we want to watch.
    sealed internal class ContactWatcher : IDisposable
    {
        private ContactWatcherSingular[] _watchers;
        private static readonly ContactTypes[] _Types = new[]
            { 
                ContactTypes.Organization, 
                ContactTypes.Contact, 
                ContactTypes.Group 
            };

        public ContactWatcher(Dispatcher dispatcher, ContactLoader loader, string directory, ContactWatcherEventCallback callback)
        {
            // Assert here rather than dynamically generate these arrays.  This class should include all the supported types.
            Assert.AreEqual(Contact.GetExtensionsFromType(ContactTypes.All).Split('|').Length, _Types.Length);

            var watchers = new ContactWatcherSingular[_Types.Length];
            try
            {
                for (int i = 0; i < _Types.Length; ++i)
                {
                    watchers[i] = new ContactWatcherSingular(dispatcher, loader, directory, _Types[i], callback);
                }

                _watchers = watchers;
                watchers = null;
            }
            finally
            {
                if (null != watchers)
                {
                    foreach (ContactWatcherSingular watcher in watchers)
                    {
                        if (null != watcher)
                        {
                            watcher.Dispose();
                        }
                    }
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (null != _watchers)
            {
                foreach (ContactWatcherSingular watcher in _watchers)
                {
                    if (null != watcher)
                    {
                        watcher.Dispose();
                    }
                }
                _watchers = null;
            }
        }

        #endregion

        sealed private class ContactWatcherSingular : IDisposable
        {
            private struct ContactInfo
            {
                public string Id;
                public DateTime LastWrite;
                // Consider: public Guid HashValue;
            }

            #region Callback Delegate Declarations
            delegate void _FileChanged(string path);
            delegate void _FileRenamed(string oldPath, string newPath);
            #endregion

            #region Fields
            private const int _MaxRetryCount = 5;
            private const int _TimerMsDelay = 2000;

            private readonly Dispatcher _dispatcher;
            private readonly ContactLoader _loader;
            private readonly string _rootDirectory;
            private readonly string _findExtension;
            private readonly ContactWatcherEventCallback _notify;

            // Listen for changes to the file system under _rootDirectory.
            private FileSystemWatcher _fileWatch;
            // Mapping of file names to ContactIds.
            private readonly Dictionary<string, ContactInfo> _knownContacts;
            // Sometimes an update can't be processed because the file has immediately been moved.
            // Cache the need to process the update until we can open the file.
            // We'll only ever hold one of these at a time, but this prevents the general case of
            // an update/move from appearing as a remove/add.
            private string _pendingUpdateFile;
            // to avoid delays for pending changes, queue a timer to signal an appropriate time
            // to retry.
            private Timer _timer;
            // ensure we don't end up in a perpetual bad loop on some file.
            private int _retryCount;
            private bool _stopProcessing;
            private int _pushedFrames;
            private readonly DispatcherFrame _frame;

            #endregion

            #region Private Utilities

            private void FileSystemWatcher_OnChanged(object sender, FileSystemEventArgs e)
            {
                if (!_stopProcessing)
                {
                    int inc = Interlocked.Increment(ref _pushedFrames);
                    Assert.BoundedInteger(1, inc, int.MaxValue);
                    _frame.Continue = true;
                    _dispatcher.Invoke(DispatcherPriority.Normal, (_FileChanged)_OnFileChanged, e.FullPath);
                }
            }

            private void FileSystemWatcher_OnCreated(object sender, FileSystemEventArgs e)
            {
                if (!_stopProcessing)
                {
                    int inc = Interlocked.Increment(ref _pushedFrames);
                    Assert.BoundedInteger(1, inc, int.MaxValue);
                    _frame.Continue = true;
                    _dispatcher.Invoke(DispatcherPriority.Normal, (_FileChanged)_OnFileChanged, e.FullPath);
                }
            }

            private void FileSystemWatcher_OnDeleted(object sender, FileSystemEventArgs e)
            {
                if (!_stopProcessing)
                {
                    int inc = Interlocked.Increment(ref _pushedFrames);
                    Assert.BoundedInteger(1, inc, int.MaxValue);
                    _frame.Continue = true;
                    _dispatcher.Invoke(DispatcherPriority.Normal, (_FileChanged)_OnFileDeleted, e.FullPath);
                }
            }

            private void FileSystemWatcher_OnRenamed(object sender, RenamedEventArgs e)
            {
                if (!_stopProcessing)
                {
                    int inc = Interlocked.Increment(ref _pushedFrames);
                    Assert.BoundedInteger(1, inc, int.MaxValue);
                    _frame.Continue = true;
                    _dispatcher.Invoke(DispatcherPriority.Normal, (_FileRenamed)_OnFileRenamed, e.OldFullPath, e.FullPath);
                }
            }

            // This could get called concurrently on multiple threads, but the
            // potential races aren't interesting.  To do this correctly we'd
            // keep a list of files to be processed, but that's unnecessarily
            // heavy until demonstrated otherwise.
            private void _SetPendingFile(string path)
            {
                if (_pendingUpdateFile != path || null == path)
                {
                    _retryCount = 0;
                    _pendingUpdateFile = path;
                }
            }

            private void _ReprocessPendingUpdate()
            {
                if (!string.IsNullOrEmpty(_pendingUpdateFile))
                {
                    _timer.Stop();
                    if (_retryCount < _MaxRetryCount)
                    {
                        ++_retryCount;
                        int inc = Interlocked.Increment(ref _pushedFrames);
                        Assert.BoundedInteger(1, inc, int.MaxValue);
                        _frame.Continue = true;
                        _OnFileChanged(_pendingUpdateFile);
                    }

                }
            }

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
            private void _TryGetContactInfo(string path, string key, out ContactInfo newInfo, ref ContactCollectionChangeType action)
            {
                newInfo = new ContactInfo();

                Assert.IsTrue(_dispatcher.CheckAccess());

                bool failedMissingFile = false;
                bool failedBadData = false;
                bool failedManagerIsGone = false;
                try
                {
                    // We just have a file with this extension, it's not necessarily valid.
                    // If we can't get a contact from it then don't raise a notification.
                    using (Contact contact = _loader.GetByFilePath(path))
                    {
                        newInfo.Id = contact.Id;
                        newInfo.LastWrite = File.GetLastWriteTime(path);
                    }

                    _knownContacts[key] = newInfo;
                }
                // Couldn't load the file.  Whatever...
                catch (FileNotFoundException) { failedMissingFile = true; }
                catch (DirectoryNotFoundException) { failedMissingFile = true; }
                catch (InvalidDataException) { failedBadData = true; }
                catch (ObjectDisposedException) { failedManagerIsGone = true; }
                // Let these propagate to the caller.
                catch (IOException)
                {
                    throw;
                }
                catch (UnauthorizedAccessException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    // Not expecting other exceptions here, but if we hit them then throw away the cached object.
                    Assert.Fail(e.ToString());
                    failedBadData = true;
                }

                // Only care about failures if we're still processing.
                if (_stopProcessing)
                {
                    action = ContactCollectionChangeType.NoChange;
                    return;
                }

                if (failedBadData)
                {
                    // If we couldn't load the file and this was going to be treated as an add, then just do nothing.
                    action = (ContactCollectionChangeType.Added == action)
                        ? ContactCollectionChangeType.NoChange
                        : ContactCollectionChangeType.Removed;
                    _knownContacts.Remove(key);
                }
                else if (failedMissingFile)
                {
                    // If we couldn't load the file and this was going to be treated as an add, then just do nothing.
                    switch (action)
                    {
                        case ContactCollectionChangeType.Added:
                            action = ContactCollectionChangeType.NoChange;
                            break;
                        case ContactCollectionChangeType.Updated:
                            _SetPendingFile(path);
                            _timer.Start();
                            action = ContactCollectionChangeType.NoChange;
                            break;
                        default:
                            action = ContactCollectionChangeType.Removed;
                            _knownContacts.Remove(key);
                            break;
                    }
                }
                else if (failedManagerIsGone)
                {
                    // We should stop processing these...
                    action = ContactCollectionChangeType.NoChange;
                }
            }

            /// <summary>
            /// Generic function to deal with changed file events of types Created and Changed.
            /// </summary>
            /// <param name="path">The full path of the changed file.</param>
            private void _OnFileChanged(string path)
            {
                Assert.IsNeitherNullNorEmpty(path);

                Assert.IsTrue(_dispatcher.CheckAccess());
                Assert.BoundedInteger(1, _pushedFrames, int.MaxValue);
                Assert.IsTrue(_frame.Continue);
                // Caller should have incremented _pushedFrames.
                // Need to unconditionally decrement it when this returns and update _frame.
                try
                {
                    // Normalize the path.
                    string pathNormalized = path.ToUpperInvariant();

                    if (path != _pendingUpdateFile)
                    {
                        _ReprocessPendingUpdate();
                    }

                    ContactInfo oldInfo;
                    ContactInfo newInfo = default(ContactInfo);
                    ContactCollectionChangeType action = _knownContacts.TryGetValue(pathNormalized, out oldInfo)
                        ? ContactCollectionChangeType.Updated
                        : ContactCollectionChangeType.Added;

                    for (int i = 0; i < 5; ++i)
                    {
                        Exception ex;
                        try
                        {
                            _TryGetContactInfo(path, pathNormalized, out newInfo, ref action);
                            break;
                        }
                        catch (IOException e)
                        {
                            ex = e;
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            ex = e;
                        }

                        // Only get here because we caught an exception.
                        Assert.IsNotNull(ex);

                        if (i == 5)
                        {
                            Assert.Fail(ex.Message);
                            action = ContactCollectionChangeType.NoChange;
                        }
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));

                        //Application.DoEvents();
                    }

                    switch (action)
                    {
                        case ContactCollectionChangeType.NoChange:
                            break;
                        case ContactCollectionChangeType.Updated:
                            // If the write time hasn't changed since the last notification then swallow this redundant notification
                            if (newInfo.LastWrite != oldInfo.LastWrite)
                            {
                                _notify(new ContactCollectionChangedEventArgs(action, oldInfo.Id, newInfo.Id));
                            }
                            break;
                        case ContactCollectionChangeType.Added:
                            _notify(new ContactCollectionChangedEventArgs(action, newInfo.Id));
                            break;
                        case ContactCollectionChangeType.Removed:
                            _notify(new ContactCollectionChangedEventArgs(action, oldInfo.Id));
                            break;
                        default:
                            Assert.Fail();
                            break;
                    }

                    // Clear the pending update if we just processed it.
                    if (_pendingUpdateFile == path && action != ContactCollectionChangeType.NoChange)
                    {
                        _SetPendingFile(null);
                    }
                }
                finally
                {
                    int dec = Interlocked.Decrement(ref _pushedFrames);
                    Assert.BoundedInteger(0, dec, int.MaxValue);
                    if (0 == dec)
                    {
                        _frame.Continue = false;
                    }
                }
            }

            /// <summary>
            /// Generic function to deal with changed file events of type Deleted.
            /// </summary>
            /// <param name="path">The full path of the deleted file.</param>
            private void _OnFileDeleted(string path)
            {
                Assert.IsNeitherNullNorEmpty(path);

                // Normalize the path.
                string pathNormalized = path.ToUpperInvariant();

                Assert.IsTrue(_dispatcher.CheckAccess());
                Assert.IsTrue(_pushedFrames > 0);
                Assert.IsTrue(_frame.Continue);
                // Caller should have incremented _pushedFrames.
                // Need to unconditionally decrement it when this returns and update _frame.
                try
                {
                    _ReprocessPendingUpdate();

                    // If the file wasn't a known contact, then nothing to do.
                    // Otherwise, remove it from the list and send the notification.
                    ContactInfo oldInfo;
                    if (_knownContacts.TryGetValue(pathNormalized, out oldInfo))
                    {
                        _knownContacts.Remove(pathNormalized);
                        _notify(new ContactCollectionChangedEventArgs(ContactCollectionChangeType.Removed, oldInfo.Id));
                    }
                }
                finally
                {
                    int dec = Interlocked.Decrement(ref _pushedFrames);
                    Assert.BoundedInteger(0, dec, int.MaxValue);
                    if (0 == dec)
                    {
                        _frame.Continue = false;
                    }
                }
            }

            private void _OnFileRenamed(string oldPath, string newPath)
            {
                Assert.IsNeitherNullNorEmpty(oldPath);
                Assert.IsNeitherNullNorEmpty(newPath);

                // Normalize the paths.
                string oldPathNormalized = oldPath.ToUpperInvariant();
                string newPathNormalized = newPath.ToUpperInvariant();

                Assert.IsTrue(_dispatcher.CheckAccess());
                Assert.IsTrue(_pushedFrames > 0);
                Assert.IsTrue(_frame.Continue);
                // Caller should have incremented _pushedFrames.
                // Need to unconditionally decrement it when this returns and update _frame.
                try
                {
                    ContactInfo oldInfo;
                    ContactInfo newInfo = default(ContactInfo);
                    if (_knownContacts.TryGetValue(oldPathNormalized, out oldInfo))
                    {
                        _knownContacts.Remove(oldPathNormalized);

                        // Update the Id at the new location
                        try
                        {
                            // Make sure we can still get the contact.  If we can't then remove it.
                            // If we can't get a contact from it then don't raise a notification.
                            using (Contact contact = _loader.GetByFilePath(newPath))
                            {
                                newInfo.Id = contact.Id;
                                newInfo.LastWrite = File.GetLastWriteTime(newPath);
                            }

                            _knownContacts[newPathNormalized] = newInfo;
                        }
                        // Couldn't load the file.  Whatever...
                        catch (FileNotFoundException)
                        {
                            // If we couldn't load the file, treat this as a delete.  But it's weird that this happened...
                            _notify(new ContactCollectionChangedEventArgs(ContactCollectionChangeType.Removed, oldInfo.Id));
                            return;
                        }

                        _notify(new ContactCollectionChangedEventArgs(ContactCollectionChangeType.Moved, oldInfo.Id, newInfo.Id));

                        // Did we miss an update because this file was moved?
                        // If so, reprocess it.
                        Assert.IsNotNull(oldPath);
                        if (_pendingUpdateFile == oldPath)
                        {
                            newInfo = _knownContacts[newPathNormalized];
                            // Modify the write time so we won't ignore the change.
                            // This is kindof fragile, but tests should catch if the comparison method changes.
                            newInfo.LastWrite = newInfo.LastWrite.AddMilliseconds(-100);
                            _knownContacts[newPathNormalized] = newInfo;
                            _SetPendingFile(newPath);
                            _ReprocessPendingUpdate();
                        }
                    }
                }
                finally
                {
                    int dec = Interlocked.Decrement(ref _pushedFrames);
                    Assert.BoundedInteger(0, dec, int.MaxValue);
                    if (0 == dec)
                    {
                        _frame.Continue = false;
                    }
                }
            }

            private void _PopulateKnownContacts(ContactTypes type)
            {
                Assert.IsNotNull(_loader);
                Assert.IsNotNull(_dispatcher);
                Assert.IsTrue(_dispatcher.CheckAccess());
                Assert.IsNotNull(_knownContacts);
                Assert.AreEqual(0, _knownContacts.Count);

                // Go through all contacts associated with the manager and make the mapping of file paths to the ContactIds. 
                foreach (var entry in _loader.GetPropertiesCollection(type))
                {
                    // Not using these contacts for very long.  Don't need the ContactManager parameter.
                    using (var contact = new Contact(null, entry.Value, entry.Key))
                    {
                        ContactInfo info = default(ContactInfo);
                        info.Id = contact.Id;
                        string pathNormalized = contact.Path.ToUpperInvariant();
                        info.LastWrite = File.GetLastWriteTime(pathNormalized);
                        _knownContacts.Add(pathNormalized, info);
                    }
                }
            }
            #endregion

            #region Constructors

            [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
            public ContactWatcherSingular(Dispatcher dispatcher, ContactLoader loader, string directory, ContactTypes type, ContactWatcherEventCallback callback)
            {
                Assert.IsNotNull(dispatcher);
                // It's expected that this is created on the same thread as the manager,
                // so don't need to go through the dispatcher for methods invoked in the constructor.
                Assert.IsTrue(dispatcher.CheckAccess());
                Assert.IsNotNull(loader);
                Assert.IsFalse(string.IsNullOrEmpty(directory));
                Assert.IsNotNull(callback);
                Assert.IsTrue(Enum.IsDefined(typeof(ContactTypes), type));
                Assert.AreNotEqual(ContactTypes.All, type);
                Assert.AreNotEqual(ContactTypes.None, type);

                _dispatcher = dispatcher;
                _rootDirectory = directory;
                _loader = loader;
                _notify = callback;
                _findExtension = "*" + Contact.GetExtensionsFromType(type);

                // This is why creating this object is expensive:
                // In order to be able to give accurate change notifications we need to know all the contacts that are present at the beginning.
                _knownContacts = new Dictionary<string, ContactInfo>();

                // _pushedFrames = 0;
                _frame = new DispatcherFrame
                {
                    Continue = false
                };

                // Create the timer, but only signal it if we're going to need to reprocess an update.
                _timer = new Timer();
                _timer.Tick += delegate
                {
                    if (!_stopProcessing)
                    {
                        // Not invoked by the FileSystemWatcher, so don't push a frame here.
                        _dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)_ReprocessPendingUpdate);
                    }
                };
                _timer.Interval = TimeSpan.FromMilliseconds(_TimerMsDelay);
                _timer.IsEnabled = false;

                _PopulateKnownContacts(type);

                _fileWatch = new FileSystemWatcher(_rootDirectory, _findExtension)
                {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Attributes,
                };

                // There's some extra indirection required here because the manager requires thread affinity.
                _fileWatch.Changed += FileSystemWatcher_OnChanged;
                _fileWatch.Created += FileSystemWatcher_OnCreated;
                _fileWatch.Deleted += FileSystemWatcher_OnDeleted;
                _fileWatch.Renamed += FileSystemWatcher_OnRenamed;
                _fileWatch.EnableRaisingEvents = true;
            }

            #endregion

            #region IDisposable Members

            [
                SuppressMessage(
                    "Microsoft.Security",
                    "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"),
                SuppressMessage(
                    "Microsoft.Usage",
                    "CA2213:DisposableFieldsShouldBeDisposed",
                    MessageId = "_fileWatch"),
                SuppressMessage(
                    "Microsoft.Usage",
                    "CA2213:DisposableFieldsShouldBeDisposed",
                    MessageId = "_timer"),
            ]
            public void Dispose()
            {
                Assert.IsTrue(_dispatcher.CheckAccess());

                _fileWatch.Changed -= FileSystemWatcher_OnChanged;
                _fileWatch.Created -= FileSystemWatcher_OnCreated;
                _fileWatch.Deleted -= FileSystemWatcher_OnDeleted;
                _fileWatch.Renamed -= FileSystemWatcher_OnRenamed;
                _fileWatch.EnableRaisingEvents = false;

                _stopProcessing = true;

                _timer.Stop();
                //Utility.SafeDispose(ref _timer);

                // Finish processing everything queued by the SystemFileWatcher
                // so we can successfully Dispose it.
                Dispatcher.PushFrame(_frame);

                // Debugging tip: Historically this has lead to deadlocks.  The PushFrame above is designed
                // to guard against it.  If this Dispose deadlocks anyways then chances are one
                // of the callbacks threw an unexpected exception.  Try commenting out this dispose
                // and the Exception will generally propagate out.
                Utility.SafeDispose(ref _fileWatch);
            }

            #endregion
        }
    }
}
