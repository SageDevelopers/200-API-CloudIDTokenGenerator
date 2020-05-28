using System;

namespace SageCloudIDTokenGeneratorv1._1
{
    /// <summary>
    /// Exception raised by the identity provider to indicate user cancelled during authentication
    /// </summary>
    public class AuthenticationCancelException : System.Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public AuthenticationCancelException()
            : base()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public AuthenticationCancelException(string message)
            : base(message)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public AuthenticationCancelException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
