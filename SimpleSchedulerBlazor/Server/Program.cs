using SimpleSchedulerBusiness;
using SimpleSchedulerData;
using SimpleSchedulerEmail;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("secrets.json", optional: true);

switch (builder.Configuration["DatabaseType"])
{
    case "SqlServer":
        builder.Services.AddScoped<BaseDatabase, SqlDatabase>();
        builder.Services.AddScoped<IWorkerManager, SimpleSchedulerBusiness.SqlServer.WorkerManager>();
        builder.Services.AddScoped<IScheduleManager, SimpleSchedulerBusiness.SqlServer.ScheduleManager>();
        builder.Services.AddScoped<IJobManager, SimpleSchedulerBusiness.SqlServer.JobManager>();
        builder.Services.AddScoped<IUserManager, SimpleSchedulerBusiness.SqlServer.UserManager>();
        break;
    case "Sqlite":
        builder.Services.AddScoped<BaseDatabase, SqliteDatabase>();
        builder.Services.AddScoped<IWorkerManager, SimpleSchedulerBusiness.Sqlite.WorkerManager>();
        builder.Services.AddScoped<IScheduleManager, SimpleSchedulerBusiness.Sqlite.ScheduleManager>();
        builder.Services.AddScoped<IJobManager, SimpleSchedulerBusiness.Sqlite.JobManager>();
        builder.Services.AddScoped<IUserManager, SimpleSchedulerBusiness.Sqlite.UserManager>();
        break;
}
builder.Services.AddScoped<DatabaseFactory>();
builder.Services.AddScoped<IEmailer, Emailer>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

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

app.Map("/EnvironmentName", (IConfiguration config) =>
{
    return Results.Ok(config["EnvironmentName"]);
});

app.Map("/HelloThere", () =>
{
    return Results.Ok(new { Message = "Howdy" });
});

app.Map("/GetUtcNow", () =>
{
    return Results.Ok(JsonSerializer.Serialize(DateTime.UtcNow.ToString("MMM dd yyyy HH\\:mm\\:ss")));
});

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

/******+

    public void ConfigureServices(IServiceCollection services)
    {
        

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

 * *****/
