
namespace Microsoft.Communications.Contacts
{
    using Standard;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// A view that exposes Windows Contacts Group properties on a Contact
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class MapiGroupView : ContactView
    {
        private const string _ContactIdKey = "[MSWABMAPI]PropTag0x66001102";
        private const string _NameEmailKey = "[MSWABMAPI]PropTag0x80091102";
        private const string _MapiPrefix = "CID_V1:";
        // One-off members contain some unused prefix data.
        // It's of constant size and can be ignored.
        private const int _OneOffPrefixJunk = 24;

        private readonly List<string> _contactIds;
        private readonly List<Person> _oneOffs;

        /// <summary>
        /// Check whether a Contact contains properties for the paths of MAPI Groups.
        /// </summary>
        /// <param name="contact">The contact to check for MAPI-groupness</param>
        /// <returns>
        /// Returns true if the contact would have values for either MemberIds or
        /// MemberNameEmailPairs if the contact was viewed as a MAPI group.  False
        /// otherwise.
        /// </returns>
        /// <remarks>
        /// This does not check that the properties are valid, just that the property
        /// paths of MAPI Group properties exist in the given contact.
        /// </remarks>
        public static bool HasMapiProperties(Contact contact)
        {
            Verify.IsNotNull(contact, "contact");

            using (Stream stm = contact.GetBinaryProperty(_ContactIdKey, null))
            {
                if (null != stm)
                {
                    return true;
                }
            }

            using (Stream stm = contact.GetBinaryProperty(_NameEmailKey, null))
            {
                if (null != stm)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get a view over the contact that accesses Windows Contacts group properties.
        /// </summary>
        /// <param name="contact">The contact to create the group-view over.</param>
        /// <remarks>
        /// This does not create an actively updated view over the contact's properties.
        /// Changes to the contact in memory or on disk are not reflected by this instance.
        /// </remarks>
        public MapiGroupView(Contact contact)
            : base(contact)
        {
            var encoding = new UnicodeEncoding();

            _contactIds = new List<string>();
            _oneOffs = new List<Person>();

            using (Stream stm = contact.GetBinaryProperty(_ContactIdKey, null))
            {
                if (null != stm)
                {
                    using (var binreader = new BinaryReader(stm))
                    {
                        try
                        {
                            int idCount = binreader.ReadInt32();
                            for (int i = 0; i < idCount; ++i)
                            {
                                int cb = binreader.ReadInt32();
                                byte[] id = binreader.ReadBytes(cb);
                                // Seeing a double-null termination.  Trim it...
                                Assert.AreEqual<byte>(0, id[id.Length - 1]);
                                Assert.AreEqual<byte>(0, id[id.Length - 2]);
                                string mapiId = encoding.GetString(id, 0, id.Length - 2);
                                if (!mapiId.StartsWith(_MapiPrefix, StringComparison.Ordinal))
                                {
                                    throw new FormatException("Group contains invalid member data.  ContactIds don't match the expected format.");
                                }
                                _contactIds.Add(mapiId.Substring(_MapiPrefix.Length));
                            }
                        }
                        catch (EndOfStreamException)
                        {
                            throw new FormatException("Group contains invalid member data.  The data stream ended prematurely.");
                        }
                        if (stm.Position != stm.Length)
                        {
                            throw new FormatException("Group contains invalid member data.  More data was found than was specified in the header.");
                        }
                    }
                }
            }
            using (Stream stm = contact.GetBinaryProperty(_NameEmailKey, null))
            {
                if (null != stm)
                {
                    using (var binreader = new BinaryReader(stm))
                    {
                        try
                        {
                            int pairCount = binreader.ReadInt32();
                            for (int i = 0; i < pairCount; ++i)
                            {
                                int cb = binreader.ReadInt32();
                                byte[] id = binreader.ReadBytes(cb);
                                // Seeing a double-null termination.  Trim it...
                                Assert.AreEqual<byte>(0, id[id.Length - 1]);
                                Assert.AreEqual<byte>(0, id[id.Length - 2]);
                                string mapiId = encoding.GetString(id, _OneOffPrefixJunk, id.Length - 2 - _OneOffPrefixJunk);
                                // Should contain three elements: Name, e-mail type, e-mail address.
                                string[] tokens = mapiId.Split(new[] {'\0'}, StringSplitOptions.None);
                                if (3 != tokens.Length)
                                {
                                    throw new FormatException("Group contains invalid one-off member data.");
                                }
                                // Throw away the e-mail address type.
                                _oneOffs.Add(new Person(tokens[0], null, tokens[2], null));
                            }
                        }
                        catch (EndOfStreamException)
                        {
                            throw new FormatException("Group contains invalid member data.  The data stream ended prematurely.");
                        }
                        if (stm.Position != stm.Length)
                        {
                            throw new FormatException("Group contains invalid member data.  More data was found than was specified in the header.");
                        }
                    }
                }
            }
        }

        public ICollection<string> MemberIds
        {
            get
            {
                return _contactIds.AsReadOnly(); 
            }
        }

        public ICollection<Person> OneOffMembers
        {
            get
            {
                return _oneOffs.AsReadOnly();
            }
        }
    }
}
