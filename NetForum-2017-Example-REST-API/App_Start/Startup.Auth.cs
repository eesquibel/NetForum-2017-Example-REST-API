using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Owin;

namespace NetForumRestExample
{
    public partial class Startup
    {
        static public string issuer;
        static public string audience;
        static public byte[] secret;

        private void ConfigureAuth(IAppBuilder app)
        {
            // This should all really be loaded from a configuration file
            issuer = "https://nf-eesquibel.mshome.net/Example/"; // arbitrary URI
            audience = "https://nf-eesquibel.mshome.net/Example/"; // arbitrary string or Uri that identifies this specific resource
            secret = TextEncodings.Base64Url.Decode("p/MLeU433JN5pOYcqaRleFRK2OX6nSMnXMUsEDlkle35lUrEqAuF4iFp8EWfCZ1R"); // 48 random bytes

            app.UseJwtBearerAuthentication
            (
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    AllowedAudiences = new[] { audience },
                    IssuerSecurityKeyProviders = new IIssuerSecurityKeyProvider[]
                    {
                        new SymmetricKeyIssuerSecurityKeyProvider(issuer, secret)
                    }
                }
            );
        }
    }
}
