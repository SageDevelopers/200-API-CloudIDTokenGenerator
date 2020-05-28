using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace SageCloudIDTokenGeneratorv1._1
{
    /// <summary>
    /// Mutex to restrict access to SageID Client library settings held in Isolated Storage
    /// </summary>
    /// <remarks>
    /// Isolated Storage crashes when multiple processes attempt concurrent access.
    /// </remarks>
    public class SageIDClientMutex : IDisposable
    {
        private static Mutex _mutex;

        /// <summary>
        /// Static constructor to set security access for Mutex
        /// </summary>
        static SageIDClientMutex()
        {
            _mutex = new Mutex(false, "TestApp.APIClient.SageIDClientMutex");

            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            _mutex.SetAccessControl(securitySettings);
        }

        /// <summary>
        /// Wait for mutex
        /// </summary>
        private static void Wait()
        {
            _mutex.WaitOne();
        }

        /// <summary>
        /// Release the mutex
        /// </summary>
        private static void Release()
        {
            _mutex.ReleaseMutex();
        }

        /// <summary>
        /// Constructor, waits for mutex
        /// </summary>
        public SageIDClientMutex()
        {
            Wait();
        }


        /// <summary>
        /// Dispose, releases mutex
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }


        /// <summary>
        /// Dispose, releases mutex
        /// </summary>
        /// <param name="isDisposing"></param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                Release();
            }
        }
    }
}