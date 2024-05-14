using Domain.Entities;
using Contract.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.DTOs.EventDTO;
using Infrastructure.Extensions;
using System.Collections;
using System.Linq.Expressions;

namespace Infrastructure.Repository
{
    public class EventRepository : RepositoryBase<Event>, IEventRepository
    {
        public EventRepository(EventContext context) : base(context)
        {
        }

        public IEnumerable<Event> GetEvent()
        {
            return _context.Events
                .Include(x => x.WeatherInfos)
                .Include(x => x.User);
        }

        public IEnumerable<Event> GetEventByUserId(int id)
        {
            return _context.Events
                .Where(x => x.UserId == id)
                .Include(x => x.WeatherInfos)
                .Include(x => x.User);
        }

        public Event GetEventById(int id)
        {
            return _context.Events
                .Where(x => x.Id == id)
                .Include(x => x.WeatherInfos)
                .Include(x => x.User)
                .FirstOrDefault();
        }
    }
}
