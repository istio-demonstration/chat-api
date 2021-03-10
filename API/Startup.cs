using API.Extensions;
using API.Middleware;
using API.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace API
{
  public class Startup
  {
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
      _configuration = configuration;

    }


    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {

      services.AddControllers();
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
      });
      services.AddApplicationServices(_configuration);
      services.AddIdentityService(_configuration);
      services.AddSignalR();

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    { 
        app.UseMiddleware<ExceptionMiddleware>();
      if (env.IsDevelopment())
      {
        //app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
      }

      app.UseHttpsRedirection();

      app.UseRouting();
      app.UseCors(options => {
          options.AllowAnyHeader().AllowCredentials().AllowAnyMethod().WithOrigins("http://localhost:4200");
      });
      app.UseAuthentication();
      app.UseAuthorization();
    
      app.UseDefaultFiles();
      app.UseStaticFiles();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        endpoints.MapHub<PresenceHub>("hubs/presence");
        endpoints.MapHub<MessageHub>("hubs/message");
        endpoints.MapFallbackToController("Index", "Fallback");
      });
    }
  }
}
