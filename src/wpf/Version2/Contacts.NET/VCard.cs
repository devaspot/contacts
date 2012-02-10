/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using Standard;

    internal static class VCard21
    {
        /// <summary>"BASE64"</summary>
        private const string _EncodingBase64 = "BASE64";
        /// <summary>"QUOTED-PRINTABLE"</summary>
        private const string _EncodingQuotedPrintable = "QUOTED-PRINTABLE";
        
        /// <summary>"CHARSET="</summary>
        // private const string _DeclareCharset = "CHARSET=";

        /// <summary>"TYPE="</summary>
        private const string _DeclareType = "TYPE=";
        /// <summary>"ENCODING="</summary>
        private const string _DeclareEncoding = "ENCODING=";
        /// <summary>"VALUE="</summary>
        private const string _DeclareValue = "VALUE=";
        /// <summary>"BEGIN:VCARD"</summary>
        private const string _BeginVCardProperty = "BEGIN:VCARD";
        /// <summary>"END:VCARD"</summary>
        private const string _EndVCardProperty = "END:VCARD";
        /// <summary>"URL"</summary>
        private const string _UrlType = "URL";

        private delegate void _ReadVCardProperty(_Property prop, Contact contact);

        private class _Property
        {
            public string Name;
            public readonly List<string> Types;
            public string ValueString;
            public byte[] ValueBinary;

            public _Property()
            {
                Types = new List<string>();
            }

            // Append extra labels into the returned array.
            // Used for Photos where the labels are implied by the vcard type.
            public string[] GetLabels(string append1, string append2)
            {
                var labelList = new List<string>();

                // Add the optional added strings.
                if (!string.IsNullOrEmpty(append1))
                {
                    labelList.Add(append1);
                }

                if (!string.IsNullOrEmpty(append2))
                {
                    labelList.Add(append2);
                }

                foreach (string label in Types)
                {
                    int index = _labelMap.IndexOfValue(label);
                    if (-1 != index)
                    {
                        labelList.Add(_labelMap.Keys[index]);
                    }
                }
                if (labelList.Count == 0)
                {
                    return null;
                }
                return labelList.ToArray();
            }

            public string[] GetLabels()
            {
                return GetLabels(null, null);
            }
        }

        #region Static Data

        // Mapping of Contact labels to VCard equivalent types.
        private static readonly SortedList<string, string> _labelMap = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {PropertyLabels.Business, "WORK"},
            {PropertyLabels.Personal, "HOME"},
            {PropertyLabels.Preferred, "PREF"},
            {AddressLabels.Domestic, "DOM"},
            {AddressLabels.International, "INTL"},
            {AddressLabels.Parcel, "PARCEL"},
            {AddressLabels.Postal, "POSTAL"},

            // No equivalent label for "MSG"
            {PhoneLabels.Bbs, "BBS"},
            {PhoneLabels.Car, "CAR"},
            {PhoneLabels.Cellular, "CELL"},
            {PhoneLabels.Fax, "FAX"},
            {PhoneLabels.Voice, "VOICE"},
            {PhoneLabels.Video, "VIDEO"},
            {PhoneLabels.Isdn, "ISDN"},
            {PhoneLabels.Mobile, "CELL"},
            {PhoneLabels.Modem, "MODEM"},
            {PhoneLabels.Pager, "PAGER"},

            // Labels that I want to include, but aren't strictly part of the VCARD format.
            // Readers shouldn't choke on these, but other than ours they probably won't
            // correctly pick up the values.
            {UrlLabels.Rss, "RSS"}
        };

        private static readonly SortedList<string, _ReadVCardProperty> _writeVCardPropertiesMap = new SortedList<string, _ReadVCardProperty>(StringComparer.OrdinalIgnoreCase)
        {
            {"ADR", _ReadAddresses},
            {"BDAY", _ReadBirthday},
            {"COMMENT", _ReadNotes},
            {"EMAIL", _ReadEmailAddress},
            {"FN", _ReadFormattedName},
            {"LABEL", _ReadLabel},
            {"LOGO", _ReadLogo},
            {"MAILER", _ReadMailer},
            {"N", _ReadName},
            {"PHOTO", _ReadPhoto},
            {"ORG", _ReadOrganization},
            {"ROLE", _ReadRole},
            {"SOUND", _ReadPhonetic},
            {"TEL", _ReadPhoneNumbers},
            {"TITLE", _ReadTitle},
            {"UID", _ReadUniqueIdentifier},
            {"URL", _ReadUrl}
        };

        private const string Crlf = "\r\n";

        #endregion

        #region Encoding/Decoding Functions

        private static string[] _TokenizeEscapedMultipropString(string multiprop)
        {
            Assert.IsNotNull(multiprop);

            var list = new List<string>();

            string[] split = multiprop.Split(';');
            Assert.BoundedInteger(1, split.Length, int.MaxValue);

            list.Add(split[0]);
            for (int i = 1; i < split.Length; ++i)
            {
                // Check for escaped ';'s
                if (split[i - 1].EndsWith("\\", StringComparison.Ordinal))
                {
                    list[list.Count-1] += ";" + split[i];
                }
                else
                {
                    list.Add(split[i]);
                }
            }

            return list.ToArray();
        }

        // In the presence of badly formed data this just returns null.
        private static string _DecodeQuotedPrintable(string encodedProperty)
        {
            Assert.IsNotNull(encodedProperty);

            var sb = new StringBuilder();
            for (int i = 0; i < encodedProperty.Length; ++i)
            { 
                // look for the escape char.
                if ('=' == encodedProperty[i])
                {
                    if (i + 2 >= encodedProperty.Length)
                    {
                        // badly formed data.  Not enough space for an escape sequence.
                        return null;
                    }

                    int iVal;
                    if (int.TryParse(encodedProperty.Substring(i + 1, 2), NumberStyles.AllowHexSpecifier, null, out iVal))
                    {
                        sb.Append((char)iVal);
                        i += 2;
                    }
                }
                else
                {
                    sb.Append(encodedProperty[i]);
                }
            }
            return sb.ToString();
        }

        private static string _EncodeQuotedPrintable(byte[] property)
        {
            Verify.IsNotNull(property, "property");
            Verify.AreNotEqual(0, property.Length, "property", "Array can't be of length 0");

            const int MaxLine = 76;
            // Only directly print characters between these two values (other than '=')
            const char LowPrintableChar = ' ';
            const char HighPrintableChar = '~';

            var qpString = new StringBuilder();

            // Current length of the line (including expanded characters)
            int lineLength = 0;
            for (int ix = 0; ix < property.Length; ++ix)
            {
                var ch = (char)property[ix];
                
                if (ch == '\t' || (ch >= LowPrintableChar && ch <= HighPrintableChar && ch != '='))
                {
                    if (lineLength >= MaxLine)
                    {
                        // Wrap!  Don't have space for another character.
                        qpString.Append("=" + Crlf);
                        lineLength = 0;
                    }
                    qpString.Append(ch);
                    ++lineLength;
                }
                else
                {
                    if (lineLength >= MaxLine - 3)
                    {
                        // Wrap!  Don't have space for another three (=XX) characters on this line.
                        qpString.Append("=" + Crlf);
                        lineLength = 0;
                    }
                    qpString.Append(string.Format(CultureInfo.InvariantCulture, "={0:X2}", (uint)ch));
                    lineLength += 3;
                }
            }

            // If a '\t' or ' ' appear at the end of a line they need to be =XX escaped.
            int lastIndex = qpString.Length -1;
            char lastChar = qpString[lastIndex];
            if ('\t' == lastChar || ' ' == lastChar)
            {
                qpString.Remove(lastIndex, 1);
                // Need three characters, so lineLength less the one just removed.
                if (lineLength >= MaxLine - 2)
                {
                    // Wrap!  Don't have space for another three (=XX) characters on this line.
                    qpString.Append("=" + Crlf);
                }
                qpString.Append(string.Format(CultureInfo.InvariantCulture, "={0:X2}", (uint)lastChar));
            }

            return qpString.ToString();
        }

        private static bool _ShouldEscapeQuotedPrintable(string property)
        {
            return property.Contains("\n") || property.Contains("\r");
        }

        #endregion

        #region Read VCard Property Functions

        private static void _ReadAddresses(_Property addressProp, Contact contact)
        {
            Assert.IsNotNull(addressProp);
            Assert.IsNotNull(contact);

            // Always create a new address node for any ADR property.
            var addr = new PhysicalAddressBuilder();
            string[] elements = _TokenizeEscapedMultipropString(addressProp.ValueString);

            switch (elements.Length)
            {
                default: // too many.  Ignore extras
                case 7:
                    addr.Country = elements[6];
                    goto case 6;
                case 6:
                    addr.ZipCode = elements[5];
                    goto case 5;
                case 5:
                    addr.State = elements[4];
                    goto case 4;
                case 4:
                    addr.City = elements[3];
                    goto case 3;
                case 3:
                    addr.Street = elements[2];
                    goto case 2;
                case 2:
                    addr.ExtendedAddress = elements[1];
                    goto case 1;
                case 1:
                    addr.POBox = elements[0];
                    break;
                case 0:
                    Assert.Fail("Tokenize shouldn't have yielded an empty array.");
                    break;
            }

            contact.Addresses.Add(addr, addressProp.GetLabels());
        }

        private static void _ReadBirthday(_Property bdayProp, Contact contact)
        {
            Assert.IsNotNull(bdayProp);
            Assert.IsNotNull(contact);

            DateTime bday;
            if (DateTime.TryParse(bdayProp.ValueString, out bday))
            {
                contact.Dates[DateLabels.Birthday] = bday;
            }
        }

        private static void _ReadEmailAddress(_Property emailProp, Contact contact)
        {
            Assert.IsNotNull(emailProp);
            Assert.IsNotNull(contact);

            var email = new EmailAddressBuilder
            {
                Address = emailProp.ValueString
            };
            // Try to determine a type from this
            if (emailProp.Types.Contains("INTERNET") || emailProp.Types.Contains("TYPE=INTERNET"))
            {
                email.AddressType = "SMTP";
            }
            else
            {
                // Try to coerce a type.  Otherwise leave it blank.  Smtp is already implicitly default.
                foreach(string type in emailProp.Types)
                {
                    if (type.StartsWith(_DeclareType, StringComparison.OrdinalIgnoreCase))
                    {
                        email.AddressType = type.Substring(_DeclareType.Length);
                        break;
                    }
                }
            }
            contact.EmailAddresses.Add(email, emailProp.GetLabels());
        }

        private static void _ReadLabel(_Property labelProp, Contact contact)
        {
            // This is directly related to the physical addresses.
            // Without supporting property grouping there's no good way to tell whether
            // this label corresponds to an existing address node (or vice versa).
            // I'm okay with skipping this property on read since the ADR should be present.
            // Better to not get this wrong in the 5% (probably more) case.
        }

        private static void _ReadFormattedName(_Property nameProp, Contact contact)
        {
            Assert.IsNotNull(nameProp);
            Assert.IsNotNull(contact);

            // Don't expect multiple names so just coalesce FN, N, and SOUND to the default Name.
            // Explicitly ignoring any labels set on the name.
            contact.Names.Default = new NameBuilder(contact.Names.Default)
            {
                FormattedName = nameProp.ValueString
            };
        }

        private static void _ReadLogo(_Property logoProp, Contact contact)
        {
            Assert.IsNotNull(logoProp);
            Assert.IsNotNull(contact);
            Assert.IsNotNull(logoProp);
            Assert.IsNotNull(contact);

            Photo photo = _ReadPhotoProperty(logoProp);

            // Add any other labels on the Photo.  Business and Logo are implied by the LOGO type.
            contact.Photos.Add(photo, logoProp.GetLabels(PhotoLabels.Logo, PropertyLabels.Business));
        }

        private static void _ReadMailer(_Property mailerProp, Contact contact)
        {
            Assert.IsNotNull(mailerProp);
            Assert.IsNotNull(contact);

            contact.Mailer = mailerProp.ValueString;
        }

        private static void _ReadName(_Property nameProp, Contact contact)
        {
            Assert.IsNotNull(nameProp);
            Assert.IsNotNull(contact);

            var nb = new NameBuilder(contact.Names.Default);
            string[] names = _TokenizeEscapedMultipropString(nameProp.ValueString);

            switch (names.Length)
            {
                default: // too many.  Ignore extras
                case 5:
                    nb.Suffix = names[4];
                    goto case 4;
                case 4:
                    nb.Prefix = names[3];
                    goto case 3;
                case 3:
                    nb.MiddleName = names[2];
                    goto case 2;
                case 2:
                    nb.GivenName = names[1];
                    goto case 1;
                case 1:
                    nb.FamilyName = names[0];
                    break;
                case 0:
                    Assert.Fail("Tokenize shouldn't have yielded an empty array.");
                    break;
            }
            contact.Names.Default = nb;
        }

        private static void _ReadNotes(_Property notesProp, Contact contact)
        {
            Assert.IsNotNull(notesProp);
            Assert.IsNotNull(contact);

            contact.Notes = notesProp.ValueString;
        }

        private static void _ReadPhoneNumbers(_Property phoneProp, Contact contact)
        {
            Assert.IsNotNull(phoneProp);
            Assert.IsNotNull(contact);

            contact.PhoneNumbers.Add(new PhoneNumber(phoneProp.ValueString), phoneProp.GetLabels());
        }
        
        private static void _ReadPhonetic(_Property nameProp, Contact contact)
        {
            Assert.IsNotNull(nameProp);
            Assert.IsNotNull(contact);

            // Don't expect multiple names so just coalesce FN, N, and SOUND to the default Name.
            // Explicitly ignoring any labels set on the name, except that phonetic must be a URL.
            if (nameProp.Types.Contains(_DeclareValue + _UrlType))
            {
                contact.Names.Default = new NameBuilder(contact.Names.Default)
                {
                    Phonetic = nameProp.ValueString
                };
            }
        }

        private static void _ReadPhoto(_Property photoProp, Contact contact)
        {
            Assert.IsNotNull(photoProp);
            Assert.IsNotNull(contact);

            Photo photo = _ReadPhotoProperty(photoProp);

            // Add any other labels on the Photo.  UserTile is implied by the PHOTO type.
            contact.Photos.Add(photo, photoProp.GetLabels(PhotoLabels.UserTile, null));
        }

        private static Photo _ReadPhotoProperty(_Property photoProp)
        {
            var pb = new PhotoBuilder();

            // support either URLs or inline streams.
            if (photoProp.Types.Contains(_DeclareValue + _UrlType))
            {
                Uri uri;
                if (Uri.TryCreate(photoProp.ValueString, UriKind.RelativeOrAbsolute, out uri))
                {
                    pb.Url = uri;
                }
            }
            else if (null != photoProp.ValueBinary)
            {
                pb.Value = new MemoryStream(photoProp.ValueBinary);

                // look for a type to put into it also.
                pb.ValueType = "image";
                foreach (string token in photoProp.Types)
                {
                    if (token.StartsWith(_DeclareType, StringComparison.OrdinalIgnoreCase))
                    {
                        pb.ValueType = "image/" + token.Substring(_DeclareType.Length);
                        break;
                    }
                }
            }
            return pb;
        }

        private static void _ReadOrganization(_Property orgProp, Contact contact)
        {
            // In VCF there are three related properties: ORG, ROLE, and TITLE.
            // There reasonably could be multiple organizations on a vcard but short
            // of property groupings there's no way to distinguish, and even then
            // no guarantee that the property groupings will be present in the case
            // of multiple sets of these properties.
            // So instead, treat this like name and assume only the default, but rather
            // than use .Default, use the PropertyLabels.Business indexer.
            Assert.IsNotNull(orgProp);
            Assert.IsNotNull(contact);

            var position = new PositionBuilder(contact.Positions[PropertyLabels.Business]);
            string[] elements = _TokenizeEscapedMultipropString(orgProp.ValueString);
            Assert.BoundedInteger(1, elements.Length, int.MaxValue);

            // ORG is weird in that it doesn't actually say what the tokens represent.
            // The first one can be safely assumed to be Company, but anything else it's probably
            // best to just put back the ';'s and stick the string somewhere visible.
            position.Company = elements[0];
            if (elements.Length > 1)
            {
                position.Office = string.Join(";", elements, 1, elements.Length -1);
            }
            contact.Positions[PropertyLabels.Business] = position;
        }

        private static void _ReadRole(_Property roleProp, Contact contact)
        {
            // In VCF there are three related properties: ORG, ROLE, and TITLE.
            // There reasonably could be multiple organizations on a vcard but short
            // of property groupings there's no way to distinguish, and even then
            // no guarantee that the property groupings will be present in the case
            // of multiple sets of these properties.
            // So instead, treat this like name and assume only the default, but rather
            // than use .Default, use the PropertyLabels.Business indexer.
            Assert.IsNotNull(roleProp);
            Assert.IsNotNull(contact);

            var position = new PositionBuilder(contact.Positions[PropertyLabels.Business])
            {
                Role = roleProp.ValueString
            };
            contact.Positions[PropertyLabels.Business] = position;
        }

        private static void _ReadTitle(_Property titleProp, Contact contact)
        {
            // In VCF there are three related properties: ORG, ROLE, and TITLE.
            // There reasonably could be multiple organizations on a vcard but short
            // of property groupings there's no way to distinguish, and even then
            // no guarantee that the property groupings will be present in the case
            // of multiple sets of these properties.
            // So instead, treat this like name and assume only the default, but rather
            // than use .Default, use the PropertyLabels.Business indexer.
            Assert.IsNotNull(titleProp);
            Assert.IsNotNull(contact);

            var position = new PositionBuilder(contact.Positions[PropertyLabels.Business])
            {
                JobTitle = titleProp.ValueString
            };
            contact.Positions[PropertyLabels.Business] = position;
        }

        private static void _ReadUniqueIdentifier(_Property nameProp, Contact contact)
        {
            // Someone (maybe us) bothered to put a UID on the contact, so if it matches
            //    our ContactId's GUID format then go ahead and use it.
            Guid id;
            if (Utility.GuidTryParse(nameProp.ValueString, out id))
            {
                contact.ContactIds.Default = id;
            }
        }

        private static void _ReadUrl(_Property urlProp, Contact contact)
        {
            Assert.IsNotNull(urlProp);
            Assert.IsNotNull(contact);

            Uri uri;
            if (Uri.TryCreate(urlProp.ValueString, UriKind.RelativeOrAbsolute, out uri))
            {
                contact.Urls.Add(uri, urlProp.GetLabels());
            }
        }

        #endregion

        #region Write VCard Property Functions

        private static void _WriteAddresses(Contact contact, TextWriter sw)
        {
            for (int i = 0; i < contact.Addresses.Count; ++i)
            {
                PhysicalAddress address = contact.Addresses[i];
                // ADR:Address;Structured
                // Escape ';' in multiprops with a '\'
                // Note that WAB doesn't actually escape the ; properly, so it may mess up on this read.
                var adrPropBuilder = new StringBuilder();
                adrPropBuilder
                    .Append(address.POBox.Replace(";", "\\;")).Append(";")
                    .Append(address.ExtendedAddress.Replace(";", "\\;")).Append(";")
                    .Append(address.Street.Replace(";", "\\;")).Append(";")
                    .Append(address.City.Replace(";", "\\;")).Append(";")
                    .Append(address.State.Replace(";", "\\;")).Append(";")
                    .Append(address.ZipCode.Replace(";", "\\;")).Append(";")
                    .Append(address.Country.Replace(";", "\\;"));
                string adrProp = adrPropBuilder.ToString();
                // If there aren't any properties then don't write this.
                if (adrProp.Replace(";", null).Length > 0)
                {
                    _WriteLabeledProperty(sw, "ADR", contact.Addresses.GetLabelsAt(i), null, adrProp);
                }

                if (!string.IsNullOrEmpty(address.AddressLabel))
                {
                    _WriteLabeledProperty(sw, "LABEL", contact.Addresses.GetLabelsAt(i), null, address.AddressLabel);
                }
            }
        }

        private static void _WriteBirthday(Contact contact, TextWriter sw)
        {
            DateTime? bday = contact.Dates[DateLabels.Birthday];
            if (null != bday)
            {
                // ISO 8601 format
                _WriteStringProperty(sw, "BDAY", bday.Value.ToString("s", CultureInfo.InvariantCulture));
            }
        }

        private static void _WriteEmailAddresses(Contact contact, TextWriter sw)
        {
            // EMAIL:E-mail addresses
            for (int i = 0; i < contact.EmailAddresses.Count; ++i)
            {
                EmailAddress email = contact.EmailAddresses[i];
                if (!string.IsNullOrEmpty(email.Address))
                {
                    string addInternet = null;
                    if (string.IsNullOrEmpty(email.AddressType)
                        || string.Equals("SMTP", email.AddressType, StringComparison.OrdinalIgnoreCase))
                    {
                        addInternet = "INTERNET";
                    }
                    _WriteLabeledProperty(sw, "EMAIL", contact.EmailAddresses.GetLabelsAt(i), addInternet, email.Address);
                }
            }
        }

        private static void _WriteLabeledProperty(TextWriter sw, string propertyPrefix, IEnumerable<string> labels, string additionalLabel, string value)
        {
            Assert.IsFalse(propertyPrefix.EndsWith(":", StringComparison.Ordinal));
            var propertyBuilder = new StringBuilder();
            propertyBuilder.Append(propertyPrefix);
            if (!string.IsNullOrEmpty(additionalLabel))
            {
                // This shouldn't be added for any case of this.
                Assert.IsFalse(additionalLabel.Contains(";"));
                propertyBuilder.Append(";").Append(additionalLabel);
            }
            foreach (string label in labels)
            {
                Assert.IsNeitherNullNorEmpty(label);
                propertyBuilder.Append(";");
                // If the label's not mapped then don't write it.
                // Expect unmapped labels to contain ":", which are completely illegal
                string mapped;
                if (_labelMap.TryGetValue(label, out mapped))
                {
                    propertyBuilder.Append(mapped);
                }
            }

            _WriteStringProperty(sw, propertyBuilder.ToString(), value);
        }

        private static void _WriteMailer(Contact contact, TextWriter sw)
        {
            // MAILER:mailer
            string mailer = contact.Mailer;
            if (!string.IsNullOrEmpty(mailer))
            {
                _WriteStringProperty(sw, "MAILER", mailer);
            }
        }

        private static void _WriteName(Contact contact, TextWriter sw)
        {
            // The VCF spec implies that name isn't multi-valued, so don't mess with enumerating them.
            // Just use the default.

            // FN:Formatted Name
            Name name = contact.Names.Default;
            if (!string.IsNullOrEmpty(name.FormattedName))
            {
                _WriteStringProperty(sw, "FN", name.FormattedName);
            }

            // SOUND:Phonetic of FN.
            if (!string.IsNullOrEmpty(name.Phonetic))
            {
                _WriteStringProperty(sw, "SOUND", name.Phonetic);
            }

            // N:Name;Structured
            // Escape ';' in multiprops with a '\'
            // Note that WAB doesn't actually escape the ; properly, so it may mess up on this read.
            var nPropBuilder = new StringBuilder();
            nPropBuilder
                .Append(name.FamilyName.Replace(";", "\\;")).Append(";")
                .Append(name.GivenName.Replace(";", "\\;")).Append(";")
                .Append(name.MiddleName.Replace(";", "\\;")).Append(";")
                .Append(name.Prefix.Replace(";", "\\;")).Append(";")
                .Append(name.Suffix.Replace(";", "\\;"));
            string nProp = nPropBuilder.ToString();
            // If there aren't any properties then don't write this.
            if (nProp.Replace(";", null).Length > 0)
            {
                _WriteStringProperty(sw, "N", nProp);
            }
        }

        private static void _WriteNotes(Contact contact, TextWriter sw)
        {
            // NOTE:Notes
            string note = contact.Notes;
            if (!string.IsNullOrEmpty(note))
            {
                _WriteStringProperty(sw, "NOTE", note);
            }
        }

        private static void _WriteOrganization(Contact contact, TextWriter sw)
        {
            // Organizational properties
            Position position = contact.Positions[PropertyLabels.Business];

            // Title:
            if (!string.IsNullOrEmpty(position.JobTitle))
            {
                _WriteStringProperty(sw, "TITLE", position.JobTitle);
            }

            // ROLE:Business Category
            if (!string.IsNullOrEmpty(position.Role))
            {
                _WriteStringProperty(sw, "ROLE", position.Role);
            }

            // LOGO: Company logo
            // Contact schema doesn't directly associate the logo with the business, but it's also
            // only kind of implied by vCards that they are also.  Can use the [Business,Logo] labels here.
            Photo logo = contact.Photos[PropertyLabels.Business, PhotoLabels.Logo];
            if (logo != default(Photo))
            {
                _WritePhotoProperty(sw, "LOGO", logo);
            }

            // AGENT:Embedded vCard (Unsupported)
            // Contacts can contain links to other contacts, but not going to expose this through vCard export.

            // ORG: Structured organization description
            var oPropBuilder = new StringBuilder();
            // Only the first field is actually defined (Name), the others are just kindof open-ended,
            // so don't write unnecessary properties.  Unfortunately on read we also won't know what
            // these properties are actually supposed to represent.
            oPropBuilder.Append(position.Company.Replace(";", "\\;"));
            foreach (string orgInfo in new[] { position.Organization, position.Profession, position.Department, position.Office })
            {
                if (!string.IsNullOrEmpty(orgInfo))
                {
                    oPropBuilder.Append(";")
                        .Append(orgInfo.Replace(";", "\\;"));
                }
            }
            string oProp = oPropBuilder.ToString();
            // If there aren't any properties then don't write this.
            if (oProp.Replace(";", null).Length > 0)
            {
                _WriteStringProperty(sw, "ORG", oProp);
            }
        }

        private static void _WritePhoneNumbers(Contact contact, TextWriter sw)
        {
            // TEL:Telephone numbers
            for (int i = 0; i < contact.PhoneNumbers.Count; ++i)
            {
                PhoneNumber number = contact.PhoneNumbers[i];
                if (!string.IsNullOrEmpty(number.Number))
                {
                    _WriteLabeledProperty(sw, "TEL", contact.PhoneNumbers.GetLabelsAt(i), null, number.Number);
                }
            }
        }

        private static void _WritePhoto(Contact contact, TextWriter sw)
        {
            Photo userTile = contact.Photos[PhotoLabels.UserTile];
            if (userTile != default(Photo))
            {
                _WritePhotoProperty(sw, "PHOTO", userTile);
            }
        }

        private static void _WritePhotoProperty(TextWriter sw, string propertyPrefix, Photo photo)
        {
            // 76 is more correct.  I'm trying to emulate Outlook 2007's formatting here.
            const int MaxLine = 72;
            // When writing the photo if it's inline it can be base64 encoded, or VALUE=URL:<url>
            if (null != photo.Value && 0 != photo.Value.Length)
            {
                var bytes = new byte[photo.Value.Length];
                photo.Value.Position = 0;
                photo.Value.Read(bytes, 0, bytes.Length);
                // Base64FormattingOptions doesn't give me the option of shifting the newlines,
                //    so need to do it manually.
                string encoded = Convert.ToBase64String(bytes, Base64FormattingOptions.None);
                string photoType = null;
                if (!string.IsNullOrEmpty(photo.ValueType))
                {
                    // Expecting mime-types, so just give the type as the value after the '/'
                    photoType = ";" + _DeclareType + photo.ValueType.Substring(Math.Max(0, photo.ValueType.LastIndexOf('/')));
                }

                sw.Write(propertyPrefix + photoType + ";" + _DeclareEncoding + _EncodingBase64 + ":");
                for (int i = 0; i < encoded.Length; i += MaxLine)
                {
                    sw.Write(Crlf + " ");
                    sw.Write(encoded.Substring(i, Math.Min(encoded.Length-i, MaxLine)));
                }
                sw.Write(Crlf);
                sw.Write(Crlf);
            }
            else if (null != photo.Url && !string.IsNullOrEmpty(photo.Url.ToString()))
            {
                _WriteStringProperty(sw, propertyPrefix + ";" + _DeclareValue + _UrlType, photo.Url.ToString());
            }
        }

        private static void _WriteStringProperty(TextWriter sw, string propertyPrefix, string value)
        {
            Assert.IsFalse(propertyPrefix.EndsWith(":", StringComparison.Ordinal));
            sw.Write(propertyPrefix);
            if (_ShouldEscapeQuotedPrintable(value))
            {
                sw.Write(";" + _DeclareEncoding + _EncodingQuotedPrintable + ":");
                sw.Write(_EncodeQuotedPrintable(Encoding.ASCII.GetBytes(value)));
            }
            else
            {
                sw.Write(":");
                sw.Write(value);
            }
            sw.Write(Crlf);
        }

        private static void _WriteUniqueIdentifier(Contact contact, TextWriter sw)
        {
            // UID:Universal identifier
            Assert.IsTrue(contact.ContactIds.Default.HasValue);
            _WriteStringProperty(sw, "UID", contact.ContactIds.Default.Value.ToString());
        }

        private static void _WriteUrls(Contact contact, TextWriter sw)
        {
            // URL:Webpages
            for (int i = 0; i < contact.Urls.Count; ++i)
            {
                Uri uri = contact.Urls[i];
                if (null != uri && !string.IsNullOrEmpty(uri.ToString()))
                {
                    _WriteLabeledProperty(sw, "URL", contact.Urls.GetLabelsAt(i), null, uri.ToString());
                }
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="filePath"></param>
        /// <remarks>This implementation is based on the VersitCard 2.1 specification.</remarks>
        /// <returns></returns>
        public static void EncodeToVCard(Contact contact, string filePath)
        {
            Verify.IsNotNull(contact, "contact");

            using (var sw = new StreamWriter(filePath, false, Encoding.ASCII))
            {
                _EncodeToVCardStream(contact, sw);
            }
        }

        // vCards can be chained together in a single file.
        // This is useful as a transport mechanism.
        public static void EncodeCollectionToVCard(IList<Contact> contacts, string filePath)
        {
            Verify.IsNotNull(contacts, "contacts");

            using (var sw = new StreamWriter(filePath, false, Encoding.ASCII))
            {
                foreach (Contact c in contacts)
                {
                    if (null != c)
                    {
                        _EncodeToVCardStream(c, sw);
                        // A linebreak visually separates each vcard within the file.
                        sw.WriteLine();
                    }
                }
            }
        }

        // This function writes vCard properties as they're described in the vCard 2.1 specification.
        // For several of the properties in the spec it lists valid type tags (Property Parameters)
        //   that can be added, the equivalent of the Contact Schema's labels.  Not all properties
        //   have these types, even though most of them really should, e.g. e-mails and urls.
        // It also supports groupings of properties, but it doesn't really say how they're supposed
        //   to be consumed.  This doesn't exactly map to a concept in Contacts other than to
        //   ensure that the same node contains data disparate in the vcard, e.g. Org-Role-Title.
        //   That there's a grouping implies that it's OK with multiple values for any property, but
        //   I really don't think most applications are expecting to consume multiple name properties.
        // The spec doesn't say that the property parameters are strictly limited to the enumerated
        //   properties.  In fact, WAB writes HOME and WORK on the URLs.  It also doesn't say whether
        //   the enumerated properties are the only ones that can be added.  For properties where it's
        //   reasonable for VCFs to have multiple values I add the additional mapped Contact labels.
        //   For types, such as EmailType, I'm just going to embed the string as the label.  The types
        //   of e-mail addresses in 1996 doesn't make sense in 2007, and any updated list isn't going
        //   to make sense in 2018, so it seems silly to try and guess (GMAIL as PRODIGY, anyone?)
        //
        // 2.1 is much more ubiquitous than 3.0, and 3.0 has more ambiguities regarding a lot of this
        //   than 2.1 does.  Neither do a great job of addressing globalization issues.  Since this
        //   is just a transport mechanism (vcard itself is just designed to be embedded in mime) I'm
        //   sticking with 2.1.
        private static void _EncodeToVCardStream(Contact contact, StreamWriter sw)
        {
            Verify.IsNotNull(contact, "contact");
            Assert.IsNotNull(sw);

            // VCard properties that are not written:
            // * TZ:TimeZone
            // * GEO: latitude/longitude coordinates
            // * AGENT: nor any other kind of embedded reference to other contacts
            // * CERT: Certificates.

            _WriteStringProperty(sw, "BEGIN", "VCARD");
            _WriteStringProperty(sw, "VERSION", "2.1");
            _WriteName(contact, sw);
            _WritePhoto(contact, sw);
            _WriteBirthday(contact, sw);
            _WriteAddresses(contact, sw);
            _WritePhoneNumbers(contact, sw);
            _WriteEmailAddresses(contact, sw);
            _WriteMailer(contact, sw);
            _WriteOrganization(contact, sw);
            _WriteNotes(contact, sw);
            _WriteUrls(contact, sw);
            _WriteUniqueIdentifier(contact, sw);
            _WriteStringProperty(sw, "REV", DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture));
            _WriteStringProperty(sw, "END", "VCARD");

            sw.Flush();
        }

        private static string _ReadVCardItem(TextReader tr)
        {
            // BUGBUG:
            // VCard 2.1 Spec 2.1.3:
            //   "Long lines of text can be split into a multiple-line
            //   representation using the RFC 822 “folding” technique.
            //   That is, wherever there may be linear white space
            //   (NOT simply LWSP-chars), a CRLF immediately followed by
            //   at least one LWSP-char may instead be inserted."
            // This is one of the parts of the VCard spec that hasn't aged well.
            // Support for this requires peeking ahead in the stream to see if
            // the next line follows the pattern.  This isn't reasonable for
            // most stream implementations.  It also only helps when the data is
            // split with white space.  Most vCard readers I've seen also
            // ignore that part of the spec so I'm less concerned about it than
            // I am with other issues (e.g. CHARSET support).

            // Quoted Printable encoded properties curry based on a trailing '='.
            // Base 64 encoded properties curry until an empty line is read.

            bool isQuotedPrintable;
            bool isBase64;
            string line;
            do
            {
                line = tr.ReadLine();
                if (null == line)
                {
                    return null;
                }

                // Every property in a vCard should have a ':'...
                // Is it reasonable to fail here if -1?
                // Lines between vCard objects might look like this.
                // At the very least we're not going to parse this for multiple lines.
                int delimeterIndex = Math.Max(line.IndexOf(':'), 0);

                string propertyDeclaration = line.Substring(0, delimeterIndex).ToUpperInvariant();

                // Do this calculation inside the loop.
                // If there's both QUOTED-PRINTABLE and BASE64 I'm not going to try and guess.
                isQuotedPrintable = propertyDeclaration.Contains(_EncodingQuotedPrintable);
                isBase64 = propertyDeclaration.Contains(_EncodingBase64);

                // Property names must come at the beginning of the line.
                //   Per above comment a valid vcard may have a line start with a space,
                //   so we'll just ignore it.
            } while (string.IsNullOrEmpty(line)
                || line.StartsWith(" ", StringComparison.Ordinal)
                || (isQuotedPrintable && isBase64));

            var sbProperty = new StringBuilder();
            if (isQuotedPrintable)
            {
                bool carry;
                do
                {
                    carry = line.EndsWith("=", StringComparison.Ordinal);
                    sbProperty.Append(line, 0, line.Length - (carry ? 1 : 0));
                    // If there was a soft line break read the next line also.
                } while (carry && null != (line = tr.ReadLine()));
            }
            else if (isBase64)
            {
                do
                {
                    sbProperty.Append(line);
                    line = tr.ReadLine();
                    // Keep reading until there's a blank line.
                } while (!string.IsNullOrEmpty(line));
            }
            else
            {
                // not a multi-line property.
                sbProperty.Append(line);
            }

            return sbProperty.ToString();
        }

        public static ICollection<Contact> ReadVCard(TextReader tr)
        {
            var retContacts = new List<Contact>();

            try
            {
                var vcardProperties = new Stack<List<_Property>>();

                string line;
                while (null != (line = _ReadVCardItem(tr)))
                {
                    if (_BeginVCardProperty.Equals(line, StringComparison.OrdinalIgnoreCase))
                    {
                        vcardProperties.Push(new List<_Property>());
                    }
                    // If we're not currently reading a vcard then we don't care.
                    else if (vcardProperties.Count > 0)
                    {
                        if (_EndVCardProperty.Equals(line, StringComparison.OrdinalIgnoreCase))
                        {
                            List<_Property> vcard = vcardProperties.Pop();
                            retContacts.Add(_ParseVCard(vcard));
                        }
                        else
                        {
                            _Property prop;
                            if (_TryParseVCardProperty(line, out prop))
                            {
                                vcardProperties.Peek().Add(prop);
                            }
                        }
                    }
                }
            }
            catch
            {
                // If there's an Exception then dispose of the pending contacts.
                foreach (Contact c in retContacts)
                {
                    c.Dispose();
                }
                throw;
            }
            return retContacts;
        }

        private static bool _TryParseVCardProperty(string line, out _Property prop)
        {
            Assert.IsNotNull(line);
            prop = null;

            string valueString;
            byte[] valueBinary = null;

            int colonIndex = line.IndexOf(':');
            // If this doesn't contain a colon it's not a property.
            if (colonIndex == -1)
            {
                return false;
            }

            prop = new _Property();
            
            // Note some properties, such as AGENT may be empty.
            // AGENT is actually a weird case, because its property is the embedded vcard object
            // that follows on the next line.
            valueString = line.Substring(colonIndex + 1);
            
            string[] propTags = _TokenizeEscapedMultipropString(line.Substring(0, colonIndex));
            Assert.BoundedInteger(1, propTags.Length, int.MaxValue);
            
            // Ignoring group tags for now.
            // Don't know of any clients that write them with an expectation of their consumption.
            int dotIndex = propTags[0].IndexOf('.');
            if (-1 != dotIndex)
            {
                // Mildly concerned about property strings that look like "GROUP.;TYPE=..."
                // which isn't a property... Shouldn't need to special case it though.
                propTags[0] = propTags[0].Substring(dotIndex);
            }
            prop.Name = propTags[0];
            for (int i = 1; i < propTags.Length; ++i)
            {
                // Look for encodings before putting it into the _Property.
                // Ignoring CHARSETs altogether here.
                if (propTags[i].StartsWith(_DeclareEncoding, StringComparison.OrdinalIgnoreCase))
                {
                    propTags[i] = propTags[i].Substring(_DeclareEncoding.Length);
                }

                // These strings don't strictly require the ENCODING= prefix.
                // They're unambiguous even without it, so look for them directly as well.
                if (propTags[i].Equals(_EncodingBase64))
                {
                    valueBinary = Convert.FromBase64String(valueString);
                    valueString = null;

                    // skip assigning this as a type.
                    continue;
                }

                if (propTags[i].Equals(_EncodingQuotedPrintable))
                {
                    valueString = _DecodeQuotedPrintable(valueString);
                    // skip assigning this as a type.
                    continue;
                }

                prop.Types.Add(propTags[i]);
            }

            prop.ValueString = valueString;
            prop.ValueBinary = valueBinary;

            return true;
        }

        private static Contact _ParseVCard(List<_Property> vcard)
        {
            Assert.IsNotNull(vcard);

            var contact = new Contact();

            foreach (_Property prop in vcard)
            {
                _ReadVCardProperty mapFunc;
                if (_writeVCardPropertiesMap.TryGetValue(prop.Name, out mapFunc))
                {
                    mapFunc(prop, contact);
                }
            }

            return contact;
        }
    }
}
