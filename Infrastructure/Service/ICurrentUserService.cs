using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public interface ICurrentUserService
    {
        //get current user
        string GetCurrentUser();
    }
}
