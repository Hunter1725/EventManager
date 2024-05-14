using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Repository;
using Contract.Repositories;

namespace Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private EventContext _context;
        private IEventRepository _eventRepository;
        private IUserRepository _userRepository;
        private IWeatherRepository _weatherRepository;

        public UnitOfWork(EventContext context)
        {
            _context = context;
        }

        public IEventRepository EventRepository
        {
            get
            {
                if (_eventRepository == null)
                {
                    _eventRepository = new EventRepository(_context);
                }
                return _eventRepository;
            }
        }

        public IUserRepository UserRepository
        {
            get
            {
                if (_userRepository == null)
                {
                    _userRepository = new UserRepository(_context);
                }
                return _userRepository;
            }
        }

        public IWeatherRepository WeatherRepository
        {
            get
            {
                if (_weatherRepository == null)
                {
                    _weatherRepository = new WeatherRepository(_context);
                }
                return _weatherRepository;
            }
        }

        //Save changes async
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
