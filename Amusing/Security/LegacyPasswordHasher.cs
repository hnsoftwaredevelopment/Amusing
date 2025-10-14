using System;
using System.Security.Cryptography;
using System.Text;

using Amusing.Data;

using Microsoft.AspNetCore.Identity;

namespace Amusing.Security
{
    public class LegacyPasswordHasher : IPasswordHasher<ApplicationUser>
    {
        public string HashPassword( ApplicationUser user, string password )
        {
            // Voor nieuwe gebruikers gebruiken we Identity hash
            return new PasswordHasher<ApplicationUser>().HashPassword( user, password );
        }

        public PasswordVerificationResult VerifyHashedPassword( ApplicationUser user, string hashedPassword, string providedPassword )
        {
            // Check legacy MD5
            using var md5 = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(providedPassword);
            var hashBytes = md5.ComputeHash(inputBytes);
            var md5Hash = Convert.ToHexStringLower( hashBytes );

            if ( md5Hash == user.LegacyPassword.ToLowerInvariant() )
            {
                // Legacy correct, schrijf Identity hash naar PasswordHash (niet LegacyPassword!)
                user.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword( user, providedPassword );
                return PasswordVerificationResult.SuccessRehashNeeded;
            }

            // Check Identity hash
            return new PasswordHasher<ApplicationUser>().VerifyHashedPassword( user, hashedPassword, providedPassword );
        }
    }
}
