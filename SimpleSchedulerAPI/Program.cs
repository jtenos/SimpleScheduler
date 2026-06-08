using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SimpleSchedulerAppServices.Interfaces;
using Polly;
using Polly.Retry;
using SimpleSchedulerEmail;
using SimpleSchedulerData;
using Microsoft.OpenApi;
using SimpleSchedulerSerilogEmail;
using Serilog;
using SimpleSchedulerAPI;
using SimpleSchedulerAPI.Auth;
using SimpleSchedulerAPI.ApiServices;

Serilog.Debugging.SelfLog.Enable(msg => System.Diagnostics.Debug.WriteLine(msg));

WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Host.UseWindowsService();

builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v3", new OpenApiInfo
    {
        Title = "SimpleScheduler",
        Version = "v3"
    });
});

// The database provider is selected via configuration (Database:Provider). It picks both the
// IDatabase implementation and the matching set of managers (same class names, separate namespaces).
string databaseProvider = builder.Configuration["Database:Provider"] ?? "SqlServer";
if (string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IJobManager, SimpleSchedulerAppServices.Implementations.Sqlite.JobManager>();
    builder.Services.AddSingleton<IScheduleManager, SimpleSchedulerAppServices.Implementations.Sqlite.ScheduleManager>();
    builder.Services.AddSingleton<IUserManager, SimpleSchedulerAppServices.Implementations.Sqlite.UserManager>();
    builder.Services.AddSingleton<IWorkerManager, SimpleSchedulerAppServices.Implementations.Sqlite.WorkerManager>();

    builder.Services.AddSingleton<IDatabase>(sp =>
    {
        IConfiguration config = sp.GetRequiredService<IConfiguration>();
        string sqlitePath = config["Database:SqlitePath"] is { Length: > 0 } configured
            ? configured
            : Path.Combine(AppContext.BaseDirectory, "SimpleScheduler.sqlite");
        Microsoft.Data.Sqlite.SqliteConnectionStringBuilder csb = new()
        {
            DataSource = sqlitePath,
            Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWriteCreate
        };
        AsyncRetryPolicy retryPolicy = sp.GetRequiredService<AsyncRetryPolicy>();
        return new SqliteDatabase(csb.ToString(), retryPolicy);
    });
}
else
{
    builder.Services.AddSingleton<IJobManager, SimpleSchedulerAppServices.Implementations.SqlServer.JobManager>();
    builder.Services.AddSingleton<IScheduleManager, SimpleSchedulerAppServices.Implementations.SqlServer.ScheduleManager>();
    builder.Services.AddSingleton<IUserManager, SimpleSchedulerAppServices.Implementations.SqlServer.UserManager>();
    builder.Services.AddSingleton<IWorkerManager, SimpleSchedulerAppServices.Implementations.SqlServer.WorkerManager>();

    builder.Services.AddSingleton<IDatabase>(sp =>
    {
        string connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("SimpleScheduler")!;
        AsyncRetryPolicy retryPolicy = sp.GetRequiredService<AsyncRetryPolicy>();
        return new SqlServerDatabase(connectionString, retryPolicy);
    });
}

builder.Services.AddSingleton<AsyncRetryPolicy>(Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(retryCount: 2,
        sleepDurationProvider: times => TimeSpan.FromSeconds(3),
        onRetry: async (ex, ts) => { await Task.CompletedTask; })
);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.AddSerilog();
});

builder.Services.AddSingleton<IEmailer>((sp) =>
{
    IConfiguration config = sp.GetRequiredService<IConfiguration>();

    IEmailer emailer;
    if (!string.IsNullOrWhiteSpace(config["EmailFolder"]))
    {
        emailer = new LogFileEmailer(config["EmailFolder"]!);
    }
    else
    {
        var mailSettings = config.MailSettings();

        emailer = new Emailer(
            Port: mailSettings.Port,
            EmailFrom: mailSettings.EmailFrom,
            AdminEmail: mailSettings.AdminEmail,
            Host: mailSettings.Host,
            UserName: mailSettings.UserName,
            Password: mailSettings.Password,
            EnvironmentName: config.EnvironmentName()
        );
    }

    EmailSink.SetEmailer(emailer);
    return emailer;
});

builder.Services.AddSingleton<ITokenService>(new TokenService());

builder.Services.AddCors();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
{
    (string issuer, string audience, string key) = builder.Configuration.Jwt();
    opt.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromHexString(key))
        //ClockSkew is 5 minutes by default, so this still passes for up to 5 minutes
    };
});

WebApplication app = builder.Build();

app.UsePathBase(app.Configuration["PathBase"]);

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v3/swagger.json", "SimpleScheduler v3");
    });
}
else
{
    // ExceptionHandlingMiddleware (above) handles all unhandled exceptions globally.
    // Don't add UseExceptionHandler("/Error") here — the path has no endpoint, so the
    // re-execution lands on MapFallbackToFile("index.html"), which only allows GET/HEAD,
    // and a thrown exception on a POST then surfaces as 405 Method Not Allowed.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.MapHomeService();
app.MapJobsService();
app.MapLoginService();
app.MapSchedulesService();
app.MapWorkersService();

app.MapFallbackToFile("index.html");

app.Run();
