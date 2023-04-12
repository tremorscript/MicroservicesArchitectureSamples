using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace DuendeIdentityServer6;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
                new ApiScope("sampleapi2"),
                new ApiScope("sampleapi1"),
                new ApiScope("samplebffwebapp"),
        };

    public static IEnumerable<Client> GetClients(IConfiguration configuration)
    {
        return new Client[]
        {
                new Client
                {
                    ClientId = "samplewebapp",
                    ClientName = "SampleWebAppClient",
                    ClientSecrets = new List<Secret>
                    {

                        new Secret("secret".Sha256())
                    },
                    ClientUri = $"{configuration["SampleWebAppClient"]}",                             // public uri of the client
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowAccessTokensViaBrowser = false,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequirePkce = false,
                    RedirectUris = new List<string>
                    {
                        $"{configuration["SampleWebAppClient"]}/signin-oidc"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        $"{configuration["SampleWebAppClient"]}/signout-callback-oidc"
                    },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "sampleapi2",
                        "sampleapi1",
                    },
                    AccessTokenLifetime = 60*60*2, // 2 hours
                    IdentityTokenLifetime= 60*60*2 // 2 hours
                },
                new Client
                {
                    ClientId = "sampleapi1swaggerui",
                    ClientName = "SampleApi1 Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = { $"{configuration["SampleApi1Client"]}/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris = { $"{configuration["SampleApi1Client"]}/swagger/" },
                    AllowedScopes =
                    {
                        "sampleapi1"
                    }
                },
                new Client
                {
                    ClientId = "sampleapi2swaggerui",
                    ClientName = "SampleApi2 Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = { $"{configuration["SampleApi2Client"]}/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris = { $"{configuration["SampleApi2Client"]}/swagger/" },
                    AllowedScopes =
                    {
                        "sampleapi2"
                    }
                },
                new Client
                {
                    ClientId = "samplebffswaggerui",
                    ClientName = "SampleBff Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = { $"{configuration["SampleBffClient"]}/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris = { $"{configuration["SampleBffClient"]}/swagger/" },
                    AllowedScopes =
                    {
                        "samplebffwebapp"
                    }
                }
            };
    }
}
