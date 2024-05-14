using Infrastructure.Repository;
using Contract.Repositories;

namespace Infrastructure.UnitOfWork
{
    public interface IUnitOfWork
    {
        IEventRepository EventRepository { get; }
        IUserRepository UserRepository { get; }
        IWeatherRepository WeatherRepository { get; }

        Task SaveChangesAsync();
    }
}