/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;
    using System.Windows.Threading;
    using Win32;
    using Standard;
	using Synrc;
	using Microsoft.ContactsBridge.Interop;

    /// <summary>
    /// Manages the user's address book.
    /// </summary>
    /// <remarks>
    /// This class provides basic management functionality for the user's contacts.
    /// Contacts can be loaded by their ContactId string, or the entire address book
    /// can be enumerated.
    /// </remarks>
    public class ContactManager : DispatcherObject, IDisposable, INotifyPropertyChanged
    {
        internal sealed class _MeManager : IDisposable
        {
            // Key under HKCU where we store the Me contact's ContactId.
            // The ContactManager RootDirectory is the value under the key.  If we're sharing the
            // root with Windows, then the default value is used.
            private const string _MeRegKey = @"Software\Microsoft\WAB\Me";
            private RegistryListener _meListener;
            private readonly string _meRegValue;
            private string _meContactId;
            private readonly ContactManager _manager;

            public _MeManager(ContactManager manager)
            {
                Assert.IsNotNull(manager);
                Assert.IsTrue(Directory.Exists(manager.RootDirectory));

                // If this is the same as the user's contacts folder then we can share the Me contact.
                // Otherwise use a different registry value.
                //
                // This has weird implications if the Contacts folder is redirected while the manager
                // is up and running.  The behavior there is intentional, as that's very much an edge case.
                _meRegValue = manager.RootDirectory.Equals(ContactUtil.GetContactsFolder(), StringComparison.OrdinalIgnoreCase)
                    ? ""
                    : manager.RootDirectory;

                _manager = manager;
            }

            private event PropertyChangedEventHandler _PropertyChangedInternal;

            public event PropertyChangedEventHandler MeChanged
            {
                add
                {
                    if (null == _meListener)
                    {
                        _meContactId = GetMeContactId();
                        _meListener = new RegistryListener(RegistryHive.CurrentUser, _MeRegKey);

                        _meListener.KeyChanged += _OnMeKeyChanged;
                        _manager.CollectionChanged += _ListenForMeContactChanges;
                    }
                    _PropertyChangedInternal += value;
                }
                remove
                {
                    _PropertyChangedInternal -= value;
                }
            }


            public void SetMeContactId(string id)
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(_MeRegKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
                // CreateSubKey can return null if the operation failed.
                if (null == key)
                {
                    throw new InvalidOperationException("Unable to create the storage for the Me contact's Id");
                }

                try
                {
                    if (string.IsNullOrEmpty(id))
                    {
                        key.DeleteValue(_meRegValue, false);
                    }
                    else
                    {
                        key.SetValue(_meRegValue, id, RegistryValueKind.String);
                    }
                }
                finally
                {
                    key.Close();
                }
            }

            public string GetMeContactId()
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(_MeRegKey);
                // OpenSubKey tends to return null rather than throw exceptions if the key isn't present.
                if (null == key)
                {
                    return "";
                }

                try
                {
                    return key.GetValue(_meRegValue) as string;
                }
                finally
                {
                    key.Close();
                }
            }

            private void _OnMeKeyChanged(RegistryListener listener)
            {
                string newMeId = GetMeContactId();
                if (_meContactId != newMeId)
                {
                    _meContactId = newMeId;

                    PropertyChangedEventHandler handler = _PropertyChangedInternal;
                    if (null != handler)
                    {
                        handler(this, new PropertyChangedEventArgs("MeContact"));
                    }
                }
            }

            private void _ListenForMeContactChanges(object sender, ContactCollectionChangedEventArgs e)
            {
                // If there is no MeContact in the registry then we don't care about
                // changes to any contact - none of them is going to be Me.
                if (null != _meContactId)
                {
                    bool changed = false;

                    Contact contact;
                    string realMeId = GetMeContactId();
                    if (_manager.TryGetContact(realMeId, out contact))
                    {
                        if (e.NewId == contact.Id)
                        {
                            changed = true;
                        }
                    }
                    else
                    {
                        changed = true;
                    }

                    if (changed)
                    {
                        PropertyChangedEventHandler handler = _PropertyChangedInternal;
                        if (null != handler)
                        {
                            handler(this, new PropertyChangedEventArgs("MeContact"));
                        }
                    }
                }
            }

            public string RegistryValue
            {
                get { return _meRegValue; }
            }

            #region IDisposable Members

            [SuppressMessage(
                "Microsoft.Usage",
                "CA2213:DisposableFieldsShouldBeDisposed",
                MessageId = "_meListener")]
            public void Dispose()
            {
                Utility.SafeDispose(ref _meListener);
            }

            #endregion
        }

        #region Fields
        private const string _ExceptionStringBadThreadId = "Contacts can only be created on STA threads and can only be accessed from the thread on which they were created.";

        private readonly string _rootDirectory;
        private readonly ContactLoader _contactCache;
        private ContactWatcher _watcher;
        private bool _disposed;
        #endregion

        #region Auto Properties
        public bool UseSubfolders { get; private set; }
        #endregion

        #region Internal Utilities

        /// <summary>
        /// Is the path considered to be part of the user's address book?
        /// </summary>
        /// <param name="path">The path to check for containment.  This can be a directory or a file name.</param>
        /// <returns>Whether this path is part of the user's address book.</returns>
        /// <remarks>
        /// This does not check for whether the path exists.
        /// </remarks>
        internal bool IsContainedPath(string path)
        {
            //Assert.IsTrue(CheckAccess());

            if (!string.IsNullOrEmpty(path))
            {
                // Callers should have done this.
                Assert.AreEqual(Path.GetFullPath(path), path);

                return path.StartsWith(RootDirectory, StringComparison.CurrentCultureIgnoreCase);
            }
            return false;
        }

        /// <summary>
        /// Get the MeManager.  Exposed only for unit tests.
        /// </summary>
        internal _MeManager MeManager { get; private set; }

        #endregion

        #region Private Utilities

        /// <summary>
        /// Proxy for the CollectionChanged event that hides issues with re-entrancy and no listeners.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void _NotifyCollectionChanged(object sender, ContactCollectionChangedEventArgs e)
        {
            Assert.IsTrue(CheckAccess());

            // Only process these if we haven't already started disposing this...
            if (!_disposed)
            {
                // By assigning to a temporary we avoid race conditions with the event losing listeners
                // after the null check, and ensure that we don't end up in any kind of weird cycle due
                // to re-entrancy.
                EventHandler<ContactCollectionChangedEventArgs> handler = _CollectionChangedInternal;
                if (null != handler)
                {
                    handler(sender, e);
                }
            }
        }

        /// <summary>
        /// Perform basic validation to ensure that this object is in a valid state.
        /// This should be called at the beginning of any external-facing method.
        /// </summary>
        private void _Validate(bool verifyThread)
        {
            if (verifyThread)
            {
                // DispatcherObject baseclass enforces that this object
                // is only accessed from the thread on which it was created.
                VerifyAccess();
                // All the constructors should catch this also.
                Assert.IsApartmentState(ApartmentState.STA);
            }

            if (_disposed)
            {
                throw new ObjectDisposedException("this");
            }
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Create a new ContactManager.
        /// </summary>
        /// <remarks>
        /// This constructor roots the manager in the user's Contacts folder.
        /// </remarks>
        public ContactManager()
            : this("*", true)
        {
        }

        /// <summary>
        /// Create a new ContactManager rooted in a specific directory.
        /// </summary>
        /// <param name="rootDirectory">
        /// The directory to use as the root.  Use "*" for the user's Contacts folder.
        /// Use a path prefixed with "*", e.g. "*\subfolder" to user a root directory relative
        /// to the user's Contacts folder.
        /// </param>
        public ContactManager(string rootDirectory)
            : this(rootDirectory, true)
        {
        }

        /// <summary>
        /// Create a new ContactManager rooted in a specific directory.
        /// </summary>
        /// <param name="rootDirectory">
        /// The directory to use as the root.  Use "*" for the user's Contacts folder.
        /// Use a path prefixed with "*", e.g. "*\subfolder" to user a root directory relative
        /// to the user's Contacts folder.
        /// </param>
        /// <param name="recurseSubfolders">
        /// Whether this object should manage contacts in subfolders of the rootDirectory.
        /// False indicates that this will only access contact files contained at the top-level
        /// of the rootDirectory.
        /// </param>
        public ContactManager(string rootDirectory, bool recurseSubfolders)
        {
            Verify.IsApartmentState(ApartmentState.STA, _ExceptionStringBadThreadId);
            Verify.IsNeitherNullNorEmpty(rootDirectory, "rootDirectory");

            _rootDirectory = ContactUtil.ExpandRootDirectory(rootDirectory);
            UseSubfolders = recurseSubfolders;

            if (!Directory.Exists(_rootDirectory))
            {
                Directory.CreateDirectory(_rootDirectory);
            }

            // Cache could use, but doesn't really need to, the ContactWatcher.
            // Need to be careful with startup dependencies if the Watcher, Cache,
            // and this Manager refer to each other on startup.
            _contactCache = new ContactLoader(this);
            
            MeManager = new _MeManager(this);

            // Don't initialize the ContactWatcher until we need to.  It's expensive
            // because it loads all contacts from disk.
            // _watcher = null;
        }

        #endregion

        #region Public Properties and Methods

        /// <summary>
        /// Load a Contact from a contactId string.
        /// </summary>
        /// <param name="contactId">The identifier for the contact to load.</param>
        /// <returns>
        /// The contact that maps to the provided identifier.
        /// </returns>
        /// <remarks>
        /// The full Contact ID string is a composite of different ways to identify a contact.
        /// All contacts contain a heirarchical "ContactID" property as a Guid.  Contacts
        /// that are backed by a file may also contain the path as part of the ID.
        /// These IDs can be retrieved from Contact's Id property.  Passing that Id to GetContact
        /// or TryGetContact will resolve to a Contact.  Usually it will resolve to the same contact
        /// as the one where the Id was gotten even if the contact has been updated, or the
        /// backing file has been renamed, copied, or moved.  If the contact has been deleted
        /// then this will try to find a compatible contact to load (e.g. if the file was copied
        /// before being deleted, it may find the copy), but if no contact can be found with
        /// the identifier then GetContact will throw an UnreachableContactException (whereas TryGetContact
        /// will simply return false).  If two contacts have been merged, then calling GetContact on
        /// the Id of one of the original contacts will generally load the merged one.<para/>
        /// This will not load contacts that are outside the user's address book.
        /// </remarks>
        /// <exception cref="UnreachableContactException">
        /// If the contactId doesn't resolve to a contact in the user's address book.
        /// </exception>
        public Contact GetContact(string contactId)
        {
            _Validate(true);
            Verify.IsNeitherNullNorEmpty(contactId, "contactId");

            Contact contact;
            if (!TryGetContact(contactId, out contact))
            {
                throw new UnreachableContactException("The contact couldn't be loaded by this manager.");
            }
            return contact;
        }

        /// <summary>
        /// Try to load a Contact from a contactId string.
        /// This returns a Boolean on failure rather than throw an exception for common errors.
        /// </summary>
        /// <param name="contactId">
        /// The identifier for the contact to load.
        /// </param>
        /// <param name="contact">
        /// When true is returned, this contains the contact that maps to the provided identifier.
        /// If false is returned this parameter is set to null.
        /// </param>
        /// <returns>
        /// Returns true if a contact could be found with the provided identifier.
        /// </returns>
        /// <remarks>
        /// The full Contact ID string is a composite of different ways to identify a contact.
        /// All contacts contain a heirarchical "ContactID" property as a Guid.  Contacts
        /// that are backed by a file may also contain the path as part of the ID.
        /// These IDs can be retrieved from Contact's Id property.  Passing that Id to GetContact
        /// or TryGetContact will resolve to a Contact.  Usually it will resolve to the same contact
        /// as the one where the Id was gotten even if the contact has been updated, or the
        /// backing file has been renamed, copied, or moved.  If the contact has been deleted
        /// then this will try to find a compatible contact to load (e.g. if the file was copied
        /// before being deleted, it may find the copy), but if no contact can be found with
        /// the identifier then GetContact will throw an UnreachableContactException (whereas TryGetContact
        /// will simply return false).  If two contacts have been merged, then calling GetContact on
        /// the Id of one of the original contacts will generally load the merged one.<para/>
        /// This will not load contacts that are outside the user's address book.
        /// </remarks>
        public bool TryGetContact(string contactId, out Contact contact)
        {
            //_Validate(true);
            Verify.IsNeitherNullNorEmpty(contactId, "contactId");

            return _contactCache.GetById(contactId, out contact);
        }

        /// <summary>
        /// Create a new Contact associated with this manager.
        /// </summary>
        /// <returns>
        /// Returns a new Contact associated with this manager.
        /// </returns>
        /// <remarks>
        /// The ContactType of the created contact is ContactType.Contact.<para/>
        /// This is similar to creating a contact with Contact's default constructor, except that CommitChanges
        /// will work despite that there is no file initially backing the contact.  After CommitChanges is
        /// called on the contact can later be loaded by Id with this manager.<para/>
        /// The contact is not considered to be part of this manager until it has been committed.
        /// </remarks>
        public Contact CreateContact()
        {
            return CreateContact(ContactTypes.Contact);
        }

        /// <summary>
        /// Create a new Contact associated with this manager.
        /// </summary>
        /// <param name="type">The type of contact to create.  Must be a valid enumeration value.</param>
        /// <returns>Returns a new Contact associated with this manager.</returns>
        /// <remarks>
        /// This is similar to creating a contact with Contact's constructor, except that CommitChanges
        /// will work despite that there is no file initially backing the contact.  After CommitChanges is
        /// called on the contact can later be loaded by Id with this manager.<para/>
        /// The contact is not considered to be part of this manager until it has been committed.
        /// </remarks>
        public Contact CreateContact(ContactTypes type)
        {
            return new Contact(this, type);
        }

        /// <summary>
        /// Add a Contact to this manager.
        /// </summary>
        /// <param name="contact">The contact to import.</param>
        /// <remarks>
        /// Add a contact that doesn't exist as part of this manager to its collection.
        /// </remarks>
        public void AddContact(Contact contact)
        {
            Verify.IsNotNull(contact, "contact");
            Contact.CopyToDirectory(contact, RootDirectory);
        }
        
        /// <summary>
        /// Remove a contact with the Id from this manager's collection.
        /// </summary>
        /// <param name="contactId">The id of the contact to remove.</param>
        /// <returns>
        /// Returns whether the manager's collection was modified as a result of this call.
        /// </returns>
        /// <remarks>
        /// Even though a contact has been removed the id may still resolve to a contact if called
        /// with ContactManager.Load (e.g. the file has been copied and pasted).
        /// </remarks>
        /// <exception cref="System.IO.IOException">
        /// This will throw if the file that backs the contact can't be deleted because of access restrictions.
        /// </exception>
        public bool Remove(string contactId)
        {
            Verify.IsNotNull(contactId, "contactId");

            Contact item;
            if (!TryGetContact(contactId, out item))
            {
                // Contact doesn't appear to be from this manager
                return false;
            }

            try
            {
                string path = item.Path;
                Assert.IsNotNull(path);
                if (File.Exists(path))
                {
                    // If the file is locked then it's a legitimate exception to throw here.
                    // MSDN says this doesn't throw if the file doesn't exist.
                    File.Delete(path);
                    return true;
                }
                return false;
            }
            finally
            {
                Utility.SafeDispose(ref item);
            }
        }

        public IList<Contact> GetContactCollection()
        {
            return GetContactCollection(ContactTypes.Contact);
        }

        /// <summary>
        /// Get a collection of all the contacts in this manager.
        /// </summary>
        /// <returns>
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IList<Contact> GetContactCollection(ContactTypes typeFilter)
        {
            _Validate(false);
            IList<Contact> list = new List<Contact>();
            foreach (KeyValuePair<string, IContactProperties> prop in _contactCache.GetPropertiesCollection(typeFilter))
            {
                Contact c = new Contact(this, prop.Value, prop.Key);
                list.Add(c);
            }

            return list;
        }

        /// <summary>
        /// Get or set the contact that represents the user.
        /// </summary>
        /// <exception cref="UnreachableContactException">
        /// The Me contact can't be set to a contact that is not reachable by this ContactManager.
        /// This include contacts that are backed by files outside the user's address book.
        /// </exception>
        public Contact MeContact
        {
            get
            {
                _Validate(true);

                string id = MeManager.GetMeContactId();
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }

                Contact contact;
                // TryLoad will swallow reasonable exceptions (ERROR_NO_MATCH).
                bool loaded = TryGetContact(id, out contact);
                Assert.Implies(!loaded, null == contact);

                // CONSIDER: If there was a value in the registry but we couldn't load it, zero the value.
                // The drawback here is that when the caller tries to get the Me contact and it fails,
                // they'll immediately see a change and potentially try again, which is just kindof wasteful.
                //if (!loaded)
                //{
                //    _meManager.SetMeContactId(null);
                //}

                return contact;
            }
            set
            {
                _Validate(true);
                Verify.IsNotNull(value, "value");

                // It's generally a logic error to set Me to a contact that can't be loaded (i.e. not committed to disk)
                if (!IsContainedPath(value.Path))
                {
                    throw new UnreachableContactException("The contact can't be loaded by this manager.");
                }
                MeManager.SetMeContactId(value.Id);
            }
        }

        /// <summary>
        /// Get the user's Contacts folder.
        /// </summary>
        /// <remarks>
        /// The ContactManager considers all Contact files in and under this folder to be part of the user's
        /// address book.
        /// </remarks>
        public string RootDirectory
        {
            get
            {
                return _rootDirectory;
            }
        }

        #endregion

        #region IDisposable Pattern

        /// <summary>
        /// Release all resources held by this object.
        /// </summary>
        /// <remarks>
        /// Dispose can safely be called multiple times, but the object cannot be used for anything else.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected overload of Dispose that is standard in IDisposable patterns.
        /// </summary>
        /// <param name="disposing">Whether or not this is being called by Dispose, rather than by the finalizer.</param>
        /// <remarks>
        /// Overrides of this method should always call base.Dispose.
        /// </remarks>
        [
            SuppressMessage(
                "Microsoft.Usage",
                "CA2213:DisposableFieldsShouldBeDisposed",
                MessageId = "_meManager"),
            SuppressMessage(
                "Microsoft.Usage",
                "CA2213:DisposableFieldsShouldBeDisposed",
                MessageId = "_watcher",
                Justification="Disposed by Utility.SafeDispose")
        ]
        protected virtual void Dispose(bool disposing)
        {
            _disposed = true;
            if (disposing)
            {
                Utility.SafeDispose(ref _watcher);

                IDisposable disposableManager = MeManager;
                MeManager = null;
                if (null != disposableManager)
                {
                    disposableManager.Dispose();
                }
            }
        }

        #endregion

        #region INotifyCollectionChanged Members

        private event EventHandler<ContactCollectionChangedEventArgs> _CollectionChangedInternal;

        /// <summary>
        /// This event gets raised when there has been a change to the collection of contacts.
        /// </summary>
        public event EventHandler<ContactCollectionChangedEventArgs> CollectionChanged
        {
            add
            {
                // The act of subscribing to the event shouldn't really require thread affinity.
                _Validate(false);
                if (null == _watcher)
                {
                    ContactWatcherEventCallback callback = e => _NotifyCollectionChanged(this, e);
                    _watcher = new ContactWatcher(Dispatcher, _contactCache, _rootDirectory, callback);
                }
                _CollectionChangedInternal += value;
            }
            remove
            {
                _Validate(false);
                _CollectionChangedInternal -= value;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                // The act of subscribing to the event shouldn't really require thread affinity.
                _Validate(false);
                MeManager.MeChanged += value;
            }
            remove
            {
                // Reasonable that this might have been Disposed but still trying to unsubscribe events.
                // _Validate(false);
                _MeManager copy = MeManager;
                if (null != copy)
                {
                    copy.MeChanged -= value;
                }
            }
        }

        #endregion
    }
}
