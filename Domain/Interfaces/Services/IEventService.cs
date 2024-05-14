using Domain.DTOs.EventDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Services
{
    public interface IEventService
    {
        Task CreateEvent(EventForCreate eventForCreate, int id);
        IEnumerable<EventForRead> GetEvents();
        IEnumerable<EventForRead> GetEventsByUserId(int userId);
        Task<EventForRead> GetEventById(int id);
        Task UpdateEvent(int id, EventForUpdate eventForUpdate);
        Task DeleteEvent(int id);
    }
}
