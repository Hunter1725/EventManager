using Contract.Repositories;
using Contract.Services;
using Domain.Interfaces.Services;
using EventManagerAPI.Services;
using Infrastructure.Repository;
using Infrastructure.Service;
using Infrastructure.UnitOfWork;

namespace EventManagerAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureUnitOfWork(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IWeatherRepository, WeatherRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        public static void ConfigureService(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IWeatherService, WeatherService>();
            services.AddScoped<IOpenWeatherService, OpenWeatherService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IBackgroundService, Infrastructure.Service.BackgroundService>();
            services.AddScoped<IHashService, HashService>();
            services.AddScoped<ICacheService, CacheService>();
        }
    }
}
