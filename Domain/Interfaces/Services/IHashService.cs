using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface IHashService
    {
        public PasswordHashResult HashPassword(string password);
        public string HashPassword(string password, string? saltValue);
        public string GenerateOTP();
    }
}
