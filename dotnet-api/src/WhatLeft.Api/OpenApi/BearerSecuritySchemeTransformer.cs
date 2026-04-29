using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WhatLeft.Api.OpenApi;

/// <summary>
/// Declares the Auth0 OAuth2 Authorization Code + PKCE scheme in the OpenAPI document
/// and marks every protected operation as requiring it.
/// </summary>
internal sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider,
    IConfiguration configuration) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var schemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        if (!schemes.Any(s => s.Name == JwtBearerDefaults.AuthenticationScheme))
            return;

        var domain = configuration["Auth0:Domain"]!;
        var audience = configuration["Auth0:Audience"]!;

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Auth0"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        // Auth0 requires the audience param to issue a JWT scoped to our API
                        AuthorizationUrl = new Uri($"https://{domain}/authorize?audience={Uri.EscapeDataString(audience)}"),
                        TokenUrl = new Uri($"https://{domain}/oauth/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            ["openid"]  = "OpenID Connect",
                            ["profile"] = "User profile",
                            ["email"]   = "User email"
                        }
                    }
                }
            }
        };

        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
        {
            operation.Value.Security ??= [];
            operation.Value.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Auth0", document)] = ["openid", "profile", "email"]
            });
        }
    }
}
