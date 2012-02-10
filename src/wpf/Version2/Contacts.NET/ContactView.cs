/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System;

    public class ContactView : IDisposable
    {
        #region Fields
        private Contact _contact;
        private bool _disposed;
        #endregion

        private void _Verify()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("this");
            }
        }

        protected ContactView(Contact source)
        {
            _contact = source;
        }
        
        protected Contact Source
        {
            get
            {
                _Verify();
                return _contact;
            }
        }

        #region IDisposable Pattern

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Don't dispose of the source contact.  This is just a view over it.
                _contact = null;
                _disposed = true;
            }
        }

        #endregion

    }
}
