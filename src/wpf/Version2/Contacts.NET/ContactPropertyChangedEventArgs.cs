/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// An enumeration of different types of changes that can occur on a Contact.
    /// </summary>
    public enum ContactPropertyChangeType
    {
        /// <summary>
        /// A property in the contact has been set.
        /// </summary>
        PropertySet,

        /// <summary>
        /// A property in the contact has been removed.
        /// </summary> 
        PropertyRemoved,

        /// <summary>
        /// A node has been appended to the contact's properties.
        /// </summary>
        NodeAppended,

        /// <summary>
        /// A node has been prepended to the contact's properties.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Prepended",
            Justification="Compliment to 'Appended', this is correctly spelled.")]
        NodePrepended,

        /// <summary>
        /// A node has been removed from the contact's properties.
        /// </summary>
        /// <remarks>
        /// The node's index is still present, but all values under it
        /// and labels associated with it have been cleared.
        /// </remarks>
        NodeRemoved,

        /// <summary>
        /// Labels have been added to a node in the contact.
        /// </summary>
        LabelsAdded,

        /// <summary>
        /// Labels have been removed from a node in the contact.
        /// </summary>
        LabelsRemoved,

        /// <summary>
        /// All labels have been removed from a node in the contact.
        /// </summary>
        LabelsCleared,

        /// <summary>
        /// The file path that backs this contact has changed.
        /// </summary>
        PathChanged,

        /// <summary>
        /// The runtime Id associated with this contact has changed.
        /// </summary>
        IdChanged,
    }

    /// <summary>
    /// EventArgs for PropertyChange events raised from a contact.
    /// </summary>
    public class ContactPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        private ContactPropertyChangeType _type;

        /// <summary>
        /// Create a new ContactPropertyChangedEventArgs object.
        /// </summary>
        /// <param name="propertyName">The name of the property being changed.</param>
        /// <param name="changeType">The type of change that has occurred.</param>
        public ContactPropertyChangedEventArgs(string propertyName, ContactPropertyChangeType changeType)
            : base(propertyName)
        {
            _type = changeType;
        }

        /// <summary>
        /// The nature of the change that was made to the contact.
        /// </summary>
        public ContactPropertyChangeType ChangeType
        {
            get
            {
                return _type;
            }
        }
    }
}