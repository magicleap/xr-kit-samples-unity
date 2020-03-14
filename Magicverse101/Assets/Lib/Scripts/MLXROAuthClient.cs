// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using IdentityModel.Jwk;
using IdentityModel.OidcClient;

public class MLXROAuthClient : Assets.UnityAuthClient
{
    public override OidcClient CreateAuthClient()
    {
        string uriScheme = "";
        string uriHost = "";
        string clientId = "";

        var options = new OidcClientOptions()
        {
            Authority = "https://oauth.magicleap.com",
            ClientId = clientId,
            Scope = "openid offline_access",
            // HACK: remove profile. external credentials will not have the "profile" scope provided (where it gives you back your name once you sign in)  for security reasons
            // Scope = "openid profile offline_access",
            // Redirect (reply) uri is specified in the AndroidManifest and code for handling
            // it is in the associated AndroidUnityPlugin project, and OAuthUnityAppController.mm.
            RedirectUri = $"{uriScheme}://{uriHost}",
            ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect,
            Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
            Browser = Browser,
        };

        // TODO: MLID endpoints for validation do not exist yet so we make up the discovery data
        // and disable a bunch of verification steps.
        var info = new ProviderInformation();
        info.AuthorizeEndpoint = $"{options.Authority}/auth";
        info.IssuerName = "https://auth.magicleap.com";
        info.EndSessionEndpoint = $"{options.Authority}/logout";
        info.TokenEndpoint = $"{options.Authority}/token";
        info.TokenEndPointAuthenticationMethods = new List<string>
        {
            "client_secret_basic",
            "client_secret_post"
        };

        Stream stream = WebRequest.CreateHttp("https://auth.magicleap.com/.well-known/jwks.json").GetResponse().GetResponseStream();
        string json = new StreamReader(stream, Encoding.UTF8).ReadToEnd();
        info.KeySet = new JsonWebKeySet(json);

        options.PostLogoutRedirectUri = options.RedirectUri;
        options.LoggerFactory.AddProvider(new Assets.UnityAuthLoggerProvider());
        options.RefreshDiscoveryDocumentForLogin = false;
        options.ProviderInformation = info;
        options.LoadProfile = false;
        options.Policy.RequireIdentityTokenSignature = false;
        options.Policy.Discovery.ValidateIssuerName = false;
        options.Policy.RequireAccessTokenHash = false;
        options.Policy.RequireAuthorizationCodeHash = false;
        options.Policy.RequireIdentityTokenOnRefreshTokenResponse = false;

        return new OidcClient(options);
    }
}
