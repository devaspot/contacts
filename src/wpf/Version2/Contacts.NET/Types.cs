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
    using System.ComponentModel;
    using System.IO;
    using System.Net;
    using System.Text;
    using Standard;

    public enum NameCatenationOrder
    {
        None,
        GivenMiddleFamily,
        FamilyGivenMiddle,
        FamilyCommaGivenMiddle,
        GivenFamily,
        FamilyGiven,
        FamilyCommaGiven,
    }

    /// <summary>
    /// Immutable collection of related name information for a contact.
    /// </summary>
    public struct Name : IEquatable<Name>
    {
        #region Fields
        private const StringComparison _DefaultNameStringComparison = StringComparison.CurrentCulture;

        private  string _formattedName;
        private  string _phonetic;
        private  string _prefix;
        private  string _personalTitle;
        private  string _givenName;
        private  string _familyName;
        private  string _middleName;
        private  string _generation;
        private  string _suffix;
        private  string _nickname;

        // Since the object is immutable only need to calculate these once.
        // Since the object is a struct, need to wait until the values are requested.
        private int _hashCode;
        private string _toString;
        #endregion

        /// <summary>Create a new Name property set.</summary>
        /// <param name="formattedName">The name as it should be displayed.  Free form text.</param>
        /// <param name="phonetic">
        /// The name as it should be pronounced.  Can be a URL or accented characters.  Free form text.
        /// </param>
        /// <param name="prefix">Prefix.  Free form text.</param>
        /// <param name="title">Honorific, such as Mr., Ms., Dr., etc.  Free form text.</param>
        /// <param name="given">First name.  Free form text.</param>
        /// <param name="middle">Middle name.  Free form text.</param>
        /// <param name="family">Last, or Family, name.  Free form text.</param>
        /// <param name="generation">Generation, such as Jr, or III.  Free form text.</param>
        /// <param name="suffix">Suffix.  Free form text.</param>
        /// <param name="nickname">Nickname.  Free form text.</param>
        public Name(
            string formattedName,
            string phonetic,
            string prefix,
            string title,
            string given,
            string middle,
            string family,
            string generation, 
            string suffix,
            string nickname)
        {
            _formattedName = formattedName;
            _phonetic = phonetic;
            _prefix = prefix;
            _personalTitle = title;
            _givenName = given;
            _middleName = middle;
            _familyName = family;
            _generation = generation;
            _suffix = suffix;
            _nickname = nickname;

            _hashCode = default(int);
            _toString = default(string);
        }

        /// <summary>Create a new Name property set.</summary>
        /// <param name="given">First name.  Free form text.</param>
        /// <param name="middle">Middle name.  Free form text.</param>
        /// <param name="family">Last, or Family, name.  Free form text.</param>
        /// <param name="formattedOrder">
        /// The format to use for the implicit FormattedName.
        /// If this parameter is None, then no FormattedName is set for this Name.
        /// </param>
        /// <remarks>
        /// This constructor implicitly sets the FormattedName property based on the provided
        /// fields and catenation order.
        /// </remarks>
        public Name(string given, string middle, string family, NameCatenationOrder formattedOrder)
            : this(FormatName(given, middle, family, formattedOrder), null, null, null, given, middle, family, null, null, null)
        {}

        /// <summary>Create a new Name property set.</summary>
        /// <param name="formattedName">The name as it should be displayed.  Free form text.</param>
        public Name(string formattedName)
            : this(formattedName, null, null, null, null, null, null, null, null, null)
        {}

        private static void _CatUtil(StringBuilder source, string separator, string append)
        {
            Assert.IsNotNull(source);
            Assert.IsNotNull(separator);

            if (!string.IsNullOrEmpty(append))
            {
                if (0 != source.Length)
                {
                    source.Append(separator);
                }
                source.Append(append);
            }
        }

        /// <summary>Generate a formatted name.</summary>
        /// <param name="given">First name.  Free form text.</param>
        /// <param name="middle">Middle name.  Free form text.</param>
        /// <param name="family">Last, or Family, name.  Free form text.</param>
        /// <param name="formattedOrder">The format to use for the return value.  If this parameter is None an empty string is returned.</param>
        /// <returns>A formatted name based on the provided fields and catenation order.</returns>
        public static string FormatName(string given, string middle, string family, NameCatenationOrder formattedOrder)
        {
            var sb = new StringBuilder();
            switch (formattedOrder)
            {
                case NameCatenationOrder.FamilyCommaGiven:
                    sb.Append(family);
                    _CatUtil(sb, ", ", given);
                    break;

                case NameCatenationOrder.FamilyCommaGivenMiddle:
                    // This is the only case where the second separator is conditional.
                    string secondSeparator = (null != family && null == given) ? ", " : " ";
                    sb.Append(family);
                    _CatUtil(sb, ", ", given);
                    _CatUtil(sb, secondSeparator, middle);
                    break;

                case NameCatenationOrder.FamilyGiven:
                    sb.Append(family);
                    _CatUtil(sb, " ", given);
                    break;

                case NameCatenationOrder.FamilyGivenMiddle:
                    sb.Append(family);
                    _CatUtil(sb, " ", given);
                    _CatUtil(sb, " ", middle);
                    break;

                case NameCatenationOrder.GivenFamily:
                    sb.Append(given);
                    _CatUtil(sb, " ", family);
                    break;

                case NameCatenationOrder.GivenMiddleFamily:
                    sb.Append(given);
                    _CatUtil(sb, " ", middle);
                    _CatUtil(sb, " ", family);
                    break;

                case NameCatenationOrder.None:
                    break;

                default:
                    throw new ArgumentException("Invalid name catenation ordering.", "formattedOrder");
            }
            return sb.ToString();
        }

        /// <summary>The name as it should be displayed.  Free form text.</summary>
        public string FormattedName
        {
            get { return _formattedName ?? ""; }
			set { _formattedName = value; } 
        }

        /// <summary>
        /// The name as it should be pronounced.  Can be a URL or accented characters.  Free form text.
        /// </summary>
        public string Phonetic
        {
            get { return _phonetic ?? ""; }
			set { _phonetic = value; }
        }
        
        /// <summary>
        /// Prefix.  Free form text.
        /// </summary>
        public string Prefix
        {
            get { return _prefix ?? ""; }
			set { _prefix = value; }
        }

        /// <summary>
        /// Honorific, such as Mr., Ms., Dr., etc.  Free form text.
        /// </summary>
        public string PersonalTitle
        {
            get { return _personalTitle ?? ""; }
			set { _personalTitle = value; }
        }

        /// <summary>
        /// First name.  Free form text.
        /// </summary>
        public string GivenName
        {
            get { return _givenName ?? ""; }
			set { _givenName = value; }
        }

        /// <summary>
        /// Last, or Family, name.  Free form text.
        /// </summary>
        public string FamilyName
        {
            get { return _familyName ?? ""; }
			set { _familyName = value; }
        }

        /// <summary>
        /// Middle name.  Free form text.
        /// </summary>
        public string MiddleName
        {
            get { return _middleName ?? ""; }
			set { _middleName = value; }
        }

        /// <summary>
        /// Generation, such as Jr, or III.  Free form text.
        /// </summary>
        public string Generation
        {
            get { return _generation ?? ""; }
			set { _generation = null; }
        }

        /// <summary>
        /// Suffix.  Free form text.
        /// </summary>
        public string Suffix
        {
            get { return _suffix ?? ""; }
			set { _suffix = value; }
        }

        /// <summary>
        /// Nickname.  Free form text.
        /// </summary>
        public string Nickname
        {
            get { return _nickname ?? ""; }
			set { _nickname = value; }
        }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            try
            {
                return Equals((Name)obj, _DefaultNameStringComparison);
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            if (default(int) == _hashCode)
            {
                var sb = new StringBuilder();
                // Use '-' as a placeholder for null values.
                // Helps prevent a Name with only "Joe" as the GivenName
                // compare equal to a Name with only "Joe" as the FormattedName.
                sb.Append(_familyName ?? "-")
                    .Append(_formattedName ?? "-")
                    .Append(_generation ?? "-")
                    .Append(_givenName ?? "-")
                    .Append(_middleName ?? "-")
                    .Append(_nickname ?? "-")
                    .Append(_personalTitle ?? "-")
                    .Append(_phonetic ?? "-")
                    .Append(_prefix ?? "-")
                    .Append(_suffix ?? "-");
                _hashCode = sb.ToString().GetHashCode();
            }
            return _hashCode;
        }

        public override string ToString()
        {
            if (default(string) == _toString)
            {
                string ret = "<missing-name>";
                if (!string.IsNullOrEmpty(FormattedName))
                {
                    ret = FormattedName;
                }
                else
                {
                    string generated = FormatName(GivenName, MiddleName, FamilyName, NameCatenationOrder.GivenMiddleFamily);
                    if (!string.IsNullOrEmpty(generated))
                    {
                        ret = generated;
                    }
                }
                _toString = ret;
            }
            return _toString;
        }

        #endregion

        /// <summary>Compare this Name to another for equality.</summary>
        /// <param name="other">The other Name to compare.</param>
        /// <param name="comparisonType">The comparison type to use for string properties.</param>
        /// <returns>Whether the other Name is equal to this one.</returns>
        public bool Equals(Name other, StringComparison comparisonType)
        {
            return string.Equals(FamilyName, other.FamilyName, comparisonType)
                && string.Equals(FormattedName, other.FormattedName, comparisonType)
                && string.Equals(Generation, other.Generation, comparisonType)
                && string.Equals(GivenName, other.GivenName, comparisonType)
                && string.Equals(MiddleName, other.MiddleName, comparisonType)
                && string.Equals(Nickname, other.Nickname, comparisonType)
                && string.Equals(PersonalTitle, other.PersonalTitle, comparisonType)
                && string.Equals(Phonetic, other.Phonetic, comparisonType)
                && string.Equals(Prefix, other.Prefix, comparisonType)
                && string.Equals(Suffix, other.Suffix, comparisonType);
        }

        #region IEquatable<Name> Members

        /// <summary>Compare this Name to another for equality.</summary>
        /// <param name="other">The other Name to compare.</param>
        /// <returns>Whether the other Name is equal to this one.</returns>
        /// <remarks>
        /// The string comparisons of the contained properties is case-sensitive and culture invariant.
        /// For flexibility regarding that please use an appropriate overload of the Equals method.
        /// </remarks>
        public bool Equals(Name other)
        {
            return Equals(other, _DefaultNameStringComparison);
        }

        #endregion

        /// <summary>Compare two Names for equality.</summary>
        /// <param name="left">First name to compare.</param>
        /// <param name="right">Second name to compare.</param>
        /// <returns>Whether the two names are equal.</returns>
        /// <remarks>
        /// The string comparisons of the contained properties is case-sensitive and culture invariant.
        /// For flexibility regarding that please use an appropriate overload of the Equals method.
        /// </remarks>
        public static bool operator ==(Name left, Name right)
        {
            return left.Equals(right, _DefaultNameStringComparison);
        }

        /// <summary>Compare two Names for inequality.</summary>
        /// <param name="left">First name to compare.</param>
        /// <param name="right">Second name to compare.</param>
        /// <returns>Whether the two names are not equal.</returns>
        /// <remarks>
        /// The string comparisons of the contained properties is case-sensitive and culture invariant.
        /// For flexibility regarding that please use an appropriate overload of the Equals method.
        /// </remarks>
        public static bool operator !=(Name left, Name right)
        {
            return !left.Equals(right, _DefaultNameStringComparison);
        }

        public static implicit operator Name(string formattedName)
        {
            return new Name(formattedName);
        }
    }

    /// <summary>
    /// Mutable collection of related name information for a contact.
    /// </summary>
    public class NameBuilder : INotifyPropertyChanged
    {
        private string _formattedName;
        private string _phonetic;
        private string _prefix;
        private string _personalTitle;
        private string _givenName;
        private string _familyName;
        private string _middleName;
        private string _generation;
        private string _suffix;
        private string _nickname;

        private void _OnPropertyChanged(string propertyName)
        {
            Assert.IsFalse(string.IsNullOrEmpty(propertyName));

            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>Create a new mutable name property set.</summary>
        /// <param name="formattedName">The name as it should be displayed.  Free form text.</param>
        /// <param name="phonetic">
        /// The name as it should be pronounced.  Can be a URL or accented characters.  Free form text.
        /// </param>
        /// <param name="prefix">Prefix.  Free form text.</param>
        /// <param name="title">Honorific, such as Mr., Ms., Dr., etc.  Free form text.</param>
        /// <param name="given">First name.  Free form text.</param>
        /// <param name="middle">Middle name.  Free form text.</param>
        /// <param name="family">Last, or Family, name.  Free form text.</param>
        /// <param name="generation">Generation, such as Jr, or III.  Free form text.</param>
        /// <param name="suffix">Suffix.  Free form text.</param>
        /// <param name="nickname">Nickname.  Free form text.</param>
        public NameBuilder(
            string formattedName,
            string phonetic,
            string prefix,
            string title,
            string given,
            string middle,
            string family,
            string generation, 
            string suffix,
            string nickname)
        {
            _formattedName = formattedName;
            _phonetic = phonetic;
            _prefix = prefix;
            _personalTitle = title;
            _givenName = given;
            _middleName = middle;
            _familyName = family;
            _generation = generation;
            _suffix = suffix;
            _nickname = nickname;
        }

        /// <summary>Create a new mutable name property set.</summary>
        /// <param name="given">First name.  Free form text.</param>
        /// <param name="middle">Middle name.  Free form text.</param>
        /// <param name="family">Last, or Family, name.  Free form text.</param>
        /// <param name="formattedOrder">
        /// The format to use for the implicit FormattedName.
        /// If this parameter is None, then no FormattedName is set for this Name.
        /// </param>
        /// <remarks>
        /// This constructor implicitly sets the FormattedName property based on the provided
        /// fields and catenation order.
        /// </remarks>
        public NameBuilder(string given, string middle, string family, NameCatenationOrder formattedOrder)
            : this(Name.FormatName(given, middle, family, formattedOrder), null, null, null, given, middle, family, null, null, null)
        { }

        /// <summary>Create a new mutable name property set.</summary>
        /// <param name="formattedName">The name as it should be displayed.  Free form text.</param>
        public NameBuilder(string formattedName)
            : this(formattedName, null, null, null, null, null, null, null, null, null)
        { }

        /// <summary>Create a new mutable name property set with default property values.</summary>
        public NameBuilder()
            : this(null, null, null, null, null, null, null, null, null, null)
        { }

        /// <summary>
        /// Create a new mutable name property set.
        /// </summary>
        /// <param name="name">The source of the starting name information.</param>
        /// <remarks>
        /// This creates a mutable copy of the given Name struct.
        /// </remarks>
        public NameBuilder(Name name)
            : this(
                name.FormattedName,
                name.Phonetic,
                name.Prefix, 
                name.PersonalTitle,
                name.GivenName, 
                name.MiddleName, 
                name.FamilyName, 
                name.Generation, 
                name.Suffix, 
                name.Nickname)
        { }

        /// <summary>The name as it should be displayed.  Free form text.</summary>
        public string FormattedName
        {
            get { return _formattedName ?? ""; }
            set
            { 
                if (_formattedName != value)
                {
                    _formattedName = value;
                    _OnPropertyChanged("FormattedName");
                }
            }
        }

        public string Phonetic
        {
            get { return _phonetic ?? ""; }
            set
            {
                if (_phonetic != value)
                {
                    _phonetic = value;
                    _OnPropertyChanged("Phonetic");
                }
            }
        }

        public string Prefix
        {
            get { return _prefix ?? ""; }
            set
            {
                if (_prefix != value)
                {
                    _prefix = value;
                    _OnPropertyChanged("Prefix");
                }
            }
        }

        public string PersonalTitle
        {
            get { return _personalTitle ?? ""; }
            set
            {
                if (_personalTitle != value)
                {
                    _personalTitle = value;
                    _OnPropertyChanged("PersonalTitle");
                }
            }
        }

        public string GivenName
        {
            get { return _givenName ?? ""; }
            set
            {
                if (_givenName != value)
                {
                    _givenName = value;
                    _OnPropertyChanged("GivenName");
                }
            }
        }

        public string FamilyName
        {
            get { return _familyName ?? ""; }
            set
            {
                if (_familyName != value)
                {
                    _familyName = value;
                    _OnPropertyChanged("FamilyName");
                }
            }
        }

        /// <summary>
        /// Middle name.  Free form text.
        /// </summary>
        public string MiddleName
        {
            get { return _middleName ?? ""; }
            set
            {
                if (_middleName != value)
                {
                    _middleName = value;
                    _OnPropertyChanged("MiddleName");
                }
            }
        }

        public string Generation
        {
            get { return _generation ?? ""; }
            set
            {
                if (_generation != value)
                {
                    _generation = value;
                    _OnPropertyChanged("Generation");
                }
            }
        }

        public string Suffix
        {
            get { return _suffix ?? ""; }
            set
            {
                if (_suffix != value)
                {
                    _suffix = value;
                    _OnPropertyChanged("Suffix");
                }
            }
        }

        public string Nickname
        {
            get { return _nickname ?? ""; }
            set
            {
                if (_nickname != value)
                {
                    _nickname = value;
                    _OnPropertyChanged("Nickname");
                }
            }
        }

        public Name ToName()
        {
            return new Name(
                FormattedName,
                Phonetic,
                Prefix,
                PersonalTitle,
                GivenName,
                MiddleName,
                FamilyName,
                Generation,
                Suffix,
                Nickname);
        }

        public static implicit operator Name(NameBuilder builder)
        {
            if (null == builder)
            {
                return default(Name);
            }
            return builder.ToName();
        }

        #region Object Overrides

        public override string ToString()
        {
            return ToName().ToString();
        }

        public override int GetHashCode()
        {
            return ToName().GetHashCode();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public struct Position : IEquatable<Position>
    {
        #region Fields
        private const StringComparison _DefaultPositionStringComparison = StringComparison.CurrentCulture;

        private readonly string _organization;
        private readonly string _role;
        private readonly string _company;
        private readonly string _department;
        private readonly string _office;
        private readonly string _title;
        private readonly string _profession;

        private int _hashCode;
        private string _toString;
        #endregion

        public Position(string organization, string role, string company, string department, string office, string title, string profession)
        {
            _organization = organization;
            _role = role;
            _company = company;
            _department = department;
            _office = office;
            _title = title;
            _profession = profession;

            _hashCode = default(int);
            _toString = default(string);
        }

        public string Organization
        {
            get { return _organization ?? ""; }
        }

        public string Role
        {
            get { return _role ?? ""; }
        }

        public string Company
        {
            get { return _company ?? ""; }
        }

        public string Department
        {
            get { return _department ?? ""; }
        }

        public string Office
        {
            get { return _office ?? ""; }
        }

        public string JobTitle
        {
            get { return _title ?? ""; }
        }

        public string Profession
        {
            get { return _profession ?? ""; }
        }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            try
            {
                return Equals((Position)obj, _DefaultPositionStringComparison);
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            if (default(int) == _hashCode)
            {
                var sb = new StringBuilder();
                sb.Append(_company ?? "-")
                    .Append(_department ?? "-")
                    .Append(_office ?? "-")
                    .Append(_organization ?? "-")
                    .Append(_profession ?? "-")
                    .Append(_role ?? "-")
                    .Append(_title ?? "-");
                _hashCode = sb.ToString().GetHashCode();
            }
            return _hashCode;
        }

        public override string ToString()
        {
            if (default(string) == _toString)
            {
                _toString = _title ?? "<missing-title>" + " @ " + _company ?? "<missing-company>";
            }
            return _toString;
        }

        #endregion

        public bool Equals(Position other, StringComparison comparisonType)
        {
            return string.Equals(Company, other.Company, comparisonType)
                && string.Equals(Department, other.Department, comparisonType)
                && string.Equals(JobTitle, other.JobTitle, comparisonType)
                && string.Equals(Office, other.Office, comparisonType)
                && string.Equals(Organization, other.Organization, comparisonType)
                && string.Equals(Profession, other.Profession, comparisonType)
                && string.Equals(Role, other.Role, comparisonType);
        }

        #region IEquatable<Position> Members

        public bool Equals(Position other)
        {
            return Equals(other, _DefaultPositionStringComparison);
        }

        #endregion

        public static bool operator ==(Position left, Position right)
        {
            return left.Equals(right, _DefaultPositionStringComparison);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !left.Equals(right, _DefaultPositionStringComparison);
        }
    }

    public class PositionBuilder : INotifyPropertyChanged
    { 
        private string _organization;
        private string _role;
        private string _company;
        private string _department;
        private string _office;
        private string _title;
        private string _profession;

        private void _OnPropertyChanged(string propertyName)
        {
            Assert.IsFalse(string.IsNullOrEmpty(propertyName));

            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public PositionBuilder(string organization, string role, string company, string department, string office, string title, string profession)
        {
            _organization = organization;
            _role = role;
            _company = company;
            _department = department;
            _office = office;
            _title = title;
            _profession = profession;
        }

        public PositionBuilder()
            : this(null, null, null, null, null, null, null)
        { }

        public PositionBuilder(Position position)
            : this(
                position.Organization,
                position.Role,
                position.Company,
                position.Department,
                position.Office,
                position.JobTitle,
                position.Profession)
        { }

        public string Organization
        {
            get { return _organization ?? ""; }
            set
            {
                if (_organization != value)
                {
                    _organization = value;
                    _OnPropertyChanged("Organization");
                }
            }
        }

        public string Role
        {
            get { return _role ?? ""; }
            set
            {
                if (_role != value)
                {
                    _role = value;
                    _OnPropertyChanged("Role");
                }
            }
        }

        public string Company
        {
            get { return _company ?? ""; }
            set
            {
                if (_company != value)
                {
                    _company = value;
                    _OnPropertyChanged("Company");
                }
            }
        }

        public string Department
        {
            get { return _department ?? ""; }
            set
            {
                if (_department != value)
                {
                    _department = value;
                    _OnPropertyChanged("Department");
                }
            }
        }

        public string Office
        {
            get { return _office ?? ""; }
            set
            {
                if (_office != value)
                {
                    _office = value;
                    _OnPropertyChanged("Office");
                }
            }
        }

        public string JobTitle
        {
            get { return _title ?? ""; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    _OnPropertyChanged("JobTitle");
                }
            }
        }

        public string Profession
        {
            get { return _profession ?? ""; }
            set
            {
                if (_profession != value)
                {
                    _profession = value;
                    _OnPropertyChanged("Profession");
                }
            }
        }

        public Position ToPosition()
        {
            return new Position(
                Organization,
                Role,
                Company,
                Department,
                Office,
                JobTitle,
                Profession);
        }

        public static implicit operator Position(PositionBuilder builder)
        {
            if (null == builder)
            {
                return default(Position);
            }
            return builder.ToPosition();
        }

        #region Object Overrides

        public override string ToString()
        {
            return ToPosition().ToString();
        }

        public override int GetHashCode()
        {
            return ToPosition().GetHashCode();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public struct IMAddress : IEquatable<IMAddress>
    {
        #region Fields
        // IM Addresses shouldn't have cultural components to their values.
        // Ordinal comparisons make sense here.
        private const StringComparison _DefaultIMAddressStringComparison = StringComparison.Ordinal;

        private readonly string _handle;
        private readonly string _protocol;

        private int _hashCode;
        private string _toString;
        #endregion

        public IMAddress(string handle, string protocol)
        {
            _handle = handle;
            _protocol = protocol;

            _hashCode = default(int);
            _toString = default(string);
        }

        public string Protocol
        {
            get { return _protocol ?? ""; }
        } 

        public string Handle
        {
            get { return _handle ?? ""; }
        }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            try
            {
                return Equals((IMAddress)obj, _DefaultIMAddressStringComparison);
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            if (default(int) == _hashCode)
            {
                // ToString exposes all the properties, can hijack it for 
                _hashCode = ToString().GetHashCode();
            }
            return _hashCode;
        }

        public override string ToString()
        {
            if (default(string) == _toString)
            {
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(_protocol))
                {
                    sb.Append(_protocol).Append(":");
                }
                sb.Append(_handle ?? "<missing-handle>");
                _toString = sb.ToString();
            }
            return _toString;
        }

        #endregion

        public bool Equals(IMAddress other, StringComparison comparisonType)
        {
            return string.Equals(Handle, other.Handle, comparisonType)
                && string.Equals(Protocol, other.Protocol, comparisonType);
        }
        
        #region IEquatable<IMAddress> Members

        public bool Equals(IMAddress other)
        {
            return Equals(other, _DefaultIMAddressStringComparison);
        }

        #endregion

        public static bool operator ==(IMAddress left, IMAddress right)
        {
            return left.Equals(right, _DefaultIMAddressStringComparison);
        }

        public static bool operator !=(IMAddress left, IMAddress right)
        {
            return !left.Equals(right, _DefaultIMAddressStringComparison);
        }
    }

    public class IMAddressBuilder : INotifyPropertyChanged
    {
        private string _handle;
        private string _protocol;

        private void _OnPropertyChanged(string propertyName)
        {
            Assert.IsFalse(string.IsNullOrEmpty(propertyName));

            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public IMAddressBuilder(string handle, string protocol)
        {
            _handle = handle;
            _protocol = protocol;
        }

        public IMAddressBuilder(IMAddress imAddress)
            : this(imAddress.Handle, imAddress.Protocol)
        {}

        public IMAddressBuilder()
            : this(null, null)
        { }

        public string Protocol
        {
            get { return _protocol ?? ""; }
            set 
            {
                if (_protocol != value)
                {
                    _protocol = value;
                    _OnPropertyChanged("Protocol");
                }
            }
        } 

        public string Handle
        {
            get { return _handle ?? ""; }
            set
            {
                if (_handle != value)
                {
                    _handle = value;
                    _OnPropertyChanged("Handle");
                }
            }
        }

        #region Object Overrides

        public override string ToString()
        {
            return ToIMAddress().ToString();
        }

        public override int GetHashCode()
        {
            return ToIMAddress().GetHashCode();
        }

        #endregion

        public IMAddress ToIMAddress()
        {
            return new IMAddress(Handle, Protocol);
        }

        public static implicit operator IMAddress(IMAddressBuilder builder)
        {
            if (null == builder)
            {
                return default(IMAddress);
            }
            return builder.ToIMAddress();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public struct EmailAddress : IEquatable<EmailAddress>
    {
        // EmailAddresses shouldn't have cultural components in their properties.
        private const StringComparison _DefaultEmailAddressComparison = StringComparison.Ordinal;

        private readonly string _address;
        private readonly string _type;

        private int _hashCode;
        private string _toString;

        public EmailAddress(string address, string type)
        {
            _address = address;
            _type = type;

            _hashCode = default(int);
            _toString = default(string);
        }

        public EmailAddress(string address)
            : this(address, null)
        {}

        public string Address
        {
            get
            {
                return _address ?? "";
            }
        }

        public string AddressType
        {
            get
            {
                return _type ?? "";
            }
        }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            try
            {
                return Equals((EmailAddress)obj, _DefaultEmailAddressComparison);
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            if (default(int) == _hashCode)
            {
                // ToString exposes all the properties, can hijack it for 
                _hashCode = ToString().GetHashCode();
            }
            return _hashCode;
        }

        public override string ToString()
        {
            if (default(string) == _toString)
            {
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(_type))
                {
                    sb.Append(_type).Append(":");
                }
                sb.Append(_address ?? "<missing-address>");
                _toString = sb.ToString();
            }
            return _toString;
        }

        #endregion

        public bool Equals(EmailAddress other, StringComparison comparisonType)
        {
            return string.Equals(Address, other.Address, comparisonType)
                && string.Equals(AddressType, other.AddressType, comparisonType);
        }

        #region IEquatable<EmailAddress> Members

        public bool Equals(EmailAddress other)
        {
            return Equals(other, StringComparison.CurrentCulture);
        }

        #endregion

        public static bool operator ==(EmailAddress left, EmailAddress right)
        {
            return left.Equals(right, StringComparison.CurrentCulture);
        }

        public static bool operator !=(EmailAddress left, EmailAddress right)
        {
            return !left.Equals(right, StringComparison.CurrentCulture);
        }

        public static implicit operator EmailAddress(string address)
        {
            return new EmailAddress(address);
        }
    }

    public class EmailAddressBuilder : INotifyPropertyChanged
    {
        private string _address;
        private string _type;

        private void _OnPropertyChanged(string propertyName)
        {
            Assert.IsFalse(string.IsNullOrEmpty(propertyName));

            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public EmailAddressBuilder(string address, string type)
        {
            _address = address;
            _type = type;
        }

        public EmailAddressBuilder(string address)
            : this(address, null)
        { }

        public EmailAddressBuilder(EmailAddress emailAddress)
            : this(emailAddress.Address, emailAddress.AddressType)
        { }

        public EmailAddressBuilder()
            : this(null, null)
        { }

        public string Address
        {
            get { return _address ?? ""; }
            set
            {
                if (_address != value)
                {
                    _address = value;
                    _OnPropertyChanged("Address");
                }
            }
        }

        public string AddressType
        {
            get { return _type ?? ""; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    _OnPropertyChanged("AddressType");
                }
            }
        }

        #region Object Overrides

        public override string ToString()
        {
            return ToEmailAddress().ToString();
        }

        public override int GetHashCode()
        {
            return ToEmailAddress().GetHashCode();
        }

        #endregion

        public EmailAddress ToEmailAddress()
        {
            return new EmailAddress(Address, AddressType);
        }

        public static implicit operator EmailAddress(EmailAddressBuilder builder)
        {
            if (null == builder)
            {
                return default(EmailAddress);
            }
            return builder.ToEmailAddress();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public enum PhotoResolutionPreference
    {
        // Most common case.
        PreferStream = 0,
        PreferUrl = 1,
    }

    public struct Photo : IEquatable<Photo>
    {
        #region Fields and Auto Props
        private static readonly WebClient _webclient = new WebClient { CachePolicy = HttpWebRequest.DefaultCachePolicy };

        // Strings are mime-types and Urls, so OrdinalIgnoreCase seems appropriate
        private const StringComparison _DefaultPhotoStringComparison = StringComparison.OrdinalIgnoreCase;

        public Uri Url { get; private set; }
        public Stream Value { get; private set; }
        private readonly string _valueType;

        public static bool SupportNonLocalUrls { get; set; }
        public static PhotoResolutionPreference ResolutionPreference { get; set; }
        #endregion

        public Stream ResolveToStream()
        {
            return ResolveToStream(ResolutionPreference, SupportNonLocalUrls);
        }

        public Stream ResolveToStream(PhotoResolutionPreference resolutionPreference, bool supportNonLocalUrls)
        {
            Stream retStm;

            switch (resolutionPreference)
            {
                case PhotoResolutionPreference.PreferStream:
                    if (!_TryGetValueStream(out retStm))
                    {
                        _TryGetUrlStream(supportNonLocalUrls, out retStm);
                    }
                    break;

                case PhotoResolutionPreference.PreferUrl:
                    if (!_TryGetUrlStream(supportNonLocalUrls, out retStm))
                    {
                        _TryGetValueStream(out retStm);
                    }
                    break;

                default:
                    throw new ArgumentException("Invalid enumeration value.", "resolutionPreference");
            }

            return retStm;
        }

        private bool _TryGetValueStream(out Stream stm)
        {
            stm = null;
            if (null != Value)
            {
                // Unconditionally copy the stream.
                // Make a reasonable attempt to preserve immutability contract
                // despite what callers may do with this.
                stm = new MemoryStream();
                Utility.CopyStream(stm, Value);
                return true;
            }
            return false;
        }

        private bool _TryGetUrlStream(bool supportNonLocalUrls, out Stream stm)
        {
            stm = null;

            if (null == Url)
            {
                return false;
            }

            if (supportNonLocalUrls && !Url.IsFile)
            {
                try
                {
                    stm = new MemoryStream(_webclient.DownloadData(Url));
                    return true;
                }
                catch (WebException)
                {
                    return false;
                }
                catch (Exception e)
                {
                    // I don't know what other exceptions WebClient.DownloadData can throw...
                    Assert.Fail(e.ToString());
                    throw;
                }
            }
            
            if (Url.IsFile && File.Exists(Url.LocalPath))
            {
                // Drop this fast.  Don't keep the file open longer than necessary.
                using (var fs = new FileStream(Url.LocalPath, FileMode.Open, FileAccess.Read))
                {
                    stm = new MemoryStream();
                    Utility.CopyStream(stm, fs);
                    return true;
                }
            }

            return false;
        }

        public Photo(Uri url)
            : this(null, null, url)
        {}

        public Photo(Stream stream, string type)
            : this(stream, type, null)
        {}

        public Photo(Stream stream, string streamType, Uri url) : this()
        {
            // ComStream and ReadonlyStream are immutable so I could use those directly.
            // Alternatively, I could just check the CanWrite property, but that really just
            // means that I can't write to it through this reference, not that the underlying
            // stream is really immutable.
            // No matter what, the stream isn't wholly immutable because anyone can dispose it.
            // But the lifetime management is more unclear when it's owned by the struct.
            // In general structs shouldn't have complex object semantics, I believe this
            // is one of the reasons why.
            // (This might also be an argument for these types to not be structs.)
            //
            // In the end I'm trying to err on the side of least surprising surprises.
            // The source stream is what this returns, read-only or not.
            // Clients need to be careful to ensure that the stream stays alive as long as necessary.
            Value = null;
            _valueType = "";
            if (null != stream)
            {
                Value = stream;
                _valueType = streamType;
            }
            else if (!string.IsNullOrEmpty(streamType))
            {
                throw new ArgumentNullException("stream", "A streamType cannot be provided if stream is null");
            }
            // All strings are turned into "", but Uris can't be empty.
            Url = url;
        }

        public string ValueType
        {
            get { return _valueType ?? ""; }
        }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            try
            {
                return Equals((Photo)obj, _DefaultPhotoStringComparison);
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        // This potentially modifies the Photo object because it calculates a hash over the Stream.
        public override int GetHashCode()
        {
            // Can't cache this because the stream can be modified between calls.
            var sb = new StringBuilder();
            sb.Append(ToString());
            if (null != Value)
            {
                sb.Append(Utility.HashStreamMD5(Value));
            }
            return sb.ToString().GetHashCode();
        }

        public override string ToString()
        {
            // Although we could use the Hash, it's unnecessary and surprising that ToString
            // could potentially modify the object.  Keeping references to Streams has other
            // potential problems, not the least of which is that this object is no longer
            // immutable, so can't cache the string result...
            string valueString = "<missing-stream>";
            if (null != Value)
            {
                valueString = Value.Length + " byte image stream";
            }
            var sb = new StringBuilder();
            Utility.GeneratePropertyString(sb, "Value", valueString);
            Utility.GeneratePropertyString(sb, "ValueType", ValueType);
            string urlString = (null == Url) ? "<missing-url>" : Url.ToString();
            Utility.GeneratePropertyString(sb, "Url", urlString);
            return sb.ToString();
        }

        #endregion

        public bool Equals(Photo other, StringComparison comparisonType)
        {
            return 0 == Uri.Compare(Url, other.Url, UriComponents.AbsoluteUri, UriFormat.Unescaped, comparisonType)
                && string.Equals(ValueType, other.ValueType, comparisonType)
                && Utility.AreStreamsEqual(Value, other.Value);
        }

        #region IEquatable<Photo> Members

        public bool Equals(Photo other)
        {
            return Equals(other, _DefaultPhotoStringComparison);
        }

        #endregion

        public static bool operator ==(Photo left, Photo right)
        {
            return left.Equals(right, _DefaultPhotoStringComparison);
        }

        public static bool operator !=(Photo left, Photo right)
        {
            return !left.Equals(right, _DefaultPhotoStringComparison);
        }
    }

    public class PhotoBuilder : INotifyPropertyChanged
    {
        private Stream _value;
        private string _valueType;
        private Uri _url;

        private void _OnPropertyChanged(string propertyName)
        {
            Assert.IsFalse(string.IsNullOrEmpty(propertyName));

            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public PhotoBuilder()
            : this(null, null, null)
        { }

        public PhotoBuilder(Uri url)
            : this(null, null, url)
        { }

        public PhotoBuilder(Stream stream, string type)
            : this(stream, type, null)
        { }

        public PhotoBuilder(Photo photo)
            : this(photo.Value, photo.ValueType, photo.Url)
        { }

        public PhotoBuilder(Stream stream, string streamType, Uri url)
        {
            _value = stream;
            _valueType = streamType;
            _url = url;
        }

        public Uri Url
        {
            get { return _url; }
            set
            {
                // Uri is immutable and supports operator==, so don't need reference equality.
                if (_url != value)
                {
                    _url = value;
                    _OnPropertyChanged("Url");
                }
            }
        }

        public Stream Value
        {
            get { return _value; }
            set 
            {
                if (!ReferenceEquals(_value, value))
                {
                    _value = value;
                    _OnPropertyChanged("Value");
                }
            }
        }

        public string ValueType
        {
            get { return _valueType ?? ""; }
            set
            {
                if (_valueType != value)
                {
                    _valueType = value;
                    _OnPropertyChanged("ValueType");
                }
            }
        }

        public void Clear()
        {
            // Delay notifications until everything is cleared.
            bool updateUrl = false;
            bool updateValueType = false;
            bool updateValue = false;

            if (null != _value)
            {
                _value = null;
                updateValue = true;
            }
            if (!string.IsNullOrEmpty(_valueType))
            {
                _valueType = null;
                updateValueType = true;
            }
            if (null != _url)
            {
                _url = null;
                updateUrl = true;
            }

            if (updateUrl)
            {
                _OnPropertyChanged("Url");
            }
            if (updateValue)
            {
                _OnPropertyChanged("Value");
            }
            if (updateValueType)
            {
                _OnPropertyChanged("ValueType");
            }
        }

        #region Object Overrides

        public override string ToString()
        {
            return ToPhoto().ToString();
        }

        public override int GetHashCode()
        {
            return ToPhoto().GetHashCode();
        }

        #endregion

        public Photo ToPhoto()
        {
            if (null == Value && !string.IsNullOrEmpty(ValueType))
            {
                throw new FormatException("Photos can't have a stream type with no stream value.");
            }

            return new Photo(Value, ValueType, Url);
        }

        public static implicit operator Photo(PhotoBuilder builder)
        {
            if (null == builder)
            {
                return default(Photo);
            }
            return builder.ToPhoto();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public struct PhysicalAddress : IEquatable<PhysicalAddress>
    {
        #region Fields
        private const StringComparison _DefaultPhysicalAddressStringComparison = StringComparison.CurrentCulture;

        private readonly string _label;
        private readonly string _street;
        private readonly string _city;
        private readonly string _state;
        private readonly string _zip;
        private readonly string _country;
        private readonly string _pobox;
        private readonly string _extended;

        private int _hashCode;
        private string _toString;
        #endregion

        public PhysicalAddress(string poBox, string street, string city, string state, string zip, string country, string extended, string label)
        {
            _label = label;
            _street = street;
            _city = city;
            _state = state;
            _zip = zip;
            _country = country;
            _pobox = poBox;
            _extended = extended;

            _hashCode = default(int);
            _toString = default(string);
        }

        public string City
        {
            get { return _city ?? ""; }
        } 

        public string State
        {
            get { return _state ?? ""; }
        } 

        public string ZipCode
        {
            get { return _zip ?? ""; }
        } 

        public string Country
        {
            get { return _country ?? ""; }
        } 

        public string POBox
        {
            get { return _pobox ?? ""; }
        } 

        public string ExtendedAddress
        {
            get { return _extended ?? ""; }
        } 

        public string Street
        {
            get { return _street ?? ""; }
        } 

        public string AddressLabel
        {
            get { return _label ?? ""; }
        }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            try
            {
                return Equals((PhysicalAddress)obj, _DefaultPhysicalAddressStringComparison);
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            if (default(int) == _hashCode)
            {
                _hashCode = ToString().GetHashCode();
            }
            return _hashCode;
        }

        public override string ToString()
        {
            if (default(string) == _toString)
            {
                var sb = new StringBuilder();
                Utility.GeneratePropertyString(sb, "AddressLabel", AddressLabel);
                Utility.GeneratePropertyString(sb, "City", City);
                Utility.GeneratePropertyString(sb, "Country", Country);
                Utility.GeneratePropertyString(sb, "ExtendedAddress", ExtendedAddress);
                Utility.GeneratePropertyString(sb, "POBox", POBox);
                Utility.GeneratePropertyString(sb, "State", State);
                Utility.GeneratePropertyString(sb, "Street", Street);
                Utility.GeneratePropertyString(sb, "ZipCode", ZipCode);
                _toString = sb.ToString();
            }
            return _toString;
        }

        #endregion

        public bool Equals(PhysicalAddress other, StringComparison comparisonType)
        {
            return string.Equals(AddressLabel, other.AddressLabel, comparisonType)
                && string.Equals(City, other.City, comparisonType)
                && string.Equals(Country, other.Country, comparisonType)
                && string.Equals(ExtendedAddress, other.ExtendedAddress, comparisonType)
                && string.Equals(POBox, other.POBox, comparisonType)
                && string.Equals(State, other.State, comparisonType)
                && string.Equals(Street, other.Street, comparisonType)
                && string.Equals(ZipCode, other.ZipCode, comparisonType);
        }

        #region IEquatable<PhysicalAddress> Members

        public bool Equals(PhysicalAddress other)
        {
            return Equals(other, _DefaultPhysicalAddressStringComparison);
        }

        #endregion

        public static bool operator ==(PhysicalAddress left, PhysicalAddress right)
        {
            return left.Equals(right, _DefaultPhysicalAddressStringComparison);
        }

        public static bool operator !=(PhysicalAddress left, PhysicalAddress right)
        {
            return !left.Equals(right, _DefaultPhysicalAddressStringComparison);
        }

    }

    public class PhysicalAddressBuilder : INotifyPropertyChanged
    {
        private string _label;
        private string _street;
        private string _city;
        private string _state;
        private string _zip;
        private string _country;
        private string _pobox;
        private string _extended;

        private void _OnPropertyChanged(string propertyName)
        {
            Assert.IsFalse(string.IsNullOrEmpty(propertyName));

            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public PhysicalAddressBuilder(string poBox, string street, string city, string state, string zip, string country, string extended, string label)
        {
            _pobox = poBox;
            _street = street;
            _city = city;
            _state = state;
            _zip = zip;
            _country = country;
            _extended = extended;
            _label = label;
        }

        public PhysicalAddressBuilder(PhysicalAddress address)
            : this(
                address.POBox,
                address.Street,
                address.City,
                address.State,
                address.ZipCode,
                address.Country,
                address.ExtendedAddress,
                address.AddressLabel)
        { }

        public PhysicalAddressBuilder()
            : this(null, null, null, null, null, null, null, null)
        { }

        public string City
        {
            get { return _city ?? ""; }
            set
            {
                if (_city != value)
                {
                    _city = value;
                    _OnPropertyChanged("City");
                }
            }
        }

        public string State
        {
            get { return _state ?? ""; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    _OnPropertyChanged("State");
                }
            }
        }

        public string ZipCode
        {
            get { return _zip ?? ""; }
            set
            {
                if (_zip != value)
                {
                    _zip = value;
                    _OnPropertyChanged("ZipCode");
                }
            }
        }

        public string Country
        {
            get { return _country ?? ""; }
            set
            {
                if (_country != value)
                {
                    _country = value;
                    _OnPropertyChanged("Country");
                }
            }
        }

        public string POBox
        {
            get { return _pobox ?? ""; }
            set
            {
                if (_pobox != value)
                {
                    _pobox = value;
                    _OnPropertyChanged("POBox");
                }
            }
        }

        public string ExtendedAddress
        {
            get { return _extended ?? ""; }
            set
            {
                if (_extended != value)
                {
                    _extended = value;
                    _OnPropertyChanged("ExtendedAddress");
                }
            }
        }

        public string Street
        {
            get { return _street ?? ""; }
            set
            {
                if (_street != value)
                {
                    _street = value;
                    _OnPropertyChanged("Street");
                }
            }
        }

        public string AddressLabel
        {
            get { return _label ?? ""; }
            set
            {
                if (_label != value)
                {
                    _label = value;
                    _OnPropertyChanged("AddressLabel");
                }
            }
        }

        #region Object Overrides

        public override string ToString()
        {
            return ToPhysicalAddress().ToString();
        }

        public override int GetHashCode()
        {
            return ToPhysicalAddress().GetHashCode();
        }

        #endregion

        public PhysicalAddress ToPhysicalAddress()
        {
            return new PhysicalAddress(
                POBox,
                Street,
                City,
                State,
                ZipCode,
                Country,
                ExtendedAddress,
                AddressLabel);
        }

        public static implicit operator PhysicalAddress(PhysicalAddressBuilder builder)
        {
            if (null == builder)
            {
                return default(PhysicalAddress);
            }
            return builder.ToPhysicalAddress();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public struct PhoneNumber : IEquatable<PhoneNumber>
    {
        #region Fields
        // Unfortunately don't have a good way to compare phone numbers, e.g. 222 == AbC? 555-1212 == 5551212?
        // Falling back on Ordinal comparison by default.
        private const StringComparison _DefaultPhoneNumberStringComparison = StringComparison.Ordinal;

        private readonly string _number;
        private readonly string _alternate;

        private int _hashCode;
        private string _toString;
        #endregion;

        public PhoneNumber(string number)
            : this(number, null)
        {}

        public PhoneNumber(string number, string alternate)
        {
            _number = number;
            _alternate = alternate;

            _hashCode = default(int);
            _toString = default(string);
        }

        public string Number
        {
            get { return _number ?? ""; }
        } 

        public string Alternate
        {
            get { return _alternate ?? ""; }
        }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            try
            {
                return Equals((PhoneNumber)obj, _DefaultPhoneNumberStringComparison);
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            if (default(int) == _hashCode)
            {
                _hashCode = ToString().GetHashCode();
            }
            return _hashCode;
        }

        public override string ToString()
        {
            if (default(string) == _toString)
            {
                var sb = new StringBuilder();
                Utility.GeneratePropertyString(sb, "Number", Number);
                Utility.GeneratePropertyString(sb, "Alternate", Alternate);
                _toString = sb.ToString();
            }
            return _toString;
        }

        #endregion

        public bool Equals(PhoneNumber other, StringComparison comparisonType)
        {
            return string.Equals(Alternate, other.Alternate, comparisonType)
                && string.Equals(Number, other.Number, comparisonType);
        }

        #region IEquatable<PhoneNumber> Members

        public bool Equals(PhoneNumber other)
        {
            return Equals(other, _DefaultPhoneNumberStringComparison);
        }

        #endregion

        public static bool operator ==(PhoneNumber left, PhoneNumber right)
        {
            return left.Equals(right, _DefaultPhoneNumberStringComparison);
        }

        public static bool operator !=(PhoneNumber left, PhoneNumber right)
        {
            return !left.Equals(right, _DefaultPhoneNumberStringComparison);
        }

        public static implicit operator PhoneNumber(string number)
        {
            return new PhoneNumber(number);
        }
    }

    public class PhoneNumberBuilder : INotifyPropertyChanged
    {
        private string _number;
        private string _alternate;

        private void _OnPropertyChanged(string propertyName)
        {
            Assert.IsFalse(string.IsNullOrEmpty(propertyName));

            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public PhoneNumberBuilder(string number)
            : this(number, null)
        { }

        public PhoneNumberBuilder(string number, string alternate)
        {
            _number = number;
            _alternate = alternate;
        }

        public PhoneNumberBuilder(PhoneNumber phoneNumber)
            : this(phoneNumber.Number, phoneNumber.Alternate)
        { }

        public PhoneNumberBuilder()
            : this(null, null)
        { }
        
        public string Number
        {
            get { return _number ?? ""; }
            set
            {
                if (_number != value)
                {
                    _number = value;
                    _OnPropertyChanged("Number");
                }
            }
        }

        public string Alternate
        {
            get { return _alternate ?? ""; }
            set
            {
                if (_alternate != value)
                {
                    _alternate = value;
                    _OnPropertyChanged("Alternate");
                }
            }
        }

        public PhoneNumber ToPhoneNumber()
        {
            return new PhoneNumber(Number, Alternate);
        }

        public static implicit operator PhoneNumber(PhoneNumberBuilder builder)
        {
            if (null == builder)
            {
                return default(PhoneNumber);
            }
            return builder.ToPhoneNumber();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public struct Certificate : IEquatable<Certificate>
    {
        #region Fields
        // Strings are mime-types, so OrdinalIgnoreCase seems appropriate
        private const StringComparison _DefaultCertificateStringComparison = StringComparison.OrdinalIgnoreCase;

        private readonly Stream _value;
        private readonly string _valueType;
        private readonly Stream _thumbprint;
        private readonly string _thumbprintType;
        #endregion;

        public Certificate(Stream certificateValue, string certificateType, Stream thumbprint, string thumbprintType)
        {
            // ComStream and ReadonlyStream are immutable so I could use those directly.
            // Alternatively, I could just check the CanWrite property, but that really just
            // means that I can't write to it through this reference, not that the underlying
            // stream is really immutable.
            // No matter what, the stream isn't wholly immutable because anyone can dispose it.
            // But the lifetime management is more unclear when it's owned by the struct.
            // In general structs shouldn't have complex object semantics, I believe this
            // is one of the reasons why.
            //
            // In the end I'm trying to err on the side of least surprising surprises.
            // The source stream is what this returns, read-only or not.
            // Clients need to be careful to ensure that the stream stays alive as long as necessary.
            _value = null;
            _valueType = "";
            if (null != certificateValue)
            {
                _value = certificateValue;
                _valueType = certificateType;
            }
            else if (null != certificateType)
            {
                throw new ArgumentNullException("certificateValue", "A certificateType cannot be provided if certificateValue is null");
            }
            _thumbprint = null;
            _thumbprintType = "";
            if (null != thumbprint)
            {
                _thumbprint = thumbprint;
                _thumbprintType = thumbprintType;
            }
            else if (null != thumbprintType)
            {
                throw new ArgumentNullException("thumbprint", "A thumbprintType cannot be provided if thumbprint is null");
            }
        }

        public Stream Value
        {
            get { return _value; }
        }

        public string ValueType
        {
            get { return _valueType ?? ""; }
        }

        public Stream Thumbprint
        {
            get { return _thumbprint; }
        }

        public string ThumbprintType
        {
            get { return _thumbprintType ?? ""; }
        }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            try
            {
                return Equals((Certificate)obj, _DefaultCertificateStringComparison);
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        // This potentially modifies the Photo object because it calculates a hash over the Stream.
        public override int GetHashCode()
        {
            // Can't cache this because the stream can be modified between calls.
            var sb = new StringBuilder();
            sb.Append(ToString());
            if (null != _value)
            {
                sb.Append(Utility.HashStreamMD5(_value));
            }
            if (null != _thumbprint)
            {
                sb.Append(Utility.HashStreamMD5(_thumbprint));
            }
            return sb.ToString().GetHashCode();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            string valueString = "<missing-stream>";
            if (null != _value)
            {
                // Although we could use the Hash, it's unnecessary and surprising that ToString
                // could potentially modify the object.  Keeping references to Streams has other
                // potential problems, not the least of which is that this object is no longer
                // immutable, so can't cache the string result...
                valueString = _value.Length + " byte value stream";
            }
            Utility.GeneratePropertyString(sb, "Value", valueString);
            Utility.GeneratePropertyString(sb, "ValueType", ValueType);
            valueString = "<missing-stream>";
            if (null != _thumbprintType)
            {
                valueString = _thumbprint.Length + " byte thumbprint stream";
            }
            Utility.GeneratePropertyString(sb, "Thumbprint", valueString);
            Utility.GeneratePropertyString(sb, "ThumbprintType", ThumbprintType);

            return sb.ToString();

        }

        #endregion

        public bool Equals(Certificate other, StringComparison comparisonType)
        {
            return string.Equals(ThumbprintType, other.ThumbprintType, comparisonType)
                && string.Equals(ValueType, other.ValueType, comparisonType)
                && Utility.AreStreamsEqual(Thumbprint, other.Thumbprint)
                && Utility.AreStreamsEqual(Value, other.Value);
        }

        #region IEquatable<Certificate> Members

        public bool Equals(Certificate other)
        {
            return Equals(other, _DefaultCertificateStringComparison);
        }

        #endregion

        public static bool operator ==(Certificate left, Certificate right)
        {
            return left.Equals(right, _DefaultCertificateStringComparison);
        }

        public static bool operator !=(Certificate left, Certificate right)
        {
            return !left.Equals(right, _DefaultCertificateStringComparison);
        }

    }

    public class CertificateBuilder : INotifyPropertyChanged
    {
        private Stream _value;
        private string _valueType;
        private Stream _thumbprint;
        private string _thumbprintType;

        private void _OnPropertyChanged(string propertyName)
        {
            Assert.IsFalse(string.IsNullOrEmpty(propertyName));

            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public CertificateBuilder(Stream certificateValue, string certificateType, Stream thumbprint, string thumbprintType)
        {
            _value = certificateValue;
            _valueType = certificateType;
            _thumbprint = thumbprint;
            _thumbprintType = thumbprintType;
        }

        public CertificateBuilder(Certificate certificate)
            : this(certificate.Value, certificate.ValueType, certificate.Thumbprint, certificate.ThumbprintType)
        { }

        public CertificateBuilder()
            : this(null, null, null, null)
        { }

        public Stream Value
        {
            get { return _value; }
            set
            {
                if (!ReferenceEquals(_value, value))
                {
                    _value = value;
                    _OnPropertyChanged("Value");
                }
            }
        }

        public string ValueType
        {
            get { return _valueType ?? ""; }
            set
            {
                if (_valueType != value)
                {
                    _valueType = value;
                    _OnPropertyChanged("ValueType");
                }
            }
        }

        public Stream Thumbprint
        {
            get { return _thumbprint; }
            set
            {
                if (!ReferenceEquals(_thumbprint, value))
                {
                    _thumbprint = value;
                    _OnPropertyChanged("Thumbprint");
                }
            }
        }

        public string ThumbprintType
        {
            get { return _thumbprintType ?? ""; }
            set
            {
                if (_thumbprintType != value)
                {
                    _thumbprintType = value;
                    _OnPropertyChanged("ThumbprintType");
                }
            }
        }

        #region Object Overrides

        public override string ToString()
        {
            return ToCertificate().ToString();
        }

        public override int GetHashCode()
        {
            return ToCertificate().GetHashCode();
        }

        #endregion

        public Certificate ToCertificate()
        {
            if (null == Value && !string.IsNullOrEmpty(ValueType))
            {
                throw new FormatException("A certificateType cannot be provided if certificateValue is null");
            }
            if (null == Thumbprint && !string.IsNullOrEmpty(ThumbprintType))
            {
                throw new FormatException("A certificateType cannot be provided if certificateValue is null");
            }

            return new Certificate(Value, ValueType, Thumbprint, ThumbprintType);
        }

        public static implicit operator Certificate(CertificateBuilder builder)
        {
            if (null == builder)
            {
                return default(Certificate);
            }
            return builder.ToCertificate();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public enum Gender
    {
        Unspecified,
        Male,
        Female
    }
}
