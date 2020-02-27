using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using IdentityModel.OidcClient.Infrastructure;
using IdentityModel.OidcClient.Results;
using UnityEngine;

namespace Assets
{
    public class UnityAuthClient
    {
        private OidcClient _client = null;
        public string AccessToken { get; private set; } = null;
        public string IdentityToken { get; private set; } = null;
        public string RefreshToken { get; private set; } = null;
        public string UserName { get; private set; } = "";
        public DateTime AccessTokenExpiration { get; private set; }

        public string GetClientId() => _client.Options.ClientId;

        private const string refreshKey = "refreshKey";
        private const string nameKey = "nameKey";

        public UnityAuthClient()
        {
            // We must disable the IdentityModel log serializer to avoid Json serialize exceptions on IOS.
#if UNITY_IOS
            LogSerializer.Enabled = false;
#endif

            // On Android, we use Chrome custom tabs to achieve single-sign on.
            // On Ios, we use SFSafariViewController to achieve single-sign-on.
            // See: https://www.youtube.com/watch?v=DdQTXrk6YTk
            // And for unity integration, see: https://qiita.com/lucifuges/items/b17d602417a9a249689f (Google translate to English!)
#if UNITY_ANDROID
            Browser = new AndroidChromeCustomTabBrowser();
#elif UNITY_IOS
            Browser = new SFSafariViewBrowser();
#endif
            CertificateHandler.Initialize();
            KeyChain.Initialize();

            string storedKey = KeyChain.GetString(refreshKey);
            if (storedKey.Length > 0)
            {
                RefreshToken = storedKey;
            }

            storedKey = KeyChain.GetString(nameKey);
            if (storedKey.Length > 0)
            {
                UserName = storedKey;
            }
        }

        // Instead of using AppAuth, which is not available for Unity apps, we are using
        // this library: https://github.com/IdentityModel/IdentityModel.OidcClient2
        // .Net 4.5.2 binaries have been built from the above project and included in
        // /Assets/Plugins folder.
        public virtual OidcClient CreateAuthClient()
        {
            var options = new OidcClientOptions()
            {
                Authority = "https://demo.identityserver.io/",
                ClientId = "native.code",
                Scope = "openid profile email",
                // Redirect (reply) uri is specified in the AndroidManifest and code for handling
                // it is in the associated AndroidUnityPlugin project, and OAuthUnityAppController.mm.
                RedirectUri = "io.identitymodel.native://callback",
                PostLogoutRedirectUri = "io.identitymodel.native://callback",
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect,
                Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                Browser = Browser,
            };

            options.LoggerFactory.AddProvider(new UnityAuthLoggerProvider());
            return new OidcClient(options);
        }

        private void EnsureAuthClient()
        {
            if (_client == null)
            {
                _client = CreateAuthClient();
            }
        }

        public async Task<bool> LoginAsync()
        {
            EnsureAuthClient();
            LoginResult result = null;
            try
            {
                result = await _client.LoginAsync(new LoginRequest());
            }
            catch (Exception e)
            {
                Debug.Log("UnityAuthClient::Exception during login: " + e.Message);
                return false;
            }
            finally
            {
                Debug.Log("UnityAuthClient::Dismissing sign-in browser.");
                Browser.Dismiss();
            }

            if (result.IsError)
            {
                Debug.Log("UnityAuthClient::Error authenticating: " + result.Error);
            }
            else
            {
                AccessToken = result.AccessToken;
                RefreshToken = result.RefreshToken;
                IdentityToken = result.IdentityToken;
                if (result.User.Identity.Name != null)
                {
                    UserName = result.User.Identity.Name;
                }
                AccessTokenExpiration = result.AccessTokenExpiration;

                KeyChain.SetString(refreshKey, RefreshToken);
                KeyChain.SetString(nameKey, UserName);

                Debug.Log("UnityAuthClient::AccessToken: " + AccessToken);
                Debug.Log("UnityAuthClient::RefreshToken: " + RefreshToken);
                Debug.Log("UnityAuthClient::IdentityToken: " + IdentityToken);
                Debug.Log("UnityAuthClient::AccessTokenExpiration: " + AccessTokenExpiration);
                Debug.Log("UnityAuthClient::Signed in.");
                return true;
            }

            return false;
        }

        public async Task<bool> RefreshAsync()
        {
            // Check secure storage for refresh token.
            EnsureAuthClient();
            RefreshTokenResult refreshResult = null;
            try
            {
                refreshResult = await _client.RefreshTokenAsync(RefreshToken);
            }
            catch (Exception e)
            {
                Debug.Log("UnityAuthClient::Exception during refresh: " + e.Message);
                return false;
            }

            if (refreshResult.IsError)
            {
                Debug.Log("UnityAuthClient::Error authenticating: " + refreshResult.Error);
            }
            else
            {
                AccessToken = refreshResult.AccessToken;
                RefreshToken = refreshResult.RefreshToken;
                IdentityToken = refreshResult.IdentityToken;
                AccessTokenExpiration = DateTime.Now.AddSeconds(refreshResult.ExpiresIn);

                KeyChain.SetString(refreshKey, RefreshToken);

                Debug.Log("UnityAuthClient::AccessToken: " + AccessToken);
                Debug.Log("UnityAuthClient::RefreshToken: " + RefreshToken);
                Debug.Log("UnityAuthClient::IdentityToken: " + IdentityToken);
                Debug.Log("UnityAuthClient::AccessTokenExpiration: " + AccessTokenExpiration);
                Debug.Log("UnityAuthClient::Refreshed.");
                return true;
            }

            return false;
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                KeyChain.DeleteKey(refreshKey);
                KeyChain.DeleteKey(nameKey);

                await _client.LogoutAsync(new LogoutRequest() {
                    BrowserDisplayMode = DisplayMode.Hidden,
                    IdTokenHint = IdentityToken });
                Debug.Log("UnityAuthClient::Signed out successfully.");
                return true;
            }
            catch (Exception e)
            {
                Debug.Log("UnityAuthClient::Failed to sign out: " + e.Message);
            }
            finally
            {
                Debug.Log("UnityAuthClient::Dismissing sign-out browser.");
                Browser.Dismiss();
                _client = null;
            }

            return false;
        }

        public MobileBrowser Browser { get; }
    }
}
