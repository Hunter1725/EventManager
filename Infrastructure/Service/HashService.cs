using Domain.Entities;
using Domain.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class HashService : IHashService
    {
        public PasswordHashResult HashPassword(string password)
        {
            // Using SHA256 to hash the password
            using (SHA256 sha256 = SHA256.Create())
            {
                // Generate a random salt value
                byte[] randomByte = new byte[15];
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomByte);
                }
                IEnumerable<string> saltHex = randomByte.Select(x => x.ToString("x2"));
                string salt = string.Join("", saltHex);

                // Combine the password and salt value
                string passwordWithSalt = password + salt;

                // Hash the password with salt value
                byte[] hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));

                string hashString = Convert.ToBase64String(hashValue);

                return new PasswordHashResult { Salt = salt, HashString = hashString };
            }
        }

        public string HashPassword(string password, string? saltValue)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string passwordWithSalt = password + saltValue;

                byte[] hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));

                string hashString = Convert.ToBase64String(hashValue);

                return hashString;
            }
        }

        public string GenerateOTP()
        {
            // Tạo chuỗi ngẫu nhiên có 6 ký tự
            Random random = new Random();
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, 6)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
