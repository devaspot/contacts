/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A collection of strings that can be composited to form common property names.
    /// </summary>
    public static class PropertyNames
    {
        private const string BracedFormatString = "[{0}]";

        /// <summary>Value is a level3 property name for several node types.</summary>
        public const string Value = "/Value";

        /// <summary>Sex of the contact.  This is a top level property.</summary>
        public const string Gender = "Gender";

        /// <summary>Creation time of the contact.</summary>
        /// <remarks>This is a top level readonly property.</remarks>
        public const string CreationDate = "CreationDate";

        /// <summary>Arbitrary notes about the contact.</summary>
        /// <remarks>This is a top level property.</remarks>
        public const string Notes = "Notes";

        /// <summary>The mail program that the contact uses.</summary>
        /// <remarks>
        /// This is a top level property.<para/>
        /// This information may provide assistance to a correspondent regarding the type of data representation which can
        /// be used, and how they may be packaged.  This property parameter is based on currently accepted practices within
        /// the Internet MIME community with the "X-Mailer" header field.
        /// </remarks>
        public const string Mailer = "Mailer";

        /// <summary>The root collection name for the embedded ContactID Guids.</summary>
        /// <remarks>This is a Level 1 hierarchical property.</remarks>
        public const string ContactIdCollection = "ContactIDCollection";

        /// <summary>The node name for embedded ContactID Guids.</summary>
        /// <remarks>This is a Level 2 hierarchical property.</remarks>
        public const string ContactIdArrayNode = "/ContactID";
        /// <summary>"ContactID"</summary>
        internal const string ContactIdArrayNodeRaw = "ContactID";

        /// <summary>A format string for retrieving the value name for an embedded ContactID node.</summary>
        /// <remarks>This is a format string for a level 3 hierarchical property.</remarks>
        public const string ContactIdValueFormat = ContactIdCollection + ContactIdArrayNode + BracedFormatString + Value;

        /// <summary>The root collection name for the collection of Names on the contact.</summary>
        /// <remarks>This is a Level 1 hierarchical property.</remarks>
        public const string NameCollection = "NameCollection";

        /// <summary>The node name for contact Names.</summary>
        /// <remarks>This is a Level 2 hierarchical property.</remarks>
        public const string NameArrayNode = "/Name";
        /// <summary>"Name"</summary>
        internal const string NameArrayNodeRaw = "Name";

        public const string FormattedName = "/FormattedName";
        public const string NameFormattedNameFormat = NameCollection + NameArrayNode + BracedFormatString + FormattedName;

        public const string Phonetic = "/Phonetic";
        public const string NamePhoneticFormat = NameCollection + NameArrayNode + BracedFormatString + Phonetic;

        public const string Prefix = "/Prefix";
        public const string NamePrefixFormat = NameCollection + NameArrayNode + BracedFormatString + Prefix;
        
        public const string Title = "/Title";
        public const string NameTitleFormat = NameCollection + NameArrayNode + BracedFormatString + Title;
        
        public const string GivenName = "/GivenName";
        public const string NameGivenNameFormat = NameCollection + NameArrayNode + BracedFormatString + GivenName;

        public const string FamilyName = "/FamilyName";
        public const string NameFamilyNameFormat = NameCollection + NameArrayNode + BracedFormatString + FamilyName;

        public const string MiddleName = "/MiddleName";
        public const string NameMiddleNameFormat = NameCollection + NameArrayNode + BracedFormatString + MiddleName;
        
        public const string Generation = "/Generation";
        public const string NameGenerationFormat = NameCollection + NameArrayNode + BracedFormatString + Generation;

        public const string Suffix = "/Suffix";
        public const string NameSuffixFormat = NameCollection + NameArrayNode + BracedFormatString + Suffix;
        
        public const string Nickname = "/NickName";
        public const string NameNicknameFormat = NameCollection + NameArrayNode + BracedFormatString + Nickname;

        public const string PositionCollection = "PositionCollection";

        public const string PositionArrayNode = "/Position";
        /// <summary>"Position"</summary>
        internal const string PositionArrayNodeRaw = "Position";

        public const string Organization = "/Organization";
        public const string PositionOrganizationFormat = PositionCollection + PositionArrayNode + BracedFormatString + Organization;

        public const string Company = "/Company";
        public const string PositionCompanyFormat = PositionCollection + PositionArrayNode + BracedFormatString + Company;

        public const string Department = "/Department";
        public const string PositionDepartmentFormat = PositionCollection + PositionArrayNode + BracedFormatString + Department;

        public const string Office = "/Office";
        public const string PositionOfficeFormat = PositionCollection + PositionArrayNode + BracedFormatString + Office;

        public const string JobTitle = "/JobTitle";
        public const string PositionJobTitleFormat = PositionCollection + PositionArrayNode + BracedFormatString + JobTitle;

        public const string Profession = "/Profession";
        public const string PositionProfessionFormat = PositionCollection + PositionArrayNode + BracedFormatString + Profession;

        public const string Role = "/Role";
        public const string PositionRoleFormat = PositionCollection + PositionArrayNode + BracedFormatString + Role;

        public const string PersonCollection = "PersonCollection";

        public const string PersonArrayNode = "/Person";
        internal const string PersonArrayNodeRaw = "Person";

        public const string PersonFormattedNameFormat = PersonCollection + PersonArrayNode + BracedFormatString + FormattedName;

        public const string PersonId = "/PersonID";

        public const string PersonPersonIdFormat = PersonCollection + PersonArrayNode + BracedFormatString + PersonId;

        /// <summary>Calendar dates associated with the contact.</summary>
        public const string DateCollection = "DateCollection";
        public const string DateArrayNode = "/Date";
        internal const string DateArrayNodeRaw = "Date";
        public const string DateValueFormat = DateCollection + DateArrayNode + BracedFormatString + Value;

        public const string EmailAddressCollection = "EmailAddressCollection";
        public const string EmailAddressArrayNode = "/EmailAddress";
        internal const string EmailAddressArrayNodeRaw = "EmailAddress";
        public const string Address = "/Address";
        public const string EmailAddressAddressFormat = EmailAddressCollection + EmailAddressArrayNode + BracedFormatString + Address;
        public const string AddressType = "/Type";
        public const string EmailAddressAddressTypeFormat = EmailAddressCollection + EmailAddressArrayNode + BracedFormatString + AddressType;

        public const string CertificateCollection = "CertificateCollection";
        public const string CertificateArrayNode = "/Certificate";
        internal const string CertificateArrayNodeRaw = "Certificate";
        public const string CertificateValueFormat = CertificateCollection + CertificateArrayNode + BracedFormatString + Value;
        public const string Thumbprint = "/ThumbPrint";
        public const string CertificateThumbprintFormat = CertificateCollection + CertificateArrayNode + BracedFormatString + Thumbprint;

        public const string PhoneNumberCollection = "PhoneNumberCollection";
        public const string PhoneNumberArrayNode = "/PhoneNumber";
        internal const string PhoneNumberArrayNodeRaw = "PhoneNumber";
        public const string Number = "/Number";
        public const string PhoneNumberNumberFormat = PhoneNumberCollection + PhoneNumberArrayNode + BracedFormatString + Number;
        /// <summary>Alternate number (TTY).</summary>
        public const string Alternate = "/Alternate";
        public const string PhoneNumberAlternateFormat = PhoneNumberCollection + PhoneNumberArrayNode + BracedFormatString + Alternate;

        public const string PhysicalAddressCollection = "PhysicalAddressCollection";
        public const string PhysicalAddressArrayNode = "/PhysicalAddress";
        internal const string PhysicalAddressArrayNodeRaw = "PhysicalAddress";
        public const string AddressLabel = "/AddressLabel";
        public const string PhysicalAddressAddressLabelFormat = PhysicalAddressCollection + PhysicalAddressArrayNode + BracedFormatString + AddressLabel;
        public const string Street = "/Street";
        public const string PhysicalAddressStreetFormat = PhysicalAddressCollection + PhysicalAddressArrayNode + BracedFormatString + Street;
        public const string Locality = "/Locality";
        public const string PhysicalAddressLocalityFormat = PhysicalAddressCollection + PhysicalAddressArrayNode + BracedFormatString + Locality;
        public const string Region = "/Region";
        public const string PhysicalAddressRegionFormat = PhysicalAddressCollection + PhysicalAddressArrayNode + BracedFormatString + Region;
        public const string PostalCode = "/PostalCode";
        public const string PhysicalAddressPostalCodeFormat = PhysicalAddressCollection + PhysicalAddressArrayNode + BracedFormatString + PostalCode;
        public const string Country = "/Country";
        public const string PhysicalAddressCountryFormat = PhysicalAddressCollection + PhysicalAddressArrayNode + BracedFormatString + Country;
        public const string POBox = "/POBox";
        public const string PhysicalAddressPOBoxFormat = PhysicalAddressCollection + PhysicalAddressArrayNode + BracedFormatString + POBox;
        public const string ExtendedAddress = "/ExtendedAddress";
        public const string PhysicalAddressExtendedAddressFormat = PhysicalAddressCollection + PhysicalAddressArrayNode + BracedFormatString + ExtendedAddress;

        public const string IMAddressCollection = "IMAddressCollection";
        public const string IMAddressArrayNode = "/IMAddress";
        internal const string IMAddressArrayNodeRaw = "IMAddress";
        public const string IMAddressValueFormat = IMAddressCollection + IMAddressArrayNode + BracedFormatString + Value;
        public const string Protocol = "/Protocol";
        public const string IMAddressProtocolFormat = IMAddressCollection + IMAddressArrayNode + BracedFormatString + Protocol;

        public const string UrlCollection = "UrlCollection";
        public const string UrlArrayNode = "/Url";
        internal const string UrlArrayNodeRaw = "Url";
        public const string UrlValueFormat = UrlCollection + UrlArrayNode + BracedFormatString + Value;

        public const string PhotoCollection = "PhotoCollection";
        public const string PhotoArrayNode = "/Photo";
        internal const string PhotoArrayNodeRaw = "Photo";
        public const string PhotoValueFormat = PhotoCollection + PhotoArrayNode + BracedFormatString + Value;
        public const string Url = "/Url";
        public const string PhotoUrlFormat = PhotoCollection + PhotoArrayNode + BracedFormatString + Url;
    }

    /// <summary>Labels that are commonly applied to a variety of properties in a contact.</summary>
    public static class PropertyLabels
    {
        public const string Preferred = "Preferred";
        public const string Business = "Business";
        public const string Personal = "Personal";
 
        // I really don't want to promote an "Other" label.  There's no reason to.
        //public const string Other = "Other";
    }

    /// <summary>Labels that are commonly applied to contact address properties.</summary>
    public static class AddressLabels
    {
        public const string Domestic = "Domestic";
        public const string International = "International";
        public const string Postal = "Postal";
        public const string Parcel = "Parcel";
    }

    /// <summary>Labels that are commonly applied to contact phone number properties.</summary>
    public static class PhoneLabels
    {
        public const string Voice = "Voice";
        public const string Mobile = "Mobile";
        [SuppressMessage(
            "Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Pcs",
            Justification="TLA")]
        public const string Pcs = "PCS";
        public const string Cellular = "Cellular";
        public const string Car = "Car";
        public const string Pager = "Pager";
        public const string Tty = "TTY";
        public const string Fax = "Fax";
        public const string Video = "Video";
        public const string Modem = "Modem";
        public const string Bbs = "BBS";
        public const string Isdn = "ISDN";
    }

    /// <summary>
    /// Labels that are commonly applied to a contact to declare an relationship with another contact or person.
    /// </summary>
    public static class PersonLabels
    {
        /// <summary>
        /// Apply this label to a person in conjunction with other labels to indicate that the relationship
        /// described refers to a former relationship.
        /// </summary>
        /// <remarks>
        /// E.g. "Ex" + "Spouse" implies a divorce.  This label doesn't necessarily make sense for relationships
        /// that can't be severed.  Its used should be examined on a case by case basis.
        /// </remarks>
        [SuppressMessage(
            "Microsoft.Naming",
            "CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
            Justification="This is a prefix, not a suffix")]
        public const string Ex = "wab:Ex";

        /// <summary>
        /// Apply this label to a Person to indicate that the person can act as an agent for the contact.
        /// </summary>
        public const string Agent = "Agent";
        public const string Spouse = "wab:Spouse";
        public const string Child = "wab:Child";
        public const string Manager = "wab:Manager";
        public const string DirectReport = "wab:DirectReport";
        public const string Assistant = "wab:Assistant";
        public const string Friend = "wab:Friend";
        
        /// <summary>
        /// Intended for Group contact types.  Apply this label to a Person to indicate that it's a member of the group.
        /// </summary>
        public const string Member = "wab:Member";

        /// <summary>
        /// Apply this label to a Person to indicate that the person is a coworker of the contact.
        /// </summary>
        public const string Coworker = "wab:Coworker";

        /// <summary>
        /// Apply this label to a Person to indicate that the person is a sibling of the contact.
        /// </summary>
        /// <remarks>
        /// Some PersonLabel relationships have an implied reciprocality, e.g. Sibling or Parent/Child.  If
        /// the person being labeled is also a contact then the complimentary label should generally also
        /// be applied to the other contact.
        /// </remarks>
        public const string Sibling = "wab:Sibling";

        /// <summary>
        /// Apply this label to a Person to indicate that the person is the parent of the contact.
        /// </summary>
        /// <remarks>
        /// Some PersonLabel relationships have an implied reciprocality, e.g. Sibling or Parent/Child.  If
        /// the person being labeled is also a contact then the complimentary label should generally also
        /// be applied to the other contact.
        /// </remarks>
        public const string Parent = "wab:Parent";

        public const string Grandparent = "wab:Grandparent";
        public const string Grandchild = "wab:Grandchild";
        public const string Godparent = "wab:Godparent";
        public const string Godchild = "wab:Godchild";
        public const string Mentor = "wab:Mentor";
    }

    /// <summary>Labels that are commonly applied to contact date properties.</summary>
    public static class DateLabels
    {
        /// <summary>Apply this label to a date to indicate the contact's birthday.</summary>
        public const string Birthday = "wab:Birthday";

        /// <summary>Apply this label to a date to indicate the contact's wedding anniversary.</summary>
        public const string Anniversary = "wab:Anniversary";
    }

    /// <summary>Labels that are commonly applied to contact website properties.</summary>
    public static class UrlLabels
    {
        /// <summary>
        /// This label on a URL implies that it corresponds to a social networking
        /// site's homepage for a contact.
        /// </summary>
        /// <remarks>
        /// Additional labels should be used to provide more
        /// specific information if appropriate.
        /// </remarks>
        public const string SocialNetwork = "wab:SocialNetwork";

        /// <summary>
        /// This label on a URL implies that that it is an school's website.
        /// </summary>
        public const string School = "wab:School";

        /// <summary>
        /// This label on a URL implies that it resolves to a wishlist on a commercial site.
        /// </summary>
        public const string WishList = "wab:WishList";

        /// <summary>
        /// This label on a URL implies that it resolves to an RSS (Really Simple Syndication) feed.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Rss",
            Justification="TLA")]
        public const string Rss = "wab:Rss";
    }

    /// <summary>Labels that are commonly applied to contact photo properties.</summary>
    public static class PhotoLabels
    {
        /// <summary>
        /// Use this label on a Photo to make it appear as the Contact's user tile in applications.
        /// </summary>
        public const string UserTile = "UserTile";

        /// <summary>
        /// This label on a Photo implies that it's a logo associated with the contact, such as a business banner.
        /// </summary>
        public const string Logo = "Logo";
    }
}
