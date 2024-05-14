using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface ICacheService
    {
        bool TryGetValue<TItem>(object key, out TItem value);
        void Set<TItem>(object key, TItem value, MemoryCacheEntryOptions options);
    }

}
