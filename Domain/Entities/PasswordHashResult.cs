using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class PasswordHashResult
    {
        public string Salt { get; set; }
        public string HashString { get; set; }
    }
}
