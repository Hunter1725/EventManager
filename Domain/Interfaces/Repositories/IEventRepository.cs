using Domain.DTOs.EventDTO;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories
{
    public interface IEventRepository : IRepositoryBase<Event>
    {
        IEnumerable<Event> GetEvent();
        IEnumerable<Event> GetEventByUserId(int id);
        Event GetEventById(int id);
    }
}
