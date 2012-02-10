/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception that's thrown because the Contact's data is in an inconsistent state and must be reloaded.
    /// </summary>
    [Serializable]
    public class InvalidStateException : Exception
    {
        public InvalidStateException()
        {}

        /// <summary>
        /// Create an instance of this exception with a message for the caller.
        /// </summary>
        /// <param name="message">A string containing additional context about the exception.</param>
        public InvalidStateException(string message)
            : base(message)
        {}

        public InvalidStateException(string message, Exception innerException)
            : base(message, innerException)
        {}

        protected InvalidStateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}
