using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SimpleSchedulerBusiness;
using SimpleSchedulerData;

namespace SimpleSchedulerAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization(config =>
            {
                config.AddPolicy("ValidUser", policy =>
                {
                    policy.RequireAssertion(context =>
                    {
                        return context?.User?.Claims?.Any(claim => claim.Type == "IsAuthenticated" && claim.Value == "1") == true;
                    });
                });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin());
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SimpleSchedulerAPI", Version = "v1" });
            });

            services.AddMemoryCache();
            services.AddScoped<IDatabase, SqlDatabase>();
            services.AddScoped<IDatabaseFactory, DatabaseFactory>();
            services.AddScoped<IWorkerManager, WorkerManager>();
            services.AddScoped<IScheduleManager, ScheduleManager>();
            services.AddScoped<IJobManager, JobManager>();
            services.AddScoped<IUserManager, UserManager>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SimpleSchedulerAPI v1"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
