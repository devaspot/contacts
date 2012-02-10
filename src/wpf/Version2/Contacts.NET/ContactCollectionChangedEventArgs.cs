/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using Standard;
    using System;

    /// <summary>
    /// The different types of changes that can happen to a Contact collection.
    /// </summary>
    public enum ContactCollectionChangeType
    {
        /// <summary>
        /// Nothing changed.  This type should never be raised as a change event.
        /// </summary>
        NoChange = 0,

        /// <summary>
        /// A contact was added to the collection.
        /// </summary>
        Added,

        /// <summary>
        /// A contact was removed from the collection.
        /// </summary>
        Removed,

        /// <summary>
        /// A contact in the collection had its storage relocated.
        /// </summary>
        Moved,

        /// <summary>
        /// Properties in a contact may have been updated.
        /// </summary>
        Updated,
    }

    /// <summary>
    /// The EventArgs raised when a change has been made to the contact collection.
    /// </summary>
    public class ContactCollectionChangedEventArgs : EventArgs
    {
        private readonly string _oldId;
        private readonly string _newId;
        private readonly ContactCollectionChangeType _change;

        /// <summary>
        /// Create an ContactCollectionChangedEventArgs object.
        /// </summary>
        /// <param name="change">The type of change being signaled by the event (must be Added or Removed).</param>
        /// <param name="contactId">The id of the contact being added or removed.</param>
        /// <exception cref="System.ArgumentException">
        /// An invalid ContactCollectionChangeType was provided.
        /// This constructor can only be used with the Added and Removed types.
        /// </exception>
        public ContactCollectionChangedEventArgs(ContactCollectionChangeType change, string contactId)
        {
            if (change != ContactCollectionChangeType.Added
                && change != ContactCollectionChangeType.Removed)
            {
                throw new ArgumentException("This constructor can only be used for the ChangeTypes 'Added' and 'Removed'.");
            }
            Verify.IsNeitherNullNorEmpty(contactId, "contactId");

            _oldId = null;
            _newId = null;
            switch (change)
            {
                case ContactCollectionChangeType.Added:
                    _newId = contactId;
                    break;
                case ContactCollectionChangeType.Removed:
                    _oldId = contactId;
                    break;
            }
            _change = change;
        }

        /// <summary>
        /// Create an ContactCollectionChangedEventArgs object.
        /// </summary>
        /// <param name="change">The type of change being signaled by the event (must be Moved or Updated).</param>
        /// <param name="oldContactId">The original id of the affected contact before the change.</param>
        /// <param name="newContactId">The id of the contact after the change.  This may be the same as the old id.</param>
        /// <exception cref="System.ArgumentException">
        /// An invalid ContactCollectionChangeType was provided.
        /// This constructor can only be used with the Moved and Updated types.
        /// </exception>
        public ContactCollectionChangedEventArgs(ContactCollectionChangeType change, string oldContactId, string newContactId)
        {
            if (change != ContactCollectionChangeType.Moved
                && change != ContactCollectionChangeType.Updated)
            {
                throw new ArgumentException("This constructor can only be used for the ChangeType 'Moved' and 'Updated'.");
            }
            Verify.IsNeitherNullNorEmpty(newContactId, "newContactId");
            Verify.IsNeitherNullNorEmpty(oldContactId, "oldContactId");

            _oldId = oldContactId;
            _newId = newContactId;
            _change = change;
        }

        /// <summary>
        /// The original id of the affected contact.
        /// </summary>
        /// <remarks>
        /// If the contact was removed, its id is available through this property.
        /// </remarks>
        public string OldId
        {
            get
            {
                return _oldId;
            }
        }

        /// <summary>
        /// The new id of the affected contact.
        /// </summary>
        /// <remarks>
        /// If the contact was added, its id is available through this property.
        /// </remarks>
        public string NewId
        {
            get
            {
                return _newId;
            }
        }

        /// <summary>
        /// The type of change that was made to the collection.
        /// </summary>
        public ContactCollectionChangeType Change
        {
            get
            {
                return _change;
            }
        }

    }
}