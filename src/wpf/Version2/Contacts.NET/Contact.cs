/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/
//#define USE_VISTA_WRITER 

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using Standard;

    // Disambiguate Contact.Path property.
    using IOPath = System.IO.Path;

#if USE_VISTA_WRITER
    using WriteableContactPropertiesAlias = VistaContactProperties;
	using Synrc;
#else
    using WriteableContactPropertiesAlias = WriteableContactProperties;
    using Synrc;
    using System.Drawing;
	using Microsoft.ContactsBridge.Interop;
	using System.Collections;
#endif

	public enum NameNotation
	{
		Human,
		Formal
	}

    /// <summary>
    /// Options for Contact.CommitChanges.  These can be ORed together to combine options.
    /// </summary>
    [Flags]
    public enum ContactCommitOptions
    {
        /// <summary>
        /// Default options.
        /// </summary>
        None = 0,

        /// <summary>
        /// The backing file is renamed to match the formatted name of the Contact.
        /// </summary>
        /// <remarks>
        /// This flag has no effect if the file name isn't the same as the starting formatted name.
        /// If multiple Name nodes are present then the Default node is used to query the FormattedName property.
        /// </remarks>
        SyncStorageWithFormattedName = 1,

        /// <summary>
        /// The backing file is renamed to match the formatted name of the Contact.
        /// </summary>
        /// <remarks>
        /// Using this flag overwrites any user-made changes to the file name.
        /// </remarks>
        ForceSyncStorageWithFormattedName = (1 << 1),

        /// <summary>
        /// Changes made to the storage since this contact instance was loaded will be overwritten by these changes.
        /// </summary>
        /// <remarks>
        /// Without this flag, an IncompatibleChangesException will be thrown if the storage file isn't the same
        /// at the time of the CommitChanges call as it was when this contact was loaded.
        /// </remarks>
        IgnoreChangeConflicts = 1 << 2,
    }

    /// <summary>
    /// Flaggable enumeration for the different high-level types a contact can represent.
    /// </summary>
    /// <remarks>
    /// The values can be bitwise combined for APIs that filter based on a contact's type,
    /// such as GetContactCollection(ContactType) on ContactManager.
    /// </remarks>
    [Flags]
    public enum ContactTypes
    {
        /// <summary>
        /// The contact type is unknown.
        /// </summary>
        /// <remarks>
        /// This has the sentinel value of 0.
        /// </remarks>
        None     = 0,

        /// <summary>
        /// The contact represents a person.
        /// </summary>
        Contact  = 1 << 0,
        
        /// <summary>
        /// The contact represents a mixed collection of other contacts.
        /// </summary>
        Group    = 1 << 1,

        /// <summary>
        /// The contact represents a business or organization.
        /// </summary>
        Organization = 1 << 2,

        /// <summary>
        /// Bitwise combination of Contact, Organization, and Group.
        /// </summary>
        All = Contact | Group | Organization,
    }

    /// <summary>
    /// Get and set properties for a single contact.
    /// </summary>
    public class Contact : DispatcherObject, IDisposable, INotifyPropertyChanged, IMan
    {
        #region Public Static Methods

        /// <summary>
        /// Save a contact to a VCard file.
        /// </summary>
        /// <param name="contact">The contact to convert to a vCard.</param>
        /// <param name="filePath">
        /// The path where the vCard should be saved.
        /// If it doesn't exist it will be created.
        /// If it does exist it will be overridden.
        /// </param>
        /// <remarks>
        /// This takes a subset of the possible properties in the contact and writes
        /// them to a file formatted to the vCard 2.1 specification.
        /// The copy is not full fidelity.  It also does not support writing string
        /// properties that are outside the range of ANSI values.<para/>
        /// The vCard specification is available at http://www.imc.org/pdi/pdiproddev.html.
        /// </remarks>
        public static void SaveToVCard21(Contact contact, string filePath)
        {
            Verify.IsNotNull(contact, "contact");
            VCard21.EncodeToVCard(contact, filePath);
        }

        public static void SaveToVCard21(IList<Contact> contacts, string filePath)
        {
            Verify.IsNotNull(contacts, "contacts");
            VCard21.EncodeCollectionToVCard(contacts, filePath);
        }

        /// <summary>
        /// Parse a vCard file for its contacts.
        /// </summary>
        /// <param name="vcardStream">
        /// The text content to parse.  It should be in a format consistent with the vCard 2.1
        /// specification.  It can contain multiple vCard objects, either cascaded or embedded.
        /// </param>
        /// <returns>
        /// The collection of vCard objects contained in the stream viewed as Contacts.
        /// If the stream doesn't contain any valid vCard objects an empty collection is
        /// returned.  If the stream contains improperly formatted vCard object data then
        /// an exception is thrown.
        /// </returns>
        /// <remarks>
        /// Officially this supports files formatted as specified by the vCard 2.1
        /// specification.  It may read contacts that are in vCard 3.0 format but only
        /// insomuch as 3.0 is a superset of 2.1.<para/>
        /// The stream does not need to be in ANSI character encoding.
        /// If the encoding is of an extended character set then those characters can be
        /// written into the contact without data loss.  If the stream changes character
        /// encoding on a per-property basis (which is supported by the vCard spec) and
        /// the TextReader doesn't properly step through these characters then the results
        /// of this function are undefined (likely it will throw an exception).<para/>
        /// Groupings of properties as described in the vCard specification are ignored.
        /// The CHARSET attribute on properties is also ignored, as the stream is treated
        /// as text.<para/>
        /// The vCard specification is available at http://www.imc.org/pdi/pdiproddev.html.
        /// </remarks>
        public static ICollection<Contact> CreateFromVCard(TextReader vcardStream)
        {
            Verify.IsNotNull(vcardStream, "vcardStream");
            return VCard21.ReadVCard(vcardStream);
        }
        #endregion

        #region Statics
        private const string _defaultFileName = "Unknown";
        private const string _stockInvalidUrl = "http://invalid_url_in_contact";
        private const string _exceptionStringBadThreadId = "Contacts can only be created on STA threads and can only be accessed from the thread on which they were created.";

        private static readonly Random _randomNumberGenerator = new Random();
        private static readonly Dictionary<string, ContactTypes> _extensionMap = new Dictionary<string, ContactTypes>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<ContactTypes, string> _typeMap = new Dictionary<ContactTypes, string>();

        /// <summary>
        /// Infer the type of a contact based on a file path.
        /// </summary>
        /// <param name="path">
        /// The path of the file to determine the ContactType for.
        /// It can be a relative or absolute path.
        /// </param>
        /// <returns>
        /// A ContactType based on the file extension of the given path.
        /// If the path doesn't have an extension or the extension doesn't
        /// map to a known ContactType, None is returned.
        /// </returns>
        public static ContactTypes GetTypeFromExtension(string path)
        {
            Verify.IsNeitherNullNorEmpty(path, "path");

            if (_extensionMap.Count == 0)
            {
                // Populate this on demand.
                _extensionMap.Add(".contact", ContactTypes.Contact);
                _extensionMap.Add(".organization", ContactTypes.Organization);
                _extensionMap.Add(".group", ContactTypes.Group);
            }

            ContactTypes ct;
            path = IOPath.GetExtension(path);
            Assert.IsNotNull(path);
            if (!_extensionMap.TryGetValue(IOPath.GetExtension(path), out ct))
            {
                ct = ContactTypes.None;
            }
            return ct;
        }

        /// <summary>
        /// Get the file extensions associated with given ContactTypes.
        /// </summary>
        /// <param name="type">The ContactTypes to get the extensions for.
        /// This can be a bitwise combination of multiple ContactTypes.</param>
        /// <returns>
        /// Returns the file extensions associated with the given ContactType.
        /// If more than one type is contained in the parameter, the multiple
        /// extensions are returned in a string split by the pipe character ('|').
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// If the given type is ContactTypes.None or contains bits outside the valid
        /// range of ContactTypes.All.
        /// </exception>
        public static string GetExtensionsFromType(ContactTypes type)
        {
            return _GetExtensionsFromType(type, true);
        }

        #endregion

        #region Fields
        private IContactProperties _contactProperties;
        private readonly bool _sharedProperties;
        private readonly ContactManager _manager;
        private string _originalName;
        private ILabeledPropertyCollection<Name> _nameCollection;
        private ILabeledPropertyCollection<Position> _positionCollection;
        private ILabeledPropertyCollection<Guid?> _contactIdCollection;
        private ILabeledPropertyCollection<DateTime?> _dateCollection;
        private ILabeledPropertyCollection<Uri> _urlCollection;
        private ILabeledPropertyCollection<IMAddress> _imCollection;
        private ILabeledPropertyCollection<EmailAddress> _emailCollection;
        private ILabeledPropertyCollection<Person> _personCollection;
        private ILabeledPropertyCollection<Photo> _photoCollection;
        private ILabeledPropertyCollection<Certificate> _certCollection;
        private ILabeledPropertyCollection<PhysicalAddress> _addressCollection;
        private ILabeledPropertyCollection<PhoneNumber> _numberCollection;
        private bool _isStateValid = true;
        private readonly ContactTypes _type = ContactTypes.None;
        private string _sourceHash;
        #endregion

        #region Internal Static Utilities shared with ContactManager
        internal static void CopyToDirectory(Contact contact, string directory)
        {
            Assert.IsNotNull(contact);
            Assert.IsFalse(string.IsNullOrEmpty(directory));

            FileStream fstream = null;
            try
            {
                _GenerateUniqueFileName(
                    contact,
                    _GetExtensionsFromType(contact.ContactType, false),
                    directory,
                    delegate(string newPath, bool willRetry)
                    {
                        try
                        {
                            fstream = new FileStream(newPath, FileMode.CreateNew);
                            return true;
                        }
                        // This is expected in the case that the file already exists.
                        catch (IOException)
                        {
                            if (willRetry)
                            {
                                return false;
                            }
                            throw;
                        }
                    });
                using (Stream stm = contact._contactProperties.SaveToStream())
                {
                    Utility.CopyStream(fstream, stm);
                    fstream.Close();
                    fstream = null;
                }
            }
            finally
            {
                Utility.SafeDispose(ref fstream);
            }
        }
        #endregion

        #region Private Utilities

        private void _EnsureWriteableProperties()
        {
            if (_contactProperties.IsReadonly)
            {
                _contactProperties = WriteableContactPropertiesAlias.MakeWriteableCopy(_contactProperties);
                Assert.IsFalse(_contactProperties.IsReadonly);
            }
        }

        // delegate for _GenerateUniqueFileName
        private delegate bool _UniqueFileOperation(string path, bool willRetry);

        private static string _GenerateUniqueFileName(Contact contact, string extension, string directory, _UniqueFileOperation fileOp)
        {
            Verify.IsNotNull(contact, "contact");
            Verify.IsNotNull(directory, "directory");

            // Because of race conditions in the environment it's possible that the directory doesn't exist
            // or gets deleted at any point in this call.  Let the delegate deal with it as it may.

            string displayName = contact.Names.Default.FormattedName;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = _defaultFileName;
            }

            // Replace illegal file characters with underscores.
            displayName = _MakeValidFileName(displayName);

            for (int i = 0; i <= 50; ++i)
            {
                string returnPath;
                if (0 == i)
                {
                    returnPath = IOPath.Combine(directory, displayName) + extension;
                    if (fileOp(returnPath, true))
                    {
                        return returnPath;
                    }
                }
                else if (40 >= i)
                {
                    returnPath = IOPath.Combine(directory, displayName) + " (" + i.ToString((IFormatProvider)null) + ")" + extension;
                    if (fileOp(returnPath, true))
                    {
                        return returnPath;
                    }
                }
                else
                {
                    // At this point we're hitting pathological cases.  This should stir things up enough that it works.
                    // If this fails because of naming conflicts after an extra 10 tries, then I don't care.
                    returnPath = IOPath.Combine(directory, displayName) + " (" + _randomNumberGenerator.Next() + ")" + extension;
                    if (fileOp(returnPath, i != 50))
                    {
                        return returnPath;
                    }
                }
            }

            // If this went through the full loop without being able to successfully perform the operation,
            // then we need to throw an exception.  The willRetry parameter allows the caller to give more
            // specific exceptions.
            Assert.Fail("Internal only function.  The passed in delegate should have thrown before getting here.");
            throw new IOException();
        }

        private static string _GetExtensionsFromType(ContactTypes type, bool allowMultiset)
        {
            // Verify that the type doesn't contain bits outside the valid range and it contains
            // at least one bit inside the valid range.
            if ((ContactTypes.None != (type & ~ContactTypes.All))
                || (ContactTypes.None == (type & ContactTypes.All)))
            {
                throw new ArgumentException("The provided ContactType is not within the valid range.", "type"); 
            }

            if (_typeMap.Count == 0)
            {
                _typeMap.Add(ContactTypes.Organization, ".organization");
                _typeMap.Add(ContactTypes.Contact, ".contact");
                _typeMap.Add(ContactTypes.Group, ".group");
            }

            string ext;
            if (!_typeMap.TryGetValue(type, out ext) && allowMultiset)
            {
                ext = null;
                // Not a single value, check if we should try to compose a set.
                if ((type & ContactTypes.All) != 0)
                {
                    var extBuilder = new StringBuilder();
                    foreach (ContactTypes key in _typeMap.Keys)
                    {
                        if ((type & key) != 0)
                        {
                            extBuilder.Append(_typeMap[key]).Append("|");
                        }
                    }
                    // If nothing is returned then it's bad args.
                    if (0 != extBuilder.Length)
                    {
                        ext = extBuilder.ToString(0, extBuilder.Length - 1);
                    }
                }
            }

            // Checked this above.  The type should have yielded a result.
            Assert.Implies(allowMultiset, null != ext);

            return ext;
        }

        private static bool _IsValidSingleContactType(ContactTypes type, bool isUnknownOK)
        {
            return type == ContactTypes.Contact
                || type == ContactTypes.Organization
                || type == ContactTypes.Group
                || (isUnknownOK
                    ? (type == ContactTypes.None)
                    : false);
        }

        private static string _MakeValidFileName(string invalidPath)
        {
            return invalidPath
                .Replace('\\', '_')
                .Replace('/', '_')
                .Replace(':', '_')
                .Replace('*', '_')
                .Replace('?', '_')
                .Replace('\"', '_')
                .Replace('<', '_')
                .Replace('>', '_')
                .Replace('|', '_');
        }

        /// <summary>
        /// Ensure that this contact is in a valid state.
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidStateException"></exception>
        private void _Validate()
        {
            // DispatcherObject baseclass enforces that this object
            // is only accessed from the thread on which it was created.
            //VerifyAccess();
            // All the constructors should catch this also.
            //Assert.IsApartmentState(ApartmentState.STA);

            if (null == _contactProperties)
            {
                throw new ObjectDisposedException("this");
            }

            if (!_isStateValid)
            {
                throw new InvalidStateException("The contact is in an inconsistent state because of partial updates.  It needs to be reloaded to continue.  Path and Id are the only accessible methods at this point.");
            }
        }

        /// <summary>
        /// Wraps SetBinaryProperty and RemoveProperty so the caller doesn't need to check whether the value is null.
        /// </summary>
        /// <param name="propertyName">
        /// The property being set or removed.  It must not be null.
        /// </param>
        /// <param name="value">
        /// The value to set for propertyName.  If it is null the property is removed.
        /// </param>
        /// <param name="valueType">
        /// The mime type of the value stream.  If it is null a default is provided.
        /// </param>
        private void _SetOrRemoveBinaryProperty(string propertyName, Stream value, string valueType)
        {
            Assert.IsNotNull(propertyName);
            if (null == value)
            {
                RemoveProperty(propertyName);
            }
            else
            {
                SetBinaryProperty(propertyName, value, valueType);
            }
        }

        /// <summary>
        /// Wraps SetDateProperty and RemoveProperty so the caller doesn't need to check whether the value is null.
        /// </summary>
        /// <param name="propertyName">
        /// The property being set or removed.  It must not be null.
        /// </param>
        /// <param name="value">
        /// The value to set for propertyName.  If it is null the property is removed.
        /// </param>
        private void _SetOrRemoveDateProperty(string propertyName, DateTime? value)
        {
            Assert.IsNotNull(propertyName);
            if (value.HasValue)
            {
                SetDateProperty(propertyName, value.Value);
            }
            else
            {
                RemoveProperty(propertyName);
            }
        }

        /// <summary>
        /// Wraps SetStringProperty and RemoveProperty so the caller doesn't need to check whether the value is null.
        /// </summary>
        /// <param name="propertyName">
        /// The property being set or removed.  It must not be null.
        /// </param>
        /// <param name="value">
        /// The value to set for propertyName.  If it is null the property is removed.
        /// </param>
        private void _SetOrRemoveStringProperty(string propertyName, string value)
        {
            Assert.IsNotNull(propertyName);
            if (string.IsNullOrEmpty(value))
            {
                RemoveProperty(propertyName);
            }
            else
            {
                SetStringProperty(propertyName, value);
            }
        }

        /// <summary>
        /// Renames the file that the contact is backed by based on the preferred FormattedName property.
        /// </summary>
        /// <remarks>
        /// This reloads the contact from the file, so it's possible that the contact won't be available.
        /// If that's the case then this object is in an invalid state and _Validate will generally throw.
        /// </remarks>
        private void _RenameContact()
        {
            Assert.IsNotNull(_contactProperties);
            Assert.IsNotNull(Path);

            // This could be done by saving to the new file name, but it's better if NTFS object Ids can still be tracked.
            string newPath = _GenerateUniqueFileName(
                this,
                IOPath.GetExtension(Path),
                IOPath.GetDirectoryName(Path),
                delegate(string tryPath, bool willRetry)
                {
                    try
                    {
                        File.Move(Path, tryPath);
                        return true;
                    }
                    catch (IOException)
                    {
                        if (willRetry)
                        {
                            // This gets thrown if the file couldn't be moved because the file already exists.
                            return false;
                        }
                        // If _GenerateUniqueFileName isn't going to retry this delegate, propagate the error.
                        throw;
                    }
                });

            // We moved the file, so store the new path and generate the new Id.
            // Still the same contact in memory, so don't need to reload the stream.
            Path = newPath;
            Id = ContactId.GetRuntimeId(ContactIds.Default.Value, Path);

            // Notify listeners that the path and Id have changed.
            NotifyPropertyChanged(this, new ContactPropertyChangedEventArgs("Path", ContactPropertyChangeType.PathChanged));
            NotifyPropertyChanged(this, new ContactPropertyChangedEventArgs("Id", ContactPropertyChangeType.IdChanged));
        }

        /// <summary>
        /// Commits changes for a new ContactManager associated contact.
        /// </summary>
        /// <remarks>
        /// This generates file storage for the contact, and will reload it from the manager.
        /// </remarks>
        private void _CommitAndUpdateNewContact()
        {
            Assert.IsNotNull(_manager);
            Assert.IsNull(Path);

            // Only do this for new contacts (where type should already have been verified).
            // Otherwise we just keep the pre-existing extension, whatever it may be.
            string extension = _GetExtensionsFromType(_type, false);
            Assert.IsNotNull(extension);

            FileStream fstream = null;
            try
            {
                // Cache this early in case we go to an invalid state.
                // Reload it from the contact later to ensure that the format is the same.
                Path = _GenerateUniqueFileName(
                    this,
                    extension,
                    _manager.RootDirectory,
                    delegate(string maybePath, bool willRetry)
                    {
                        try
                        {
                            fstream = new FileStream(maybePath, FileMode.CreateNew);
                            return true;
                        }
                        // This is expected in the case that the file already exists.
                        catch (IOException)
                        {
                            if (willRetry)
                            {
                                return false;
                            }
                            throw;
                        }
                    });

                // Generate new state fields into temporary objects so we're not
                // persisting partial state in case of failures.

                string newPath = fstream.Name;
                string newId = ContactId.GetRuntimeId(ContactIds.Default.Value, newPath);
                string newHash;

                using (Stream stm = _contactProperties.SaveToStream())
                {
                    stm.Position = 0;
                    newHash = Utility.HashStreamMD5(stm);

                    stm.Position = 0;
                    Utility.CopyStream(fstream, stm);
                    fstream.Close();
                }

                // Update the fields
                Path = newPath;
                Id = newId;
                _sourceHash = newHash;

                // Notify listeners that there's now a path.
                NotifyPropertyChanged(this, new ContactPropertyChangedEventArgs("Path", ContactPropertyChangeType.PathChanged));
                NotifyPropertyChanged(this, new ContactPropertyChangedEventArgs("Id", ContactPropertyChangeType.IdChanged));
            }
            finally
            {
                Utility.SafeDispose(ref fstream);
            }
        }

        #endregion

        #region Internal Utilities

        /// <summary>
        /// A kill switch for internal classes to invalidate this contact because of an incomplete operation
        /// </summary>
        internal void Invalidate()
        {
            _isStateValid = false;
        }

        internal void NotifyPropertyChanged(object sender, ContactPropertyChangedEventArgs e)
        {
            Assert.IsNotNull(sender);
            Assert.IsNotNull(e);

            if (null != PropertyChanged)
            {
                PropertyChanged(sender, e);
            }
        }

        internal IContactProperties Properties
        {
            get
            {
                _Validate();
                return _contactProperties;
            }
        }

        internal IContactProperties WriteableProperties
        {
            get
            {
                _Validate();
                _EnsureWriteableProperties();
                return _contactProperties;
            }
        }

        // Exposed for Groups.
        internal ContactManager Manager
        {
            get
            {
                _Validate();
                return _manager;
            }
        }

        #endregion

        #region Constructors

        public Contact(ContactTypes type)
            : this((ContactManager)null, type)
        {
        }

        /// <summary>
        /// Create a new blank contact of type Contact.
        /// </summary>
        public Contact()
            : this(ContactTypes.Contact)
        {
        }

        /// <summary>
        /// Internal only constructor for ContactManager.
        /// </summary>
        /// <param name="manager">The associated ContactManager instance.</param>
        /// <param name="type">The type of the contact to create.</param>
        /// <remarks>
        /// This allows the contacts returned by IContactCollection to be managed by this class.
        /// The manager is associated with the contact, which allows for Save to be called without
        /// the contact being initially backed by a path.
        /// </remarks>
        internal Contact(ContactManager manager, ContactTypes type)
        {
            //Verify.IsApartmentState(ApartmentState.STA, _exceptionStringBadThreadId);
            //if (!_IsValidSingleContactType(type, false))
            //{
            //    throw new ArgumentException("The provided type must be of a legal single value (also not ContactTypes.None).", "type");
            //}

            _manager = manager;

            _contactProperties = new WriteableContactPropertiesAlias();
            // The IContactProperties is disposable by this object.
            //_sharedProperties = false;

            // New contact, no file name associated with the Id.
            Id = ContactId.GetRuntimeId(ContactIds.Default.Value, null);
            // _path = null;

            _originalName = string.Empty;
            _type = type;

            // New contact, so no worries of conflicting changes on save.
            // _sourceHash = null;
        }

        /// <summary>Internal only constructor for Contact.</summary>
        /// <param name="manager"></param>
        /// <param name="properties"></param>
        /// <param name="fileName"></param>
        internal Contact(ContactManager manager, IContactProperties properties, string fileName)
        {
            Assert.IsNotNull(properties);
            Verify.IsNeitherNullNorEmpty(fileName, "fileName");
            
            // Caller should have ensured this is canonicalized...
            Assert.AreEqual(fileName, IOPath.GetFullPath(fileName));

            _manager = manager;
            _contactProperties = properties;
            _sharedProperties = true;
            _sourceHash = _contactProperties.StreamHash;
            _type = GetTypeFromExtension(fileName);
            Path = fileName;
            Id = ContactId.GetRuntimeId(ContactIds.Default.Value, Path);
            _originalName = Names.Default.FormattedName;
        }

        /// <summary>
        /// Load a contact from a file.
        /// </summary>
        /// <param name="fileName">The file that contains the contact data to load.</param>
        /// <exception cref="InvalidDataException">
        /// The specified file exists but doesn't doesn't represent a valid contact.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// The specified file couldn't be found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The specified file couldn't be opened for an unknown reason.  It may be that it's
        /// opened within incompatible sharing permissions.  Retrying the operation at a later
        /// time may succeed.
        /// </exception>
        public Contact(string fileName)
        {
            Verify.IsNotNull(fileName, "fileName");
            //Verify.IsApartmentState(ApartmentState.STA, _exceptionStringBadThreadId);

            // make the path absolute.
            fileName = IOPath.GetFullPath(fileName);

            _contactProperties = ContactLoader.GetContactFromFile(fileName);
            _sharedProperties = true;

            // Only really need to compute the hash if we change the contact...
            _sourceHash = _contactProperties.StreamHash;
            Path = fileName;
            Id = ContactId.GetRuntimeId(ContactIds.Default.Value, Path);
            _originalName = Names.Default.FormattedName;
            _type = GetTypeFromExtension(fileName);
        }

        /// <summary>
        /// Load a contact from a stream.
        /// </summary>
        /// <param name="stream">The stream with the Contact contents to load.</param>
        /// <param name="type">
        /// The type of the contact to create.  ContactTypes.None is valid for this constructor.
        /// </param>
        /// <remarks>
        /// This is the only Contact constructor where ContactTypes.None is a valid type parameter.
        /// </remarks>
        public Contact(Stream stream, ContactTypes type)
        {
            Verify.IsNotNull(stream, "stream");
            Verify.IsApartmentState(ApartmentState.STA, _exceptionStringBadThreadId);
            if (!_IsValidSingleContactType(type, true))
            {
                throw new ArgumentException("ContactType must be a valid single value for this constructor (ContactTypes.None is OK).", "type");
            }

            _type = type;

            //
            // Default values are implicitly set by the runtime (CodeAnalysis tools flag unnecessary default initializations).
            //

            // Loading a contact from a stream, so there's no path and the user can't commit changes directly.
            // _sourceHash = null;
            // _path = null;
            // Shouldn't need this either (only used for Commit)
            // _originalName = null;

            stream.Position = 0;

            // No reason to assume that because we're being loaded from a stream that this
            // is going to be modified.  Go ahead and delay building the DOM.
            // CONSDIER: Adding a flag indicating intention to write.
            _contactProperties = new ReadonlyContactProperties(stream);

            // The IContactProperties is disposable by this object.
            //_sharedProperties = false;

            Id = ContactId.GetRuntimeId(ContactIds.Default.Value, null);
        }
        #endregion

        #region Public Properties and Methods

        public string AddNode(string collectionName, bool appendNode)
        {
            _Validate();
            Verify.IsNotNull(collectionName, "collectionName");

            _EnsureWriteableProperties();

            string nodeName = _contactProperties.CreateArrayNode(collectionName, appendNode);

            // Notify listeners of a new node.
            NotifyPropertyChanged(this, new ContactPropertyChangedEventArgs(nodeName, appendNode ? ContactPropertyChangeType.NodeAppended : ContactPropertyChangeType.NodePrepended));

            return nodeName;
        }

        public void CommitChanges()
        {
            ContactCommitOptions flags = ContactCommitOptions.None;

            // There are different default flags if there's an associated manager
            if (null != _manager)
            {
                flags = ContactCommitOptions.SyncStorageWithFormattedName;
            }

            CommitChanges(flags);
        }

        public void CommitChanges(ContactCommitOptions commitOptions)
        {
            _Validate();

            string formattedName = Names.Default.FormattedName;
            if (string.IsNullOrEmpty(formattedName))
            {
                formattedName = _defaultFileName;
            }

            if (null == Path)
            {
                if (null == _manager)
                {
                    throw new FileNotFoundException("The contact isn't backed by any store and isn't associated with a ContactManager.  To save it, use the SaveToFile method.");
                }

                _CommitAndUpdateNewContact();
            }
            else
            {
                if (_contactProperties.IsUnchanged)
                {
                    // No changes have been made, this will be a no op.  Just return successfully.
                    return;
                }

                bool forceSave = Utility.IsFlagSet((int)commitOptions, (int)ContactCommitOptions.IgnoreChangeConflicts);
                FileMode openMode = forceSave ? FileMode.OpenOrCreate : FileMode.Open;

                try
                {
                    using (var fstream = new FileStream(Path, openMode, FileAccess.ReadWrite, FileShare.Delete))
                    {
                        string fileHash = Utility.HashStreamMD5(fstream);
                        // If the force flag was passed then ignore hash discrepancies.
                        if (!forceSave
                            && (null != _sourceHash)
                            && (_sourceHash != fileHash))
                        {
                            throw new IncompatibleChangesException("Changes were made to this contact since it was loaded.  You can reload the contact to merge the changes, or you can call save with the Force flag.");
                        }

                        using (Stream stm = _contactProperties.SaveToStream())
                        {
                            Utility.CopyStream(fstream, stm);
                        }
                        _sourceHash = _contactProperties.StreamHash;
                    }
                }
                catch (FileNotFoundException e)
                {
                    Assert.IsFalse(forceSave, "If forcing the save we shouldn't care that the file was missing...");
                    throw new IncompatibleChangesException("The underlying file backing this contact has been deleted.  You can call CommitChanges with the Force flag to replace it, or explicitly calls SaveToFile.", e);
                }

                if (Utility.IsFlagSet((int)commitOptions, (int)(ContactCommitOptions.SyncStorageWithFormattedName | ContactCommitOptions.ForceSyncStorageWithFormattedName)))
                {
                    // There are a couple ways this can be eligible for rename..
                    bool renameable = false;

                    // If force and file name doesn't equal FormattedName
                    if (!IOPath.GetFileName(Path).StartsWith(formattedName, StringComparison.CurrentCulture)
                        && (Utility.IsFlagSet((int)commitOptions, (int)ContactCommitOptions.ForceSyncStorageWithFormattedName)))
                    {
                        renameable = true;
                    }

                    // If not force but original name equals file name, and original name doesn't equal Formatted name
                    if (IOPath.GetFileName(Path).StartsWith(_MakeValidFileName(_originalName), StringComparison.CurrentCulture)
                        && !string.Equals(_originalName, formattedName))
                    {
                        renameable = true;
                    }

                    if (renameable)
                    {
                        _RenameContact();
                    }
                }
            }

            // Update the original name for the next Save.
            _originalName = formattedName;
        }

        public string Id
        {
            // Don't call Validate.  Allow callers to still retrieve the Id when the state is inconsistent.
            get;
            private set;
        }

        /// <summary>
        /// Get the type of this Contact.
        /// </summary>
        public ContactTypes ContactType
        {
            get
            {
                _Validate();
                return _type;
            }
        }

        /// <summary>
        /// Get whether the file that backs this contact is readonly.
        /// </summary>
        /// <remarks>
        /// If this contact is not backed by a file then it is not considered readonly.
        /// </remarks>
        public bool IsReadOnly
        {
            get
            {
                _Validate();
                return !(null == Path || 0 == (File.GetAttributes(Path) & FileAttributes.ReadOnly));
            }
        }

        /// <summary>
        /// Get the path of the file that backs this contact.
        /// </summary>
        /// <remarks>
        /// Not all contacts are backed by files.
        /// If this contact is not backed by a path then the property is null.
        /// </remarks>
        public string Path
        {
            // Don't call validate.  Allow callers to still retrieve the Path when the state is inconsistent.
            get;
            private set;
        }

        public ContactProperty GetAttributes(string propertyName)
        {
            _Validate();
            Verify.IsNeitherNullNorEmpty(propertyName, "propertyName");

            return _contactProperties.GetAttributes(propertyName);
        }

        public Stream GetBinaryProperty(string propertyName, StringBuilder propertyType)
        {
            _Validate();
            Verify.IsNotNull(propertyName, "propertyName");

            // propertyType can be null.
            // Verify.IsNotNull(propertyType, "propertyType");

            if (null != propertyType && 0 != propertyType.Length)
            {
                propertyType.Remove(0, propertyType.Length);
            }

            string pt;
            Stream ret = GetBinaryProperty(propertyName, out pt);

            if (null != propertyType)
            {
                propertyType.Append(pt);
            }

            return ret;
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1021:AvoidOutParameters",
            MessageId = "1#",
            Justification="There's a StringBuilder overload that avoids the out parameter, but it's more awkward to use than this in most cases.")]
        public Stream GetBinaryProperty(string propertyName, out string propertyType)
        {
            _Validate();
            Verify.IsNotNull(propertyName, "propertyName");

            return _contactProperties.GetBinary(propertyName, out propertyType);
        }

        public DateTime? GetDateProperty(string propertyName)
        {
            _Validate();
            Verify.IsNotNull(propertyName, "propertyName");
            return _contactProperties.GetDate(propertyName);
        }

        /// <summary>
        /// Get the collection of labels for a given node.
        /// </summary>
        /// <param name="nodeName">
        /// The node that contains the labels.
        /// </param>
        /// <returns>
        /// The collection of labels for the node.
        /// </returns>
        public ILabelCollection GetLabelCollection(string nodeName)
        {
            _Validate();
            Verify.IsNotNull(nodeName, "nodeName");
            return new LabelCollection(this, nodeName);
        }

        /// <summary>
        /// Get an enumerator over properties in the contact.
        /// </summary>
        /// <returns>
        /// An enumerator over all properties in the contact.
        /// </returns>
        /// <remarks>
        /// This is semantically equivalent to calling
        /// GetPropertyCollection(null, null, false).
        /// </remarks>
        public IEnumerable<ContactProperty> GetPropertyCollection()
        {
            return GetPropertyCollection(null, null, false);
        }

        /// <summary>
        /// Get an enumerator over properties in the contact.
        /// </summary>
        /// <param name="collectionName">
        /// The collection of properties to enumerate.  Optional.
        /// If this is null then all properties are returned in the enumerator.
        /// </param>
        /// <param name="labelFilter">
        /// The labels to use as a filter.  Optional.
        /// If this is not null then only array nodes are returned in the enumeration.
        /// </param>
        /// <param name="anyLabelMatches">
        /// If label filters are provided, whether the filter means any
        /// label matches or all labels must match.
        /// </param>
        /// <returns>
        /// An enumerator over properties in the contact.
        /// </returns>
        public IEnumerable<ContactProperty> GetPropertyCollection(string collectionName, string[] labelFilter, bool anyLabelMatches)
        {
            _Validate();
            return _contactProperties.GetPropertyCollection(collectionName, labelFilter, anyLabelMatches);
        }

        public string GetStringProperty(string propertyName)
        {
            _Validate();
            Verify.IsNotNull(propertyName, "propertyName");

            return _contactProperties.GetString(propertyName);
        }

        public void RemoveNode(string nodeName)
        {
            _Validate();
            Verify.IsNotNull(nodeName, "nodeName");

            if (!_contactProperties.DoesPropertyExist(nodeName))
            {
                return;
            }

            // Just verified that this property exists, so shouldn't get a PropertyNotFoundException.
            ContactProperty attrib = GetAttributes(nodeName);
            Assert.AreNotEqual(attrib.PropertyType, ContactPropertyType.None);
            if (attrib.PropertyType != ContactPropertyType.ArrayNode)
            {
                throw new ArgumentException("The parameter isn't a node.  Please use RemoveProperty instead.", "nodeName");
            }
    
            _EnsureWriteableProperties();

            bool changed = _contactProperties.DeleteArrayNode(nodeName);
            if (changed)
            {
                // If the node existed notify listeners that this node is gone.
                NotifyPropertyChanged(this, new ContactPropertyChangedEventArgs(nodeName, ContactPropertyChangeType.NodeRemoved));
            }
        }

        public void RemoveProperty(string propertyName)
        {
            _Validate();
            Verify.IsNotNull(propertyName, "propertyName");

            if (!_contactProperties.DoesPropertyExist(propertyName))
            {
                return;
            }

            // Just verified that this exists, so we shouldn't get a PropertyNotFoundException
            ContactProperty attrib = GetAttributes(propertyName);
            Assert.AreNotEqual(attrib.PropertyType, ContactPropertyType.None);
            if (attrib.PropertyType == ContactPropertyType.ArrayNode)
            {
                throw new ArgumentException("The parameter is a node.  Please use RemoveNode instead.", "propertyName");
            }

            _EnsureWriteableProperties();

            bool changed = _contactProperties.DeleteProperty(propertyName);
            if (changed)
            {
                NotifyPropertyChanged(this, new ContactPropertyChangedEventArgs(propertyName, ContactPropertyChangeType.PropertyRemoved));
            }
        }

        public void Save(string filePath)
        {
            _Validate();
            using (var fstream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                Save(fstream);
            }
        }

        public void Save(Stream stream)
        {
            _Validate();
            Verify.IsNotNull(stream, "stream");
            using (Stream stm = _contactProperties.SaveToStream())
            {
                Utility.CopyStream(stream, stm);
            }
        }

        public void SetStringProperty(string propertyName, string value)
        {
            _Validate();
            Verify.IsNotNull(propertyName, "propertyName");

            // SetString doesn't support setting to an empty string.
            if (string.IsNullOrEmpty(value))
            {
                throw new FormatException("Value cannot be null or empty.");
            }

            if (PropertyNameUtil.IsPropertyValidNode(propertyName))
            {
                throw new SchemaException("The string value can't be set on the property because it appears to be an array node.");
            }

            _EnsureWriteableProperties();

            _contactProperties.SetString(propertyName, value);

            // Notify listeners of a change.
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new ContactPropertyChangedEventArgs(propertyName, ContactPropertyChangeType.PropertySet));
            }
        }

        public void SetDateProperty(string propertyName, DateTime value)
        {
            _Validate();
            Verify.IsNotNull(propertyName, "propertyName");

            if (PropertyNameUtil.IsPropertyValidNode(propertyName))
            {
                throw new SchemaException("The date value can't be set on the property because it appears to be an array node.");
            }

            _EnsureWriteableProperties();

            _contactProperties.SetDate(propertyName, value);

            // Notify listeners of a change.
            NotifyPropertyChanged(this, new ContactPropertyChangedEventArgs(propertyName, ContactPropertyChangeType.PropertySet));
        }

        public void SetBinaryProperty(string propertyName, Stream value, string valueType)
        {
            _Validate();
            Verify.IsNotNull(propertyName, "propertyName");
            Verify.IsNotNull(value, "value");

            if (PropertyNameUtil.IsPropertyValidNode(propertyName))
            {
                throw new SchemaException("The binary value can't be set on the property because it appears to be an array node.");
            }

            if (!value.CanRead || !value.CanSeek || 0 >= value.Length)
            {
                throw new ArgumentException("The provided stream doesn't satisfy the necessary requirements to be stored in the contact.  It must be readable, seekable, and not empty.");
            }

            _EnsureWriteableProperties();

            _contactProperties.SetBinary(propertyName, value, valueType);

            // Notify listeners of a change.
            NotifyPropertyChanged(this, new ContactPropertyChangedEventArgs(propertyName, ContactPropertyChangeType.PropertySet));
        }

        public UserTile UserTile
        {
            get
            {
                var usertile = new UserTile(Photos[PhotoLabels.UserTile]);
				/*
                if (_IsMeContact())
                {
                    Uri overlayUri = ContactUtil.GetResourceUri("meOverlay.ico");
                    var overlayImage = new BitmapImage(overlayUri);
                    usertile.Overlay = overlayImage;
                }
				 * */
                return usertile;
            }
        }

        private bool _IsMeContact()
        {
            if (null != _manager)
            {
                string meId = _manager.MeManager.GetMeContactId();

                return Id.Equals(meId, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        #endregion

        #region Named Contact Property Utilities
        // This region is all syntactic sugar.  In the COM APIs this functionality is accomplished
        // through CreateArrayNode/[Get|Set][String|Binary|Date] functions.
        // These properties give strongly typed, intellisense supported access to the common Contact properties.

        /// <summary>
        /// Get or set the Gender of this contact.
        /// </summary>
        /// <exception cref="SchemaException">
        /// Throws if there's an attempt to set the Gender to an invalid value.
        /// </exception>
        /// <remarks>
        /// If there is no Gender associated with this contact, Gender.Unspecified is returned upon query.
        /// </remarks>
        public Gender Gender
        {
            get
            {
                _Validate();
                string s = GetStringProperty(PropertyNames.Gender);
                if (string.IsNullOrEmpty(s))
                {
                    return Gender.Unspecified;
                }
                if (s.Equals(Gender.Male.ToString()))
                {
                    return Gender.Male;
                }
                if (s.Equals(Gender.Female.ToString()))
                {
                    return Gender.Female;
                }
                Assert.AreEqual(s, Gender.Unspecified.ToString());
                return Gender.Unspecified;
            }
            set
            {
                _Validate();
                string s;
                switch (value)
                {
                    case Gender.Female:
                    case Gender.Male:
                    case Gender.Unspecified:
                        s = value.ToString();
                        break;
                    default:
                        throw new SchemaException("A bad gender value was provided");
                }
                SetStringProperty(PropertyNames.Gender, s);
            }
        }

        /// <summary>
        /// Get or set Notes associated with this contact.
        /// </summary>
        /// <remarks>
        /// The Notes property can be used by the user or any program to
        /// store miscellaneous, unstructured information about the contact.
        /// </remarks>
        public string Notes
        {
            get
            {
                _Validate();
                return GetStringProperty(PropertyNames.Notes);
            }
            set
            {
                _Validate();
                _SetOrRemoveStringProperty(PropertyNames.Notes, value);
            }
        }

        /// <summary>
        /// Get the DateTime when this contact was created.
        /// </summary>
        public DateTime CreationDate
        {
            get
            {
                _Validate();
                // This should never fail with a null DateTime? instance.
                return (DateTime)GetDateProperty(PropertyNames.CreationDate);
            }
        }

		public string Labels
		{
			get
			{
				string res = "";
				ILabelCollection labels = ContactIds.GetLabelsAt(0);
				List<string> l = new List<string>(labels);
				l.Sort();
				foreach (string s in l)
					res += s.Trim() + " ";
				return res.Trim();
			}
		}

		public override string ToString()
		{
			return Labels;
		}

        /// <summary>
        /// Get or set the e-mail program associated with this contact.
        /// </summary>
        /// <remarks>
        /// This information can provide assistance to a correspondent regarding the type of
        /// data representation which can be used, and how they can be packaged. This property
        /// is based on the private MIME type X-Mailer that is generally implemented by MIME user
        /// agent products.
        /// </remarks>
        public string Mailer
        {
            get
            {
                _Validate();
                return GetStringProperty(PropertyNames.Mailer);
            }
            set
            {
                _Validate();
                _SetOrRemoveStringProperty(PropertyNames.Mailer, value);
            }
        }

        /// <summary>Commit a name in the contact's collection of names.</summary>
        /// <param name="contact">The contact that is having this name committed.</param>
        /// <param name="arrayNode">The array node where this name is being committed.</param>
        /// <param name="value">The name being committed.</param>
        private static void _CommitName(Contact contact, string arrayNode, Name value)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.NameCollection + PropertyNames.NameArrayNode, StringComparison.Ordinal));
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.FamilyName, value.FamilyName);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.FormattedName, value.FormattedName);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Generation, value.Generation);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.GivenName, value.GivenName);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.MiddleName, value.MiddleName);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Nickname, value.Nickname);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Title, value.PersonalTitle);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Phonetic, value.Phonetic);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Prefix, value.Prefix);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Suffix, value.Suffix);
        }

        private static Name _CreateName(Contact contact, string arrayNode)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.NameCollection + PropertyNames.NameArrayNode, StringComparison.Ordinal));
            string familyName = contact.GetStringProperty(arrayNode + PropertyNames.FamilyName);
            string formattedName = contact.GetStringProperty(arrayNode + PropertyNames.FormattedName);
            string generation = contact.GetStringProperty(arrayNode + PropertyNames.Generation);
            string givenName = contact.GetStringProperty(arrayNode + PropertyNames.GivenName);
            string middleName = contact.GetStringProperty(arrayNode + PropertyNames.MiddleName);
            string nickName = contact.GetStringProperty(arrayNode + PropertyNames.Nickname);
            string title = contact.GetStringProperty(arrayNode + PropertyNames.Title);
            string phonetic = contact.GetStringProperty(arrayNode + PropertyNames.Phonetic);
            string prefix = contact.GetStringProperty(arrayNode + PropertyNames.Prefix);
            string suffix = contact.GetStringProperty(arrayNode + PropertyNames.Suffix);
            return new Name(formattedName, phonetic, prefix, title, givenName, middleName, familyName, generation, suffix, nickName);
        }

        /// <summary>
        /// The collection of names that this contact is known by.
        /// </summary>
        public ILabeledPropertyCollection<Name> Names
        {
            get
            {
                _Validate();
                if (null == _nameCollection)
                {
                    _nameCollection = new SchematizedLabeledPropertyCollection<Name>(this, PropertyNames.NameCollection, PropertyNames.NameArrayNode, _CreateName, _CommitName);
                }
                return _nameCollection;
            }
        }

        /// <summary>Commit a position in the contact's collection of positions.</summary>
        /// <param name="contact">The contact that is having this position committed.</param>
        /// <param name="arrayNode">The array node where this position is being committed.</param>
        /// <param name="value">The position being committed.</param>
        private static void _CommitPosition(Contact contact, string arrayNode, Position value)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.PositionCollection + PropertyNames.PositionArrayNode, StringComparison.Ordinal));
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Company, value.Company);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Department, value.Department);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.JobTitle, value.JobTitle);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Office, value.Office);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Organization, value.Organization);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Profession, value.Profession);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Role, value.Role);
        }

        private static Position _CreatePosition(Contact contact, string arrayNode)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.PositionCollection + PropertyNames.PositionArrayNode, StringComparison.Ordinal));
            string company = contact.GetStringProperty(arrayNode + PropertyNames.Company);
            string department = contact.GetStringProperty(arrayNode + PropertyNames.Department);
            string jobTitle = contact.GetStringProperty(arrayNode + PropertyNames.JobTitle);
            string office = contact.GetStringProperty(arrayNode + PropertyNames.Office);
            string organization = contact.GetStringProperty(arrayNode + PropertyNames.Organization);
            string profession = contact.GetStringProperty(arrayNode + PropertyNames.Profession);
            string role = contact.GetStringProperty(arrayNode + PropertyNames.Role);
            return new Position(organization, role, company, department, office, jobTitle, profession);
        }

        /// <summary>
        /// The collection of positions held by this contact.
        /// </summary>
        public ILabeledPropertyCollection<Position> Positions
        {
            get
            {
                _Validate();
                if (null == _positionCollection)
                {
                    _positionCollection = new SchematizedLabeledPropertyCollection<Position>(this, PropertyNames.PositionCollection, PropertyNames.PositionArrayNode, _CreatePosition, _CommitPosition);
                }
                return _positionCollection;
            }
        }

        /// <summary>Commit a Contact Id in the contact's collection of Ids.</summary>
        /// <param name="contact">The contact that is having this Contact Id committed.</param>
        /// <param name="arrayNode">The array node where this Contact Id is being committed.</param>
        /// <param name="value">The Contact Id being committed.</param>
        private static void _CommitContactId(Contact contact, string arrayNode, Guid? value)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.ContactIdCollection + PropertyNames.ContactIdArrayNode, StringComparison.Ordinal));
            if (null != value)
            {
                contact.SetStringProperty(arrayNode + PropertyNames.Value, value.ToString());
            }
            else
            {
                contact.RemoveProperty(arrayNode + PropertyNames.Value);
            }
        }

        private static Guid? _CreateContactId(Contact contact, string arrayNode)
        {
            Guid? id = null;
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.ContactIdCollection + PropertyNames.ContactIdArrayNode, StringComparison.Ordinal));
            string value = contact.GetStringProperty(arrayNode + PropertyNames.Value);
            if (!string.IsNullOrEmpty(value))
            {
                id = new Guid(value);
            }
            return id;
        }

        /// <summary>
        /// The collection of unique Guid identifiers associated with this contact.
        /// </summary>
        /// <remarks>
        /// All contacts have at least one Id associated with them.  When contacts are merged,
        /// the winner absorbs the Id collection of the loser so ContactId strings that were
        /// associated with the old contact may still resolve against the merged contact.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification="Bogus positive, Nullable<T> should be excluded from this message.")]
        public ILabeledPropertyCollection<Guid?> ContactIds
        {
            get
            {
                _Validate();
                if (null == _contactIdCollection)
                {
                    _contactIdCollection = new SchematizedLabeledPropertyCollection<Guid?>(this, PropertyNames.ContactIdCollection, PropertyNames.ContactIdArrayNode, _CreateContactId, _CommitContactId);
                }
                return _contactIdCollection;
            }
        }

        /// <summary>Commit a date in the contact's collection of dates.</summary>
        /// <param name="contact">The contact that is having this date committed.</param>
        /// <param name="arrayNode">The array node where this date is being committed.</param>
        /// <param name="value">The date being committed.</param>
        private static void _CommitDate(Contact contact, string arrayNode, DateTime? value)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.DateCollection + PropertyNames.DateArrayNode, StringComparison.Ordinal));
            contact._SetOrRemoveDateProperty(arrayNode + PropertyNames.Value, value);
        }

        private static DateTime? _CreateDate(Contact contact, string arrayNode)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.DateCollection + PropertyNames.DateArrayNode, StringComparison.Ordinal));
            return contact.GetDateProperty(arrayNode + PropertyNames.Value);
        }

        /// <summary>
        /// The collection of dates associated with this contact.
        /// The relevance of each date is identified by the labels on the node.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Bogus positive, Nullable<T> should be excluded from this message.")]
        public ILabeledPropertyCollection<DateTime?> Dates
        {
            get
            {
                _Validate();
                if (null == _dateCollection)
                {
                    _dateCollection = new SchematizedLabeledPropertyCollection<DateTime?>(this, PropertyNames.DateCollection, PropertyNames.DateArrayNode, _CreateDate, _CommitDate);
                }
                return _dateCollection;
            }
        }

        /// <summary>Commit a url in the contact's collection of urls.</summary>
        /// <param name="contact">The contact that is having this url committed.</param>
        /// <param name="arrayNode">The array node where this url is being committed.</param>
        /// <param name="value">The url being committed.</param>
        private static void _CommitUrl(Contact contact, string arrayNode, Uri value)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.UrlCollection + PropertyNames.UrlArrayNode, StringComparison.Ordinal));
            string url = (null == value) ? null : value.ToString();
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Value, url);
        }

        // This is stricter than the contract provided by the COM Contact APIs.
        // It requires that the string in the contact is really a valid URI.  If it's not then
        // this will return a dummy http: string.
        private static Uri _CreateUrl(Contact contact, string arrayNode)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.UrlCollection + PropertyNames.UrlArrayNode, StringComparison.Ordinal));
            string value = contact.GetStringProperty(arrayNode + PropertyNames.Value);
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            Uri url;
            if (Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out url))
            {
                return url;
            }
            return new Uri(_stockInvalidUrl);
        }

        // Consider that this isn't a good idea.
        // The Contact isn't smart enough to actually restrict strings to URIs...
        // It's nice to enforce here, but it can make it so some values can't be read.
        /// <summary>
        /// The collection of Urls associated with this contact.
        /// </summary>
        /// <remarks>
        /// The APIs for storing Urls is more strict than the underlying schema requires.
        /// Some programs may place strings in the collection which are not valid Urls, but the contact
        /// is still recognized as valid.  Such strings will not be returned by the Urls
        /// ILabeledPropertyCollection, instead returning a string such as "http://invalid_url_in_contact".<para/>
        /// These strings can still be retrieved through this API by the GetStringProperty method.
        /// </remarks>
        public ILabeledPropertyCollection<Uri> Urls
        {
            get
            {
                _Validate();
                if (null == _urlCollection)
                {
                    _urlCollection = new SchematizedLabeledPropertyCollection<Uri>(this, PropertyNames.UrlCollection, PropertyNames.UrlArrayNode, _CreateUrl, _CommitUrl);
                }
                return _urlCollection;
            }
        }

        /// <summary>Commit an instant message struct in the contact's collection of instant messages.</summary>
        /// <param name="contact">The contact that is having this IM struct committed.</param>
        /// <param name="arrayNode">The array node where this IM struct is being committed.</param>
        /// <param name="value">The IM struct being committed.</param>
        private static void _CommitIM(Contact contact, string arrayNode, IMAddress value)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.IMAddressCollection + PropertyNames.IMAddressArrayNode, StringComparison.Ordinal));
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Value, value.Handle);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Protocol , value.Protocol);
        }

        private static IMAddress _CreateIM(Contact contact, string arrayNode)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.IMAddressCollection + PropertyNames.IMAddressArrayNode, StringComparison.Ordinal));
            string handle = contact.GetStringProperty(arrayNode + PropertyNames.Value);
            string protocol = contact.GetStringProperty(arrayNode + PropertyNames.Protocol);
            return new IMAddress(handle, protocol);
        }

        /// <summary>
        /// The collection of instant message addresses associated with this contact.
        /// </summary>
        public ILabeledPropertyCollection<IMAddress> IMAddresses
        {
            get
            {
                _Validate();
                if (null == _imCollection)
                {
                    _imCollection = new SchematizedLabeledPropertyCollection<IMAddress>(this, PropertyNames.IMAddressCollection, PropertyNames.IMAddressArrayNode, _CreateIM, _CommitIM);
                }
                return _imCollection;
            }
        }

        /// <summary>Commit an EmailAddress in the contact's collection of e-mail addresses.</summary>
        /// <param name="contact">The contact that is having this e-mail committed.</param>
        /// <param name="arrayNode">The array node where this e-mail is being committed.</param>
        /// <param name="value">The e-mail address being committed.</param>
        private static void _CommitEmail(Contact contact, string arrayNode, EmailAddress value)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.EmailAddressCollection + PropertyNames.EmailAddressArrayNode, StringComparison.Ordinal));
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Address, value.Address);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.AddressType, value.AddressType);
        }

        private static EmailAddress _CreateEmail(Contact contact, string arrayNode)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.EmailAddressCollection + PropertyNames.EmailAddressArrayNode, StringComparison.Ordinal));
            string address = contact.GetStringProperty(arrayNode + PropertyNames.Address);
            string type = contact.GetStringProperty(arrayNode + PropertyNames.AddressType);
            return new EmailAddress(address, type);
        }

        public string Phone
        {
            get
            {
				string phone = "";
				int i = 0;
                foreach (PhoneNumber p in this.PhoneNumbers)
                {
                    phone += " " + p.Number;
					if (++i >= 3) break;
                }
				phone = phone.Trim();
                return phone == "" ? null : phone;
            }
            set
            {
            }
        }

		string background = "";
		public string Background
		{
			get
			{
				return background;
			}

			set
			{
				background = value;
			}
		}

        public string FullName
        {
            get
            {
                lock (this)
                {
                    if (this.Names.Count > 0)
                    {
						string s = Name.FormatName(this.Names[0].GivenName, this.Names[0].MiddleName,
							this.Names[0].FamilyName, NameCatenationOrder.FamilyGivenMiddle);
						return s.Replace("?","");
                    }
                    else return null;
                }
            }
            set
            {
            }
        }

		public string GetFullName(NameNotation order)
		{
			lock (this)
			{
				if (this.Names.Count > 0)
				{
					string s = "";
					if (order == NameNotation.Formal)
						s = Name.FormatName(this.Names[0].GivenName, this.Names[0].MiddleName,
							this.Names[0].FamilyName, NameCatenationOrder.FamilyGivenMiddle);
					else
						s = Name.FormatName(this.Names[0].GivenName, this.Names[0].MiddleName,
							this.Names[0].FamilyName, NameCatenationOrder.GivenMiddleFamily);
					return s.Replace("?", "");

				}
				else return null;
			}
		}

        public string EMail
        {
            get
            {
				string phone = "";
				int i = 0;
				foreach (EmailAddress p in this.EmailAddresses)
				{
					phone += " " + p.Address;
					if (++i >= 2) break;
				}
				phone = phone.Trim();
				return phone == "" ? null : phone;
            }
            set
            {
            }
        }

        /// <summary>
        /// The collection of e-mail addresses associated with this contact.
        /// </summary>
        /// 

        public ILabeledPropertyCollection<EmailAddress> EmailAddresses
        {
            get
            {
                _Validate();
                if (null == _emailCollection)
                {
                    _emailCollection = new SchematizedLabeledPropertyCollection<EmailAddress>(this, PropertyNames.EmailAddressCollection, PropertyNames.EmailAddressArrayNode, _CreateEmail, _CommitEmail);
                }
                return _emailCollection;
            }
        }

        /// <summary>Commit a person in the contact's collection of people.</summary>
        /// <param name="contact">The contact that is having this person committed.</param>
        /// <param name="arrayNode">The array node where this person is being committed.</param>
        /// <param name="value">The person being committed.</param>
        private static void _CommitPerson(Contact contact, string arrayNode, Person value)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.PersonCollection + PropertyNames.PersonArrayNode, StringComparison.Ordinal));
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.FormattedName, value.Name);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.PersonId, value.Id);
        }

        // Not static.  Relies on associated Manager
        private Person _CreatePerson(Contact contact, string arrayNode)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.PersonCollection + PropertyNames.PersonArrayNode, StringComparison.Ordinal));
            string name = contact.GetStringProperty(arrayNode + PropertyNames.FormattedName);
            string id = contact.GetStringProperty(arrayNode + PropertyNames.PersonId);
            return new Person(name, id, _manager);
        }

        /// <summary>
        /// The collection of people associated with this contact.
        /// The relationship of each person to this contact is identified by the labels on the node.
        /// </summary>
        public ILabeledPropertyCollection<Person> People
        {
            get
            {
                _Validate();
                if (null == _personCollection)
                {
                    _personCollection = new SchematizedLabeledPropertyCollection<Person>(this, PropertyNames.PersonCollection, PropertyNames.PersonArrayNode, _CreatePerson, _CommitPerson);
                }
                return _personCollection;
            }
        }

        /// <summary>Commit a certificate in the contact's collection of certificates.</summary>
        /// <param name="contact">The contact that is having this certificate committed.</param>
        /// <param name="arrayNode">The array node where this certificate is being committed.</param>
        /// <param name="value">The certificate being committed.</param>
        private static void _CommitCertificate(Contact contact, string arrayNode, Certificate value)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.CertificateCollection + PropertyNames.CertificateArrayNode, StringComparison.Ordinal));
            contact._SetOrRemoveBinaryProperty(arrayNode + PropertyNames.Value, value.Value, value.ValueType);
            contact._SetOrRemoveBinaryProperty(arrayNode + PropertyNames.Thumbprint, value.Thumbprint, value.ThumbprintType);
        }

        private static Certificate _CreateCertificate(Contact contact, string arrayNode)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.CertificateCollection + PropertyNames.CertificateArrayNode, StringComparison.Ordinal));
            var sbType = new StringBuilder();
            Stream value = contact.GetBinaryProperty(arrayNode + PropertyNames.Value, sbType);
            var sbThumbPrintType = new StringBuilder();
            Stream thumbprint = contact.GetBinaryProperty(arrayNode + PropertyNames.Thumbprint, sbThumbPrintType);
            return new Certificate(value, sbType.ToString(), thumbprint, sbThumbPrintType.ToString());
        }

        /// <summary>
        /// The collection of certificates associated with this contact.
        /// </summary>
        public ILabeledPropertyCollection<Certificate> Certificates
        {
            get
            {
                _Validate();
                if (null == _certCollection)
                {
                    _certCollection = new SchematizedLabeledPropertyCollection<Certificate>(this, PropertyNames.CertificateCollection, PropertyNames.CertificateArrayNode, _CreateCertificate, _CommitCertificate);
                }
                return _certCollection;
            }
        }

        /// <summary>Commit a physical address in the contact's collection of addresses.</summary>
        /// <param name="contact">The contact that is having this address committed.</param>
        /// <param name="arrayNode">The array node where this address is being committed.</param>
        /// <param name="value">The address being committed.</param>
        private static void _CommitAddress(Contact contact, string arrayNode, PhysicalAddress value)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.PhysicalAddressCollection + PropertyNames.PhysicalAddressArrayNode, StringComparison.Ordinal));
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.AddressLabel, value.AddressLabel);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Locality, value.City);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Country, value.Country);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.ExtendedAddress, value.ExtendedAddress);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.POBox, value.POBox);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Region, value.State);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Street, value.Street);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.PostalCode, value.ZipCode);
        }

        private static PhysicalAddress _CreateAddress(Contact contact, string arrayNode)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.PhysicalAddressCollection + PropertyNames.PhysicalAddressArrayNode, StringComparison.Ordinal));
            string label = contact.GetStringProperty(arrayNode + PropertyNames.AddressLabel);
            string city = contact.GetStringProperty(arrayNode + PropertyNames.Locality);
            string country = contact.GetStringProperty(arrayNode + PropertyNames.Country);
            string extended = contact.GetStringProperty(arrayNode + PropertyNames.ExtendedAddress);
            string pobox = contact.GetStringProperty(arrayNode + PropertyNames.POBox);
            string state = contact.GetStringProperty(arrayNode + PropertyNames.Region);
            string street = contact.GetStringProperty(arrayNode + PropertyNames.Street);
            string zip = contact.GetStringProperty(arrayNode + PropertyNames.PostalCode);
            return new PhysicalAddress(pobox, street, city, state, zip, country, extended, label);
        }

        /// <summary>
        /// The collection of physical addresses associated with this contact.
        /// </summary>
        public ILabeledPropertyCollection<PhysicalAddress> Addresses
        {
            get
            {
                _Validate();
                if (null == _addressCollection)
                {
                    _addressCollection = new SchematizedLabeledPropertyCollection<PhysicalAddress>(this, PropertyNames.PhysicalAddressCollection, PropertyNames.PhysicalAddressArrayNode, _CreateAddress, _CommitAddress);
                }
                return _addressCollection;
            }
        }

        /// <summary>Commit a phone number in the contact's collection of numbers.</summary>
        /// <param name="contact">The contact that is having this number committed.</param>
        /// <param name="arrayNode">The array node where this number is being committed.</param>
        /// <param name="value">The number being committed.</param>
        private static void _CommitNumber(Contact contact, string arrayNode, PhoneNumber value)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.PhoneNumberCollection + PropertyNames.PhoneNumberArrayNode, StringComparison.Ordinal));
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Alternate, value.Alternate);
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Number, value.Number);
        }

        private static PhoneNumber _CreateNumber(Contact contact, string arrayNode)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.PhoneNumberCollection + PropertyNames.PhoneNumberArrayNode, StringComparison.Ordinal));
            string number = contact.GetStringProperty(arrayNode + PropertyNames.Number);
            string alternate = contact.GetStringProperty(arrayNode + PropertyNames.Alternate);
            return new PhoneNumber(number, alternate);
        }

        /// <summary>
        /// The collection of phone numbers associated with this contact.
        /// </summary>
        public ILabeledPropertyCollection<PhoneNumber> PhoneNumbers
        {
            get
            {
                _Validate();
                if (null == _numberCollection)
                {
                    _numberCollection = new SchematizedLabeledPropertyCollection<PhoneNumber>(this, PropertyNames.PhoneNumberCollection, PropertyNames.PhoneNumberArrayNode, _CreateNumber, _CommitNumber);
                }
                return _numberCollection;
            }
        }

        /// <summary>Commit a photo struct in the contact's collection of photos.</summary>
        /// <param name="contact">The contact that is having this photo committed.</param>
        /// <param name="arrayNode">The array node where this photo is being committed.</param>
        /// <param name="value">The photo being committed.</param>
        private static void _CommitPhoto(Contact contact, string arrayNode, Photo value)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.PhotoCollection + PropertyNames.PhotoArrayNode, StringComparison.Ordinal));
            contact._SetOrRemoveBinaryProperty(arrayNode + PropertyNames.Value, value.Value, value.ValueType);
            string url = null;
            if (null != value.Url)
            {
                url = value.Url.ToString();
            }
            contact._SetOrRemoveStringProperty(arrayNode + PropertyNames.Url, url);
        }

        private static Photo _CreatePhoto(Contact contact, string arrayNode)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.PhotoCollection + PropertyNames.PhotoArrayNode, StringComparison.Ordinal));
            Uri url = null;
            string urlString = contact.GetStringProperty(arrayNode + PropertyNames.Url);
            if (!string.IsNullOrEmpty(urlString))
            {
                if (!Uri.TryCreate(urlString, UriKind.RelativeOrAbsolute, out url))
                {
                    url = new Uri(_stockInvalidUrl);
                }
            }
            var photoType = new StringBuilder();
            Stream photoStream = contact.GetBinaryProperty(arrayNode + PropertyNames.Value, photoType);
            return new Photo(photoStream, null == photoStream ? null : photoType.ToString(), url);
        }

        /// <summary>
        /// The collection of photos associated with this contact.
        /// </summary>
        /// <remarks>
        /// Photos generally consist of either a data-stream with an associated MIME type, or a url
        /// with a location where the image is stored.  For when the Url is used, the APIs for storing
        /// Urls is more strict than the underlying schema requires.  Some programs may place strings
        /// in the collection which are not valid Urls, but the contact is still recognized as valid.
        /// Such strings will not be returned by the Urls ILabeledPropertyCollection, instead returning
        /// a string such as "http://invalid_url_in_contact".  Also for security reasons, some programs
        /// may ignore urls that do not represent a location the local file system.<para/>
        /// These strings can still be retrieved through this API by the GetStringProperty method.
        /// </remarks>
        public ILabeledPropertyCollection<Photo> Photos
        {
            get
            {
                _Validate();
                if (null == _photoCollection)
                {
                    _photoCollection = new SchematizedLabeledPropertyCollection<Photo>(this, PropertyNames.PhotoCollection, PropertyNames.PhotoArrayNode, _CreatePhoto, _CommitPhoto);
                }
                return _photoCollection;
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
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Only dispose of the IContactProperties if it's not potentially shared with others.
                // Otherwise set it to null and let the GC deal with it as it sees fit.
                if (!_sharedProperties)
                {
                    var disposable = _contactProperties as IDisposable;
                    Utility.SafeDispose(ref disposable);
                }
                _contactProperties = null;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
