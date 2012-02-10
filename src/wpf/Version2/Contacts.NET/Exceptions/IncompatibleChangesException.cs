/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception that's thrown when an attempt has been made to save changes to a contact
    /// that are incompatible with other changes that were made since the contact was loaded.
    /// </summary>
    [Serializable]
    public class IncompatibleChangesException : Exception
    {
        public IncompatibleChangesException()
        {}

        /// <summary>
        /// Create an instance of this exception with a message for the caller.
        /// </summary>
        /// <param name="message">A string containing additional context about the exception.</param>
        public IncompatibleChangesException(string message)
            : base(message)
        {}

        public IncompatibleChangesException(string message, Exception innerException)
            : base(message, innerException)
        {}

        protected IncompatibleChangesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}
