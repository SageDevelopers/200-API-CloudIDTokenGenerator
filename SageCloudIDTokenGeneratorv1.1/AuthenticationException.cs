using System;

namespace SageCloudIDTokenGeneratorv1._1
{
    /// <summary>
    /// Exception raised by the identity provider during authentication
    /// </summary>
    public class AuthenticationException : System.Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public AuthenticationException()
            : base()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public AuthenticationException(string message)
            : base(message)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public AuthenticationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
