using System;
using System.Threading.Tasks;
using System.Threading;
using Auth0.OidcClient;
using Sage.CloudID.OAuthClient;
using Sage.CloudID.OAuthClient.LowLevel;
using Sage.CloudID.OAuthClient.Storage;
using Sage.SCI.OAuthClient.WPFForm;

namespace SageCloudIDTokenGeneratorv1._1
{
    /// <summary>
    /// Sage ID Public Client Authentication Provider
    /// </summary>
    public class AuthenticationProvider
    {
        /// <summary>
        /// Path to SageID local application storage
        /// </summary>
        private static string _SageIDStorage;

        /// <summary>
        /// Domain of SageID service
        /// </summary>
        private readonly static string _configDomain;

        /// <summary>
        /// SageID client ID
        /// This is set once by your application 
        /// and used for all subsequent calls
        /// </summary>
        private string _clientID;

        /// <summary>
        /// SageID audience
        /// This is set once by your application 
        /// and used for all subsequent calls
        /// </summary>
        private string _audience;

        /// <summary>
        /// SageID domain
        /// This is set once by your application 
        /// and used for all subsequent calls
        /// </summary>
        private string _domain;

        /// <summary>
        /// The IsSilent flag is used to control behaviour on first logon of the app.
        /// If IsSilent is false: you will be prompted every time you start the app.
        /// If IsSilent is true: you will be prompted only when Refresh token has expired.
        /// </summary>
        private bool _isSilent;

        /// <summary>
        /// The timeout for logon
        /// </summary>
        private readonly static TimeSpan _logonTimeout = new TimeSpan(0, 3, 0);     // 3 minutes

        private readonly static InMemoryOAuthAccessTokenStorage _accessTokenStorageOverride;
        private readonly static PersistentOAuthRefreshTokenStorage _refreshTokenStorageOverride;

        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Static Constuctor that gets path to SageID storage
        /// </summary>
        static AuthenticationProvider()
        {
            _SageIDStorage = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            if (string.IsNullOrEmpty(_SageIDStorage) == false)
            {
                _SageIDStorage = System.IO.Path.Combine(_SageIDStorage, @"APISamples\APISampleWinFormsApp");
            }

            _accessTokenStorageOverride = new InMemoryOAuthAccessTokenStorage();
            _refreshTokenStorageOverride = new PersistentOAuthRefreshTokenStorage(_SageIDStorage);

            _configDomain = System.Configuration.ConfigurationManager.AppSettings["sci:Domain"];
            if (string.IsNullOrEmpty(_configDomain))
            {
                _configDomain = "id.sage.com";
            }

            if (string.IsNullOrEmpty(_configDomain) == false)
            {
                _configDomain = _configDomain.TrimEnd(new char[] { '/' });
            }

            if (string.IsNullOrEmpty(_configDomain) == false)
            {
                _configDomain = _configDomain.TrimEnd(new char[] { '/' });
            }
        }

        public string Domain
        {
            get
            {
                if (string.IsNullOrEmpty(_domain))
                {
                    _domain = _configDomain;
                }
                return _domain;
            }
            set
            {
                _domain = value;
            }
        }

        /// <summary>
        /// Sage ID Client ID
        /// </summary>
        public string ClientID
        {
            get
            {
                return _clientID;
            }
            set
            {
                _clientID = value;
            }
        }

        public string Scope
        {
            get
            {
                // Standard OpenID scopes. 
                // Must include email to allow migration from SageID to CloudID.
                return "openid token access_token offline_access email";
            }
            set
            {
                // ignore: but keep for backwards compatibility
                //_scope = value;
            }
        }

        /// <summary>
        /// Sage ID Audience
        /// </summary>
        public string Audience
        {
            get
            {
                return _audience;
            }
            set
            {
                _audience = value;
            }
        }

        /// <summary>
        /// Sage ID Is Silent logon
        /// </summary>
        /// <remarks>
        /// The IsSilent flag is used to control behaviour on first logon of the app.
        /// If IsSilent is false: you will be prompted every time you start the app.
        /// If IsSilent is true: you will be prompted only when Refresh token has expired.
        /// </remarks>
        public bool IsSilent
        {
            get
            {
                return _isSilent;
            }
            set
            {
                _isSilent = value;
            }
        }

        protected string StoragePartition
        {
            get
            {
                // by default token shared
                return string.Empty;
            }
        }


        /// <summary>
        /// Get security token
        /// </summary>
        /// <returns>The security token</returns>
        /// <remarks>
        /// Called whenever client makes WCF call.
        /// It’s important to make this call on every request: to ensure that you have a new and valid Access Token.
        /// If the access token has expired: it will silently get you a new access token by making use of the Refresh token.
        /// If the Refresh token has also expired, it will then prompt user for sign in. 
        /// </remarks>
        public string GetToken()
        {
            return Logon(false);
        }

        /// <summary>
        /// Logon
        /// </summary>
        /// <remarks>
        /// Called during client startup.
        /// </remarks>
        public void Logon()
        {
            Logon(IsSilent == false);
        }

        /// <summary>
        /// Logon
        /// </summary>
        /// <param name="resetDuration">
        /// If resetDuration is true: you will be prompted every time you start the app.
        /// If resetDuration is false: you will be prompted only when Refresh token has expired.
        /// </param>
        /// <returns></returns>
        private string Logon(bool resetDuration)
        {
            string token = string.Empty;

            // Check that ClientID and Audience properties have been set
            // These are required so if not set an Authentication exception is thrown
            if (string.IsNullOrEmpty(ClientID) || string.IsNullOrEmpty(Audience))
            {
                throw new AuthenticationException("Must provide ClientID and Audience");
            }

            try
            {
                // Mutex to restrict access to SageID Client library settings held in Isolated Storage
                using (SageIDClientMutex mutex = new SageIDClientMutex())
                {
                    token = LogonInner(resetDuration);
                }
            }
            catch (AggregateException agg)
            {
                Exception ex = null;

                if (agg.InnerException != null)
                {
                    ex = agg.InnerException;
                }
                else if (agg.InnerExceptions != null)
                {
                    ex = agg.InnerExceptions[0];
                }

                if (ex == null)
                    ex = agg;

                throw ex;
            }

            return token;
        }

        private Token GetTokenSilently(OAuthLowLevel oAuth)
        {
            Token token = null;

            token = oAuth.RetrieveExistingAccessToken(ClientID, Scope, Audience, StoragePartition);

            if (token == null)
            {
                Token refreshToken = oAuth.RetrieveExistingRefreshToken(ClientID, Scope, Audience, StoragePartition);
                if (refreshToken != null)
                {
                    Auth0ClientBase client = CreateAuth0Client();

                    AuthorisationResult result = Task.Run(() => oAuth.RefreshAccessTokenAsync(client, refreshToken, ClientID, Scope, Audience, StoragePartition)).Result;

                    token = result.AccessToken;
                }
            }

            return token;
        }

        private Token GetTokenWithPrompt(OAuthLowLevel oAuth, OAuthOptions options)
        {
            Token token = null;

            using (DefaultWebBrowserOverride webBrowserOverride = new DefaultWebBrowserOverride(new AuthorisationResultUrlChecker()))
            {
                _cancellationTokenSource = new CancellationTokenSource();

                Auth0ClientBase client = CreateAuth0Client();

                var startResult = Task.Run(() => oAuth.BeginAuthorisationAsync(client, Audience, options)).Result;

                string endUrl = Task.Run(() => webBrowserOverride.NavigateToStartUrlAsync(new Uri(startResult.StartUrl), _cancellationTokenSource.Token)).Result;

                var endResult = Task.Run(() => oAuth.EndAuthorisationAsync(client, startResult, endUrl, ClientID, Scope, Audience, StoragePartition)).Result;

                token = endResult.AccessToken;
            }

            return token;
        }


        protected string LogonInner(bool resetDuration)
        {
            string ret = string.Empty;
            Token token = null;

            bool retry = false;

            OAuthLowLevel oAuth = new OAuthLowLevel(LoggingOverride, _accessTokenStorageOverride, _refreshTokenStorageOverride);

            do
            {
                retry = false;

                // Start a stopwatch to check if time elapsed has exceeded configured timeout
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                try
                {
                    try
                    {
                        if (resetDuration == false)
                        {
                            token = GetTokenSilently(oAuth);
                        }

                        if (token == null)
                        {
                            OAuthOptions options = new OAuthOptions();
                            options.ForceUserInteraction = resetDuration;
                            token = GetTokenWithPrompt(oAuth, options);
                        }
                    }
                    catch (AggregateException agg)
                    {
                        HandleAggregateException(agg);
                    }
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    throw new AuthenticationCancelException();
                }
                catch (Exception ex)
                {
                    if (stopwatch.Elapsed > _logonTimeout)
                    {
                        retry = true;
                    }
                    else
                    {
                        throw new AuthenticationException(ex.Message);
                    }
                }
            } while (retry);

            // Return the access token
            if (token != null)
            {
                ret = token.RawToken.FromSecureString();
            }

            return ret;
        }

        /// <summary>
        /// Logoff
        /// </summary>
        /// Called during client shutdown.
        public void Logoff()
        {
        }

        protected void HandleAggregateException(AggregateException agg)
        {
            Exception ex = null;

            if (agg.InnerException != null)
            {
                ex = agg.InnerException;
            }
            else if (agg.InnerExceptions != null)
            {
                ex = agg.InnerExceptions[0];
            }

            if (ex == null)
                ex = agg;

            throw ex;
        }

        protected virtual Exception ConvertCancelledException()
        {
            return new AuthenticationCancelException();
        }

        private Auth0ClientBase CreateAuth0Client()
        {
            Auth0ClientOptions options = new Auth0ClientOptions();
            options.Domain = Domain;
            options.ClientId = ClientID;
            options.Scope = Scope;

            SageAPIClient client = new SageAPIClient(options);

            return client;
        }

        private static void LoggingOverride(LogLevel logLevel, string message)
        {
            try
            {
                switch (logLevel)
                {
                    case LogLevel.Warning:
                        System.Diagnostics.Trace.TraceWarning(message);
                        break;

                    case LogLevel.Error:
                        System.Diagnostics.Trace.TraceError(message);
                        break;

                    default:
                        System.Diagnostics.Trace.TraceInformation(message);
                        break;
                }
            }
            catch
            {
            }
        }
    }

    public class SageAPIClient : Auth0ClientBase
    {
        public SageAPIClient(Auth0ClientOptions options)
            : base(options, "SageAPIWPF")
        {
        }
    }
}
