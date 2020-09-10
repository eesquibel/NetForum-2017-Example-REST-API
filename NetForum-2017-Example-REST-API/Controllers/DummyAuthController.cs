using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Web.Http;

namespace NetForumRestExample.Controllers
{
    public class DummyAuthController : ApiController
    {
        /// <summary>
        /// Dummy authentication method to demonstrate how to make a JWT token.
        /// </summary>
        /// <remarks>
        /// This code is not intended for production use as it preforms no actual authentication
        /// </remarks>
        [AllowAnonymous][HttpPost]
        public IHttpActionResult Post()
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest();
            }

            // TODO: Implement real authenication

            // Really should pull these from somewhere more secure/configurable
            var securityKey = new SymmetricSecurityKey(Startup.secret);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Create a List of Claims, Keep claims name short
            var claims = new List<Claim>();

            // JWT specific
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // JWT ID
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, "eric@abmp.com"));
            claims.Add(new Claim(JwtRegisteredClaimNames.GivenName, "Eric"));
            claims.Add(new Claim(JwtRegisteredClaimNames.FamilyName, "Esquibel"));

            /* 
             * This is the trick to using security principals in your custom APIs.
             * 
             * Avectra.netForum.Common.Config.CurrentUserName is used to get the current user for
             * loading table and column permissions, but our ability to override it when there is
             * an HttpContext present is limited / non-existant. So we need to set our security
             * principal's name to be the string "unknown" to trick the system and work around
             * the security logic, and deal with the fact there is no Session.
             */

            // Needs to be a valid iWeb/xWeb user, or "unknown" if mimicking an eWeb API
            claims.Add(new Claim(ClaimTypes.Name, @"unknown"));

            // Based on our mock authenication method            
            claims.Add(new Claim(ClaimTypes.Role, "iWeb"));

            // Would load these from the database
            claims.Add(new Claim(ClaimTypes.Role, "netFORUMAdmin"));
            claims.Add(new Claim(ClaimTypes.Role, "netFORUMUser"));

            // Custom
            claims.Add(new Claim("usr_code", "eesquibel"));
            claims.Add(new Claim("usr_name", @"avhost\eesquibel"));
            claims.Add(new Claim("usr_key", Guid.Empty.ToString()));

            //Create Security Token object by giving required parameters    
            var jwt = new JwtSecurityToken
            (
                issuer: Startup.issuer,
                audience: Startup.audience,
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );
            
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return Json(new { data = token });
        }
    }
}
