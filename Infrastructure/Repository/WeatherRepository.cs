using Domain.Entities;
using Contract.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class WeatherRepository : RepositoryBase<Weather>, IWeatherRepository
    {
        public WeatherRepository(EventContext context) : base(context)
        {
        }
    }
}
