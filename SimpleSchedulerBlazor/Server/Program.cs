using Microsoft.AspNetCore.Authentication.JwtBearer;
using SimpleSchedulerBlazor.Server.Auth;
using Microsoft.IdentityModel.Tokens;
using SimpleSchedulerConfiguration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

ApplicationConfiguration.SetUpConfiguration(builder.Configuration);
ApplicationConfiguration.SetUpAppSettings(builder.Configuration, builder.Services);

builder.Services.AddSingleton<ITokenService, TokenService>();

ApplicationConfiguration.SetUpDatabase(builder.Services);
if (appSettings.Database.IsSqlServer)
{
    builder.Services.AddScoped<BaseDatabase, SqlDatabase>();
    builder.Services.AddScoped<IWorkerManager, SimpleSchedulerBusiness.SqlServer.WorkerManager>();
    builder.Services.AddScoped<IScheduleManager, SimpleSchedulerBusiness.SqlServer.ScheduleManager>();
    builder.Services.AddScoped<IJobManager, SimpleSchedulerBusiness.SqlServer.JobManager>();
    builder.Services.AddScoped<IUserManager, SimpleSchedulerBusiness.SqlServer.UserManager>();
}
else if (appSettings.Database.IsSqlite)
{
    builder.Services.AddScoped<BaseDatabase, SqliteDatabase>();
    builder.Services.AddScoped<IWorkerManager, SimpleSchedulerBusiness.Sqlite.WorkerManager>();
    builder.Services.AddScoped<IScheduleManager, SimpleSchedulerBusiness.Sqlite.ScheduleManager>();
    builder.Services.AddScoped<IJobManager, SimpleSchedulerBusiness.Sqlite.JobManager>();
    builder.Services.AddScoped<IUserManager, SimpleSchedulerBusiness.Sqlite.UserManager>();
}

builder.Services.AddScoped<DatabaseFactory>();
builder.Services.AddScoped<IEmailer, Emailer>();

builder.Services.AddCors();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = appSettings.Jwt.Issuer,
        ValidAudience = appSettings.Jwt.Issuer,
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromHexString(appSettings.Jwt.Key))
    };
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();



/*
 using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SimpleSchedulerBusiness;
using SimpleSchedulerBusiness.SqlServer;
using SimpleSchedulerBusiness.Sqlite;
using SimpleSchedulerData;
using SimpleSchedulerEmail;

namespace SimpleSchedulerAPI;

public class Startup
{
    public Startup(IConfiguration configuration) => Configuration = configuration;

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options => options.AddPolicy("Cors", builder =>
        {
            builder
                //.AllowAnyOrigin()
                .WithOrigins(Configuration["WebUrl"])
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }));

        var cookieAuthSection = Configuration.GetSection("CookieAuth");
        string appName = cookieAuthSection.GetValue<string>("ApplicationName");
        string keyLocation = cookieAuthSection.GetValue<string>("KeyLocation");
        services.AddDataProtection()
            .SetApplicationName(appName)
            //.SetDefaultKeyLifetime(TimeSpan.FromDays(9999))
            .PersistKeysToFileSystem(new DirectoryInfo(keyLocation));

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, config =>
            {
                config.Cookie.Name = cookieAuthSection.GetValue<string>("UserCookieName");
                    //config.Cookie.SameSite = SameSiteMode.Strict;
                    config.Cookie.SameSite = SameSiteMode.None;
                config.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = redirectContext =>
                    {
                        redirectContext.HttpContext.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(config =>
        {
                // Only one policy - you must be a valid user. Valid user can see all pages.
                config.AddPolicy("ValidUser", policy => policy.RequireClaim(ClaimTypes.Email));
        });

        services.AddControllers();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "SimpleSchedulerAPI", Version = "v1" });
        });

        switch (Configuration["DatabaseType"])
        {
            case "SqlServer":
                services.AddScoped<BaseDatabase, SqlDatabase>();
                services.AddScoped<IWorkerManager, SimpleSchedulerBusiness.SqlServer.WorkerManager>();
                services.AddScoped<IScheduleManager, SimpleSchedulerBusiness.SqlServer.ScheduleManager>();
                services.AddScoped<IJobManager, SimpleSchedulerBusiness.SqlServer.JobManager>();
                services.AddScoped<IUserManager, SimpleSchedulerBusiness.SqlServer.UserManager>();
                break;
            case "Sqlite":
                services.AddScoped<BaseDatabase, SqliteDatabase>();
                services.AddScoped<IWorkerManager, SimpleSchedulerBusiness.Sqlite.WorkerManager>();
                services.AddScoped<IScheduleManager, SimpleSchedulerBusiness.Sqlite.ScheduleManager>();
                services.AddScoped<IJobManager, SimpleSchedulerBusiness.Sqlite.JobManager>();
                services.AddScoped<IUserManager, SimpleSchedulerBusiness.Sqlite.UserManager>();
                break;
        }
        services.AddScoped<DatabaseFactory>();
        services.AddScoped<IEmailer, Emailer>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
        IServiceProvider serviceProvider)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SimpleSchedulerAPI v1"));
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("Cors");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}

 */