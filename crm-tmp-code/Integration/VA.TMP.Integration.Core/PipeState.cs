using System;

namespace VA.TMP.Integration.Core
{
    public class PipeState : IDisposable
    {
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Destructor
        ~PipeState()
        {
            Dispose(false);
        }
    }
}