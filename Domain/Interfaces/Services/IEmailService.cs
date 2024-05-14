using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string receiver, string subject, string body);
        Task SendEmailWith3rdEmailJs(string receiver, string nameOfReceiver, string body);
    }
}
