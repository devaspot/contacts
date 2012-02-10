/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception that's thrown because the operation would cause the contact's
    /// data to violate its internal schema requirements.
    /// </summary>
    [Serializable]
    public class SchemaException : Exception
    {
        public SchemaException()
        {}

        /// <summary>
        /// Create an instance of this exception with a message for the caller.
        /// </summary>
        /// <param name="message">A string containing additional context about the exception.</param>
        public SchemaException(string message)
            : base(message)
        {}

        public SchemaException(string message, Exception innerException)
            : base(message, innerException)
        {}

        protected SchemaException(SerializationInfo info, StreamingContext context)
            : base (info, context)
        {}
    }
}
