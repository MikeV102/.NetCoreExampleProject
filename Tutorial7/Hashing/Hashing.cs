using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Tutorial5.Hashing
{
    public class Hashing
    {
        
       
        public static string Create(string value, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2(value, Encoding.UTF8.GetBytes(salt), KeyDerivationPrf.HMACSHA512,
                10000, 256 / 8);
            
            return Convert.ToBase64String(valueBytes);
        }

        public static bool Validate(string value, string salt, string hash) => Create(value, salt) == hash;


        public static string CreateSalt()
        {
            byte[] randomBytes = new byte[128 / 8];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }

    }
}
