/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

//
// A cache with a bad policy is another name for a memory leak.
//    - Rico Mariani
//

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Threading;
    using Standard;

    /// <summary>
    /// Manage the retrieval of Contact XML streams from disk.
    /// </summary>
    internal class ContactLoader
    {
        /// <summary>Properties of the loader's cache.</summary>
        /// <remarks>
        /// The way the cache works is it keeps any loaded contact in memory until
        /// the policy says it should be removed.  The cache is important because it
        /// makes it so we don't have to parse XML every time we load a contact, since
        /// the entry stores the file after it's already been processed.
        /// 
        /// Initially the cache is empty, we don't persist the cache beyond the process,
        /// so any requests to the loader will go to disk.  Whenever an IContactProperties
        /// has been loaded it gets added to the cache as well.  Since the loader only
        /// returns readonly references to IContactProperties the reference held by the
        /// cache represents a snapshot of the file.  The next time that file is requested
        /// we can just return another reference to the cache's copy.  To verify that the
        /// properties object is still valid, it's checked against the file's timestamp at
        /// the time of request.  If the timestamps ever don't match the cache entry is
        /// dropped.
        /// To avoid perpetually keeping these in memory beyond their use a timer is also
        /// set up on the cache.  Each entry has a TimeToLive property which is decremented
        /// with every timer hit.  If it reaches zero then properties reference is demoted
        /// to be a WeakReference.  There's no reason to explicitly drop the memory because
        /// it can still be in use by a Contact, so even if it's not on our internal MRU list
        /// we might still be able to bring it up if requested, unless the GC has gotten to it
        /// first.  If the entry's in the cache but the properties have either been dropped or
        /// are stale, then we just go back to disk for the contact and re-add it to the cache.
        /// Any time the contact is specifically requested, the TimeToLive counter on an entry
        /// is restored to a sentinal positive value, which keeps commonly requested contacts
        /// more likely to be available in the cache.
        /// </remarks>
        private static class _CachePolicy
        {
            /// <summary>
            /// Milliseconds interval for the timer that decrements the cache entry's TTL.
            /// </summary>
            public const int TimerInterval = 1000 * 2 * 60;
            /// <summary>
            /// Number of timer hits before an entry's Properties reference gets demoted to a WeakReference.
            /// </summary>
            public const int TimeToLiveCount = 2;
        }

        private class _CacheEntry
        {
            public string FilePath { get; private set; }
            public DateTime LastModified { get; private set; }
            public IContactProperties Properties { get; private set; }

            private WeakReference _weakProperties;
            private int _timeToLive;

            public _CacheEntry(string path, DateTime lastWriteAtLoad, IContactProperties properties)
            {
                Assert.IsFalse(string.IsNullOrEmpty(path));
                Assert.IsNotNull(properties);

                FilePath = path;
                LastModified = lastWriteAtLoad;
                Properties = properties;

                _timeToLive = _CachePolicy.TimeToLiveCount;
                // _weakProperties = null;
            }

            public bool TryGetProperties(out IContactProperties properties)
            {
                properties = null;

                // File.GetLastWriteTime is documented as throwing UnauthorizedAccessException
                // when the file is missing.
                DateTime fileLastAccess;
                try
                {
                    fileLastAccess = File.GetLastWriteTimeUtc(FilePath);
                }
                // UnauthorizedAccessException is thrown in the case of a missing file.
                catch (UnauthorizedAccessException)
                {
                    // Can't get properties from a missing file.
                    return false;
                }
                //catch (FileNotFoundException) { exceptionRaised = true; }
                //catch (DirectoryNotFoundException) { exceptionRaised = true; }
                catch (Exception e)
                {
                    Assert.Fail("This path shouldn't have gotten into the dictionary.\n " + e.Message);
                    throw;
                }

                // Can use this only if the file hasn't been modified since it was added.
                if (!fileLastAccess.Equals(LastModified))
                {
                    return false;
                }

                // The Properties reference may have been demoted to a WeakReference.
                // If it was but is still retrievable re-promote it to a strong reference.
                if (null == Properties)
                {
                    Properties = _weakProperties.Target as IContactProperties;
                    // If it's been collected we won't use it.
                    if (null == Properties)
                    {
                        return false;
                    }
                }

                Assert.IsNotNull(Properties);

                // If this is being requested then restore the maximum time-to-live counter.
                // Ensure most-recently used properties are prioritized.
                _timeToLive = _CachePolicy.TimeToLiveCount;

                properties = Properties;
                return true;
            }

            /// <summary>Decrements the heart-beat timer for this.</summary>
            /// <returns>
            /// Whether this entry is still active.  False implies it's completely dead.
            /// </returns>
            public bool DecrementTimer()
            {
                --_timeToLive;
                if (_timeToLive <= 0)
                {
                    if (null != Properties)
                    {
                        // The entry hasn't been accessed for a fair amount of time.
                        // Can't forcibly clean up the resources held by the properties,
                        // but can demote the reference to be weak.
                        _weakProperties = new WeakReference(Properties);
                        Properties = null;
                    }

                    // If the garbage collector already cleared the properties then remove the entry.
                    return _weakProperties.IsAlive;
                }
                Assert.IsNotNull(Properties);
                return true;
            }
        }

        public static IContactProperties GetContactFromFile(string path)
        {
            DateTime dt;
            return _GetContactFromFile(path, out dt);
        }

        private static IContactProperties _GetContactFromFile(string path, out DateTime lastModifiedUtc)
        {
            using (var constream = new MemoryStream())
            {
                // Drop the file as quickly as possible so we don't keep the lock on it.
                // Copy it instead to an im-memory stream and work with that.
                using (var fstream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
                {
                    // Get the last write time while we have the lock on the file.
                    lastModifiedUtc = File.GetLastWriteTimeUtc(path);

                    Utility.CopyStream(constream, fstream);
                }

                constream.Position = 0;
                return new ReadonlyContactProperties(constream);
            }
        }
 
        /// <summary>Try to create a contact from a file.  Swallow reasonable exceptions.</summary>
        /// <param name="filePath">The file to load from</param>
        /// <param name="properties">The contact properties to return</param>
        /// <param name="lastModifiedUtc">Timestampe when the properties were last modified.</param>
        /// <returns></returns>
        private static bool _TryLoadPropertiesFromFile(string filePath, out IContactProperties properties, out DateTime lastModifiedUtc)
        {
            try
            {
                properties = _GetContactFromFile(filePath, out lastModifiedUtc);
                return true;
            }
            catch (FileNotFoundException) { }
            catch (InvalidDataException) { }
            catch (UnauthorizedAccessException) { }
            catch (Exception e)
            {
                // Really not expecting other failures here.  Potentially causes problems.
                Assert.Fail(e.ToString());
                throw;
            }

            lastModifiedUtc = default(DateTime);
            properties = null;
            return false;
        }

        private bool _TryGetCachedFile(string normalizedPath, out IContactProperties properties)
        {
            Assert.AreEqual(normalizedPath, normalizedPath.ToUpperInvariant());

            properties = null;
            _CacheEntry entry;

            if (!_fileCache.TryGetValue(normalizedPath, out entry))
            {
                return false;
            }

            bool ret = entry.TryGetProperties(out properties);
            if (!ret)
            {
                _fileCache.Remove(normalizedPath);
            }

            return ret;
        }

        private void _Validate()
        {
            _manager.VerifyAccess();
        }

        // Lookup table for Contacts based on their file location.
        private readonly Dictionary<string, _CacheEntry> _fileCache;
        private readonly ContactManager _manager;
        private readonly DispatcherTimer _timer;
        private readonly List<string> _purgeableKeys;

        public ContactLoader(ContactManager manager)
        {
            _manager = manager;
            _fileCache = new Dictionary<string, _CacheEntry>();
            _purgeableKeys = new List<string>();
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(_CachePolicy.TimerInterval), DispatcherPriority.Normal, _OnTimer, _manager.Dispatcher);
            _timer.Start();
        }

        private void _OnTimer(object sender, EventArgs e)
        {
            Assert.AreEqual(0, _purgeableKeys.Count);

            // Timer runs every so often to remove unused references.
            foreach (KeyValuePair<string,_CacheEntry> entry in _fileCache)
            {
                bool shouldRemove = !entry.Value.DecrementTimer();
                if (shouldRemove)
                {
                    _purgeableKeys.Add(entry.Key);
                }
            }

            foreach (string key in _purgeableKeys)
            {
                _fileCache.Remove(key);
            }
            _purgeableKeys.Clear();
        }

        public bool GetById(string contactId, out Contact contact)
        {
            //_Validate();

            contact = null;
            Contact maybeContact = null;

            string path;
            string guidString;
            try
            {
                path = ContactId.TokenizeContactId(contactId, ContactId.Token.Path);
                guidString = ContactId.TokenizeContactId(contactId, ContactId.Token.Guid);
            }
            catch (FormatException)
            {
                // If the ContactId is poorly formed we're not going to load it.
                return false;
            }

            Assert.IsNotNull(guidString);

            Guid id;
            if (!Utility.GuidTryParse(guidString, out id))
            {
                return false;
            }

            string fileName = Path.GetFileNameWithoutExtension(path);
            // Normalize and canonicalize the path so this will look the same every time.
            if (!string.IsNullOrEmpty(path))
            {
                path = Path.GetFullPath(path);
                string normalizedPath = path.ToUpperInvariant();

                // If there's a file part then try to use it.
                if (_manager.IsContainedPath(path))
                {
                    // First try to get this from the in-memory cache.
                    IContactProperties maybeProperties;
                    if (!_TryGetCachedFile(normalizedPath, out maybeProperties))
                    {
                        // Could fail for a bunch of reasons almost all of which are ignoreable.
                        DateTime lastModified;
                        if (_TryLoadPropertiesFromFile(path, out maybeProperties, out lastModified))
                        {
                            // If we got a contact from the file then store it so we don't have to
                            // go to disk next time.
                            _fileCache.Add(normalizedPath, new _CacheEntry(path, lastModified, maybeProperties));
                        }
                    }

                    if (null != maybeProperties)
                    {
                        // Does it also contain the contactId?
                        try
                        {
                            Assert.IsTrue(maybeProperties.IsReadonly);
                            maybeContact = new Contact(_manager, maybeProperties, path);
                            if (maybeContact.ContactIds.Contains(id))
                            {
                                contact = maybeContact;
                                maybeContact = null;
                                return true;
                            }
                        }
                        finally
                        {
                            Utility.SafeDispose(ref maybeContact);
                        }
                    }
                }
            }

            Assert.IsNull(maybeContact);
            try
            {
                foreach (Contact c in _manager.GetContactCollection(ContactTypes.All))
                {
                    if (c.ContactIds.Contains(id))
                    {
                        if (Path.GetFileNameWithoutExtension(c.Path).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                        {
                            contact = c;
                            return true;
                        }
                        else if (null == maybeContact)
                        {
                            // Keep going in case there's a better match.
                            maybeContact = c;
                            // skip the Dispose at the end of this.
                            continue;
                        }
                    }
                    c.Dispose();
                }

                if (null != maybeContact)
                {
                    contact = maybeContact;
                    maybeContact = null;
                    return true;
                }
            }
            finally
            {
                Utility.SafeDispose(ref maybeContact);
            }

            // Still no contact :(
            return false;
        }

        public IEnumerable<KeyValuePair<string, IContactProperties>> GetPropertiesCollection(ContactTypes typeFilter)
        {
            if (typeFilter == ContactTypes.None)
            {
                // Not a valid filter.
                // Not appropriate to throw an exception but not going to yield any results.
                yield break;
            }

            // This will throw exceptions for invalid enumeration values.
            string[] extensions = Contact.GetExtensionsFromType(typeFilter).Split('|');
            Assert.AreNotEqual(0, extensions.Length);

            foreach (string ext in extensions)
            {
                // Should be of the format ".extension".  Need to change to "*.extension" for FindFile.
                Assert.AreEqual('.', ext[0]);
                string findExt = "*" + ext;

                // DirectoryInfo.GetDirectories/GetFiles doesn't work here.
                // For one, the recursive version of GetDirectories doesn't check for reparse points,
                //     so it can end up in an infinite loop.
                // Secondly, GetFiles goes through and returns the full list, and since this enumerator
                //     is likely to bail early that's unnecessarily expensive.
                // So instead this is done mostly manually, and FindFirstFile/FindNextFile are P/Invoked.

                foreach (FileInfo file in FileWalker.GetFiles(new DirectoryInfo(_manager.RootDirectory), findExt, _manager.UseSubfolders))
                {
                    string path = file.FullName;
                    string normalizedPath = path.ToUpperInvariant();

                    // Try to find a cached copy of the properties.
                    // If it's not available go to disk.
                    IContactProperties properties;
                    if (!_TryGetCachedFile(normalizedPath, out properties))
                    {
                        DateTime lastModifiedUtc;
                        if (_TryLoadPropertiesFromFile(path, out properties, out lastModifiedUtc))
                        {
                            // If we got a contact from the file then store it so we don't have to
                            // go to disk next time.
                            _fileCache.Add(normalizedPath, new _CacheEntry(path, lastModifiedUtc, properties));
                        }
                    }
                    if (null != properties)
                    {
                        yield return new KeyValuePair<string, IContactProperties>(path, properties);
                    }
                }
            }
        }

        public Contact GetByFilePath(string path)
        {
            if (!_manager.IsContainedPath(path))
            {
                return null;
            }

            string normalizedPath = path.ToUpperInvariant();

            IContactProperties properties;
            if (!_TryGetCachedFile(normalizedPath, out properties))
            {
                DateTime lastModifiedUtc;
                properties = _GetContactFromFile(path, out lastModifiedUtc);
                _fileCache.Add(normalizedPath, new _CacheEntry(path, lastModifiedUtc, properties));
            }

            Assert.IsTrue(properties.IsReadonly);
            return new Contact(_manager, properties, path);
        }
    }
}
