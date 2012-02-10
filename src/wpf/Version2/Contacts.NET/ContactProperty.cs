/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Globalization;

    /// <summary>
    /// The different types of values a ContactProperty can represent.
    /// </summary>
    public enum ContactPropertyType
    {
        /// <summary>
        /// Default type.  Valid contact properties should not be of this type.
        /// </summary>
        None = 0,

        /// <summary>
        /// The array node type.  This represents a node in an hierarchical property set.
        /// </summary>
        /// <remarks>
        /// There is no valid Getter for this type.
        /// </remarks>
        ArrayNode = 1,

        /// <summary>
        /// The property type is a string.  Use GetStringProperty to retrieve the value.
        /// </summary>
        String = 2,

        /// <summary>
        /// The property type is a date.  Use GetDateProperty to retrieve the value.
        /// </summary>
        DateTime = 3,

        /// <summary>
        /// The property type is a binary stream and string type.  Use GetBinaryProperty to retrieve the value.
        /// </summary>
        Binary = 4,
    }

    /// <summary>
    /// Information about a specific property in a contact.
    /// </summary>
    /// <remarks>
    /// These are usually gotten through Contact's GetPropertyCollection enumerator.
    /// </remarks>
    public struct ContactProperty : IEquatable<ContactProperty>
    {
        #region Fields
        private readonly DateTime _modified;
        private readonly Guid _id;
        private readonly int _version;
        private readonly string _name;
        private readonly ContactPropertyType _type;
        private readonly bool _nil;
        #endregion

        internal ContactProperty(string name, ContactPropertyType type, int version, Guid id, DateTime modificationDate, bool isNil)
        {
            _name = name;
            _type = type;
            _version = version;
            _id = id;
            _modified = modificationDate;
            _nil = isNil;
        }

        /// <summary>
        /// Get the timestamp for when this property was last modified.
        /// </summary>
        public DateTime ModificationDate
        {
            get
            {
                return _modified;
            }
        }

        /// <summary>
        /// Get the unique identifier for this property.
        /// This is only meaningful for properties of type ArrayNode.
        /// </summary>
        /// <remarks>
        /// ArrayNode properties have Guids associated with them to help sync
        /// replicators recognize when two nodes came from the same source.
        /// </remarks>
        public Guid ElementId
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Get the version number for this property.  New properties have a version of 1.
        /// </summary>
        public int Version
        {
            get
            {
                return _version;
            }
        }

        /// <summary>
        /// Get the name for this property.  This can be used in conjunction with the Contact to get the value.
        /// </summary>
        /// <remarks>
        /// If this property has a ContactPropertyType of String, then to get the value you can call:
        /// <code>
        /// string propValue = contact.GetStringProperty(property.Name);
        /// </code>
        /// If this property has a ContactPropertyType of DateTime, then to get the value you can call:
        /// <code>
        /// DateTime? propTime = contact.GetDateProperty(property.Name);
        /// </code>
        /// If this property has a ContactPropertyType of Binary, then to get the value you can call:
        /// <code>
        /// StringBuilder propType = new StringBuilder();
        /// Stream propData = contact.GetBinaryProperty(property.Name, propType);
        /// </code>
        /// </remarks>
        public string Name
        {
            get
            {
                return _name ?? "";
            }
        }

        /// <summary>
        /// Get the type of this property.
        /// This signals what types of data are queryable for this property.
        /// </summary>
        public ContactPropertyType PropertyType
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Returns whether this property currently exists.
        /// </summary>
        /// <remarks>
        /// This indicates whether the property existed at one point but was later explicitly deleted.
        /// Removed array nodes stay a part of the contact's properties with their original ElementIds.
        /// This can be useful to clients looking at the history of a contact to determine whether a
        /// property was never set of if it was simply later removed.
        /// Deleted properties can later be set to new values in place which removes the nil-ness.
        /// </remarks>
        public bool Removed
        {
            get
            {
                return _nil;
            }
        }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            try
            {
                return Equals((ContactProperty)obj);
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return ElementId.GetHashCode() ^ Name.GetHashCode() ^ Version.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Name=\"{0}\" PropertyType=\"{1}\", Version=\"{2}\", ElementId=\"{3}\", ModificationDate=\"{4}\"",
                Name,
                PropertyType,
                Version,
                ElementId,
                ModificationDate);
        }

        #endregion

        public static bool operator ==(ContactProperty left, ContactProperty right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ContactProperty left, ContactProperty right)
        {
            return !(left == right);
        }

        #region IEquatable<ContactProperty> Members

        public bool Equals(ContactProperty other)
        {
            return ElementId == other.ElementId
                && ModificationDate == other.ModificationDate
                // case sensitive:
                && string.Equals(Name, other.Name, StringComparison.Ordinal)
                && PropertyType == other.PropertyType
                && Version == other.Version;
        }

        #endregion
    }
}
