/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception that's thrown because the requested Contact couldn't be found in the given context.
    /// </summary>
    [Serializable]
    public class UnreachableContactException : Exception
    {
        public UnreachableContactException()
        {}

        /// <summary>
        /// Create an instance of this exception with a message for the caller.
        /// </summary>
        /// <param name="message">A string containing additional context about the exception.</param>
        public UnreachableContactException(string message)
            : base(message)
        {}

        public UnreachableContactException(string message, Exception innerException)
            : base(message, innerException)
        {}

        protected UnreachableContactException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}
