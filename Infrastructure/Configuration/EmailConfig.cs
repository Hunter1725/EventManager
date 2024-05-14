using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configuration
{
    public class EmailConfig
    {
        public string EmailForSend { get; set; }
        public string AppPasswordConfiguration { get; set; }
        public string ServiceId { get; set; }
        public string TemplateId { get; set; }
        public string UserId { get; set; }
        public string AccessToken { get; set; }
        public string Url { get; set; }
    }
}
