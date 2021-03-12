using API.Data;
using API.Helper;
using API.Interfaces;
using API.Services;
using API.SignalR;
using AutoMapper;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<DataContext>(options =>
            {  //GetConnectionString
                options.UseSqlite(configuration["ConnectionStrings:DefaultConnection"]);
            });

            services.AddCors();
            services.AddScoped<ITokenService, TokenService>();
            // auto mapper
            services.AddAutoMapper(typeof(AutoMapperProfiles));
            // mapster
            services.AddSingleton(MapsterProfile.GetConfiguredMappingConfig());
            services.AddScoped<MapsterMapper.IMapper, ServiceMapper>();

            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
            services.AddScoped<IPhotoService, PhotoService>();

            services.AddScoped<LogUserActivity>();

            //services.AddScoped<IUserRepository, UserRepository>();
            //services.AddScoped<ILikeRepository, LikeRepository>();
            //services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<PresenceTracker>();
            services.AddHeaderPropagation(options =>
            {
                // for tracing
                options.Headers.Add("x-request-id");
                options.Headers.Add("x-b3-traceid");
                options.Headers.Add("x-b3-spanid");
                options.Headers.Add("x-b3-parentspanid");
                options.Headers.Add("x-b3-sampled");
                options.Headers.Add("x-b3-flags");
                options.Headers.Add("x-ot-span-context");

            });
            return services;
        }
    }
}
