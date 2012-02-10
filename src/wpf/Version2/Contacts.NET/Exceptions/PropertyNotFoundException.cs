/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using Standard;

    /// <summary>
    /// Exception that's thrown when a requested Contact property was not found.
    /// </summary>
    /// <remarks>
    /// Often Contact methods will just return null or a default value when a requested
    /// property is missing.  Callers should check the documentation to ensure whether
    /// an exception is thrown on a per-method basis.
    /// </remarks>
    [Serializable]
    public class PropertyNotFoundException : Exception
    {
        public string MissingProperty { get; private set; }

        public PropertyNotFoundException()
        {}

        /// <summary>
        /// Create an instance of this exception with a message for the caller.
        /// </summary>
        /// <param name="message">A string containing additional context about the exception.</param>
        public PropertyNotFoundException(string message)
            : base(message)
        {}

        public PropertyNotFoundException(string message, string missingProperty)
            : base(message)
        {
            MissingProperty = missingProperty;
        }

        public PropertyNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {}

        protected PropertyNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            // base should have verified this...
            Assert.IsNotNull(info);

            info.AddValue("MissingProperty", MissingProperty);
        }
    }
}
