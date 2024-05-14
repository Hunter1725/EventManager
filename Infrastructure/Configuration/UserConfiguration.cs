using Domain.Entities;
using Infrastructure.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            var hashPassword = HashPassword("admin");
            builder.HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    SaltValue = hashPassword.Salt,
                    HashValue = hashPassword.HashString,
                    Email = "admin@gmail.com",
                    Role = "Admin"
                }
            );

            // Configure unique constraint for Username
            builder.HasIndex(u => u.Username)
                .IsUnique();

            // Configure unique constraint for Email
            builder.HasIndex(u => u.Email)
                .IsUnique();
        }

        private PasswordHashResult HashPassword(string password)
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
    }
}
