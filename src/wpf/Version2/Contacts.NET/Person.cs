/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

//
// CONSIDER:
//   Generally the types are using StringComparison.CurrentCulture as their
//   default comparison heuristic.  This may potentially violate the GetHashCode
//   contract that says that two objects that are Equal should also have the same
//   hash code.  GetHashCode doesn't have an overload that accepts a CultureInfo
//   or a StringComparison.  There are probably reasonable mitigations to this,
//   but pragmatically the current implementation is probably okay, and
//   CurrentCulture is the right default given the nature of the data.
//

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using Standard;

    public struct Person : IEquatable<Person>
    {
        // Use this StringComparison to be consistent.  Overloads are provided to callers who want something else.
        private const StringComparison _DefaultPersonStringComparison = StringComparison.CurrentCulture;

        private const string _EmailToken = "/EMAIL:";
        private const string _PhoneToken = "/PHONE:";
        private const string _PreferredLabelToken = "/PREFLABEL:";

        #region Fields
        // The distinction between explicit and implicit properties isn't currently exposed
        // through the public properties.  Keeping them separate internally for debugging,
        // or if we decide later to expose ExplicitXxx versions of the properties.

        // Properties explicitly set by the constructor
        private readonly string _explicitName;
        private readonly string _explicitPhone;
        private readonly string _explicitEmail;

        // Properties inferred by a Contact given in the constructor.
        private readonly string _contactName;
        private readonly string _contactPhone;
        private readonly string _contactEmail;

        // Properties read from the Id given in the constructor.
        private readonly string _id;
        private readonly string _preferredLabel;
        private readonly ContactTypes _type;
        
        #endregion

        #region Internal methods shared with PersonBuilder

        private static string GenerateId(string contactId, string phone, string email, string preferredLabel)
        {
            const string tokenFormat = "{0}\"{1}\" ";

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(contactId))
            {
                sb.Append(contactId).Append(' ');
            }
            if (null != phone)
            {
                sb.AppendFormat(tokenFormat, _PhoneToken, phone);
            }
            if (null != email)
            {
                sb.AppendFormat(tokenFormat, _EmailToken, email);
            }
            if (null != preferredLabel)
            {
                sb.AppendFormat(tokenFormat, _PreferredLabelToken, preferredLabel);
            }
            if (sb.Length > 0)
            {
                Assert.AreEqual(sb.ToString(sb.Length - 1, 1), " ");
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        internal static void ParseId(string id, out string phone, out string email, out string preferredLabel)
        {
            phone = null;
            email = null;
            preferredLabel = null;

            id = (id ?? "").Trim();
            if (id.Length != 0)
            {
                Dictionary<string, string> tokens = ContactId.TokenizeId(id);
                tokens.TryGetValue(_EmailToken, out email);
                tokens.TryGetValue(_PhoneToken, out phone);
                tokens.TryGetValue(_PreferredLabelToken, out preferredLabel);
            }
        }

        #endregion

        public Person(ContactManager manager, string contactId, string name, string phone, string email, string preferredLabel)
        {
            Contact contact = null;
            try
            {
                if (null != manager && manager.TryGetContact(contactId, out contact))
                {
                    this = new Person(contact, name, phone, email, preferredLabel);
                }
                else
                { 
                    _contactEmail = null;
                    _contactName = null;
                    _contactPhone = null;
                    _type = ContactTypes.None;

                    _explicitEmail = email;
                    _explicitName = name;
                    _explicitPhone = phone;
                    _preferredLabel = preferredLabel;
                    _id = GenerateId(contactId, phone, email, preferredLabel);
                }
            }
            finally
            {
                Utility.SafeDispose(ref contact);
            }
        }

        public Person(Contact contact, string name, string phone, string email, string preferredLabel)
        {
            _explicitName = name;
            _explicitPhone = phone;
            _explicitEmail = email;
            _preferredLabel = preferredLabel;

            // Set these to defaults, but will try to override the values in the constructor.
            _contactEmail = null;
            _contactName = null;
            _contactPhone = null;
            _type = ContactTypes.None;

            if (null != contact)
            {
                _type = contact.ContactType;

                if (!string.IsNullOrEmpty(_preferredLabel))
                {
                    int index;
                    index = contact.Names.IndexOfLabels(_preferredLabel);
                    if (-1 != index)
                    {
                        _contactName = contact.Names[index].FormattedName;
                    }
                    index = contact.PhoneNumbers.IndexOfLabels(_preferredLabel);
                    if (-1 != index)
                    {
                        _contactPhone = contact.PhoneNumbers[index].Number;
                    }
                    index = contact.EmailAddresses.IndexOfLabels(_preferredLabel);
                    if (-1 != index)
                    {
                        _contactEmail = contact.EmailAddresses[index].Address;
                    }
                }
                if (null == _contactEmail)
                {
                    _contactEmail = contact.EmailAddresses.Default.Address;
                }
                if (null == _contactName)
                {
                    _contactName = contact.Names.Default.FormattedName;
                }
                if (null == _contactPhone)
                {
                    _contactPhone = contact.PhoneNumbers.Default.Number;
                }
            }
            _id = GenerateId((null == contact ? "" : contact.Id), _explicitPhone, _explicitEmail, _preferredLabel);
        }

        public Person(string name, string id)
            : this(name, id, null)
        { }

        public Person(string name, string id, ContactManager manager)
        {
            _explicitName = name;
            _id = id;

            _contactEmail = null;
            _contactName = null;
            _contactPhone = null;
            _type = ContactTypes.None;

            ParseId(id, out _explicitPhone, out _explicitEmail, out _preferredLabel);

            if (null != manager)
            {
                Contact contact = null;
                try
                {
                    if (manager.TryGetContact(id, out contact))
                    {
                        _type = contact.ContactType;

                        if (!string.IsNullOrEmpty(_preferredLabel))
                        {
                            int index;
                            index = contact.Names.IndexOfLabels(_preferredLabel);
                            if (-1 != index)
                            {
                                _contactName = contact.Names[index].FormattedName;
                            }
                            index = contact.PhoneNumbers.IndexOfLabels(_preferredLabel);
                            if (-1 != index)
                            {
                                _contactPhone = contact.PhoneNumbers[index].Number;
                            }
                            index = contact.EmailAddresses.IndexOfLabels(_preferredLabel);
                            if (-1 != index)
                            {
                                _contactEmail = contact.EmailAddresses[index].Address;
                            }
                        }
                        if (null == _contactEmail)
                        {
                            _contactEmail = contact.EmailAddresses.Default.Address;
                        }
                        if (null == _contactName)
                        {
                            _contactName = contact.Names.Default.FormattedName;
                        }
                        if (null == _contactPhone)
                        {
                            _contactPhone = contact.PhoneNumbers.Default.Number;
                        }
                    }
                }
                finally
                {
                    Utility.SafeDispose(ref contact);
                }
            }
        }

        public Person(string name, string phone, string email, string preferredLabel)
            : this(null, name, phone, email, preferredLabel)
        { }

        public Person(Contact contact)
            : this(contact, null, null, null, null)
        { }

        public Person(string name)
            : this(name, null, null)
        { }

        public string Name
        {
            get { return _explicitName ?? _contactName ?? ""; }
        }

        public string Id
        {
            get { return _id ?? ""; }
        }

        public string Email
        {
            get { return _explicitEmail ?? _contactEmail ?? ""; }
        }

        public string Phone
        {
            get { return _explicitPhone ?? _contactPhone ?? ""; }
        }

        public string PreferredLabel
        {
            get { return _preferredLabel ?? ""; }
        }

        public ContactTypes ContactType
        {
            get { return _type; }
        }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            try
            {
                return Equals((Person)obj, _DefaultPersonStringComparison);
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            var ret = new StringBuilder();
            Utility.GeneratePropertyString(ret, "FormattedName", Name);
            Utility.GeneratePropertyString(ret, "Email", Email);
            Utility.GeneratePropertyString(ret, "Phone", Phone);
            Utility.GeneratePropertyString(ret, "Id", Id);
            Utility.GeneratePropertyString(ret, "PreferredLabel", PreferredLabel);
            return ret.ToString();
        }

        #endregion

        public bool Equals(Person other, StringComparison comparisonType)
        {
            return string.Equals(Id, other.Id, comparisonType)
                && string.Equals(Name, other.Name, comparisonType);
        }

        #region IEquatable<Person> Members

        public bool Equals(Person other)
        {
            return Equals(other, _DefaultPersonStringComparison);
        }

        #endregion

        public static bool operator ==(Person left, Person right)
        {
            return left.Equals(right, _DefaultPersonStringComparison);
        }

        public static bool operator !=(Person left, Person right)
        {
            return !left.Equals(right, _DefaultPersonStringComparison);
        }

        public static implicit operator Person(string name)
        {
            return new Person(name);
        }

        public static implicit operator Person(Contact contact)
        {
            return new Person(contact);
        }
    }

    public class PersonBuilder : INotifyPropertyChanged
    {
        private string _name;
        private string _id;
        private string _email;
        private string _phone;
        private string _preferredLabel;

        private void _OnPropertyChanged(string propertyName)
        {
            Assert.IsFalse(string.IsNullOrEmpty(propertyName));

            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public PersonBuilder(string name)
            : this(name, null)
        { }

        public PersonBuilder(string name, string id)
        {
            _name = name;
            _id = id;
            Person.ParseId(_id, out _phone, out _email, out _preferredLabel);
        }

        public PersonBuilder(Person person)
            : this(person.Name, person.Id)
        { }

        public PersonBuilder()
            : this(null, null)
        { }

        public string Name
        {
            get { return _name ?? ""; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    _OnPropertyChanged("Name");
                }
            }
        }

        public string ContactId
        {
            get { return _id ?? ""; }
            // This doesn't replace any values of phone, email, or preferredLabels.
            set
            {
                if (_id != value)
                {
                    _id = value;
                    _OnPropertyChanged("Id");
                }
            }
        }

        public string Phone
        {
            get { return _phone; }
            set
            {
                if (_phone != value)
                {
                    _phone = value;
                    _OnPropertyChanged("Phone");
                }
            }
        }

        public string Email
        {
            get { return _email; }
            set
            {
                if (_email != value)
                {
                    _email = value;
                    _OnPropertyChanged("Email");
                }
            }
        }

        public string PreferredLabel
        {
            get { return _preferredLabel; }
            set
            {
                if (_preferredLabel != value)
                {
                    _preferredLabel = value;
                    _OnPropertyChanged("PreferredLabel");
                }
            }
        }

        public Person ToPerson()
        {
            return new Person(null, ContactId, Name, Phone, Email, PreferredLabel);
        }

        public Person ToPerson(ContactManager manager)
        {
            return new Person(manager, ContactId, Name, Phone, Email, PreferredLabel);
        }

        public static implicit operator Person(PersonBuilder builder)
        {
            if (null == builder)
            {
                return default(Person);
            }
            return builder.ToPerson();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
