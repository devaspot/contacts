/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Collections.Generic;
    using Standard;

    public class GroupView : ContactView
    {
        private ILabeledPropertyCollection<Person> _members;
        private readonly ContactManager _manager;

        public GroupView(Contact contact)
            : base(contact)
        {
            if (contact.ContactType != ContactTypes.Group)
            {
                throw new ArgumentException("Contact is not a group.", "contact");
            }
            _manager = contact.Manager;
        }

        private void _Validate()
        {
            Source.VerifyAccess();
        }

        // These committer/creator functions are a bit indirect, but I don't think it should matter.

        private static void _CommitPerson(Contact contact, string arrayNode, Person value)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.PersonCollection + PropertyNames.PersonArrayNode, StringComparison.Ordinal));
            int index = PropertyNameUtil.GetIndexFromNode(arrayNode);
            contact.People[index] = value;
        }

        private static Person _CreatePerson(Contact contact, string arrayNode)
        {
            Assert.IsTrue(arrayNode.StartsWith(PropertyNames.PersonCollection + PropertyNames.PersonArrayNode, StringComparison.Ordinal));
            int index = PropertyNameUtil.GetIndexFromNode(arrayNode);
            return contact.People[index];
        }

        public ILabeledPropertyCollection<Person> Members
        {
            get
            {
                _Validate();
                if (null == _members)
                {
                    _members = new SchematizedLabeledPropertyCollection<Person>(Source, PropertyNames.PersonCollection, PropertyNames.PersonArrayNode, PersonLabels.Member, _CreatePerson, _CommitPerson);
                }
                return _members;
            }
        }

        /// <summary>
        /// Expand the members of this group to gather the e-mail addresses.
        /// </summary>
        public ICollection<string> ExpandEmailAddresses()
        {
            var retList = new List<string>();
            // As we recurse through the members, keep track of the ContactIds
            //    to avoid recursively looping.
            var idList = new List<string>();

            _ExpandEmailAddresses(idList, retList);

            return retList;
        }

        private void _ExpandEmailAddresses(ICollection<string> idList, ICollection<string> retList)
        {
            foreach (Person person in Members)
            {
                // If the person looks like an Id and doesn't have an overriding email, try to resolve it with the Manager
                if (string.IsNullOrEmpty(person.Email) && null != _manager)
                {
                    Contact contact = null;
                    try
                    {
                        if (_manager.TryGetContact(person.Id, out contact))
                        {
                            retList.Add(contact.EmailAddresses.Default.Address);
                        }
                    }
                    finally
                    {
                        Utility.SafeDispose(ref contact);
                    }
                }

                if (person.ContactType == ContactTypes.Group && null != _manager)
                {
                    Contact contact = null;
                    try
                    {
                        if (_manager.TryGetContact(person.Id, out contact))
                        {
                            // Double-check this.
                            // Just because it was true when the Person was created doesn't mean it's valid.
                            if (ContactTypes.Group == contact.ContactType)
                            {
                                if (!idList.Contains(contact.Id))
                                {
                                    idList.Add(contact.Id);
                                    _ExpandEmailAddresses(idList, retList);
                                }
                            }
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // Can't count on the ContactManager staying alive,
                        // but it's unlikely any other exception should get thrown.
                    }
                    finally
                    {
                        Utility.SafeDispose(ref contact);
                    }
                }

                // Pick up the e-mail address for this member, even if it's also a group.
                if (!string.IsNullOrEmpty(person.Email))
                {
                    retList.Add(person.Email);
                }
            }
        }
    }
}
