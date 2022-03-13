using Microsoft.AspNetCore.Authentication.JwtBearer;
using SimpleSchedulerBlazor.Server.Auth;
using Microsoft.IdentityModel.Tokens;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerAppServices.Implementations;
using Polly;
using Polly.Retry;
using SimpleSchedulerEmail;
using SimpleSchedulerData;
using Microsoft.OpenApi.Models;
using SimpleSchedulerBlazor.Server;
using SimpleSchedulerBlazor.Server.ApiServices;
using SimpleSchedulerSerilogEmail;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("secrets.json", optional: true);

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

builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddSingleton<IJobManager, JobManager>();
builder.Services.AddSingleton<IScheduleManager, ScheduleManager>();
builder.Services.AddSingleton<IUserManager, UserManager>();
builder.Services.AddSingleton<IWorkerManager, WorkerManager>();

builder.Services.AddSingleton<SqlDatabase>(sp =>
{
    string connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("SimpleScheduler");
    AsyncRetryPolicy retryPolicy = sp.GetRequiredService<AsyncRetryPolicy>();
    return new SqlDatabase(connectionString, retryPolicy);
});

builder.Services.AddSingleton<AsyncRetryPolicy>(Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(retryCount: 2,
        sleepDurationProvider: times => TimeSpan.FromSeconds(3),
        onRetry: async (ex, ts) => { await Task.CompletedTask; })
);

builder.Services.AddSingleton<IEmailer>((sp) =>
{
    IConfiguration config = sp.GetRequiredService<IConfiguration>();
    var mailSettings = config.MailSettings();
    Emailer emailer = new(
        Port: mailSettings.Port,
        EmailFrom: mailSettings.EmailFrom,
        AdminEmail: mailSettings.AdminEmail,
        Host: mailSettings.Host,
        UserName: mailSettings.UserName,
        Password: mailSettings.Password,
        EnvironmentName: config.EnvironmentName()
    );
    EmailSink.SetEmailer(emailer);
    return emailer;
});

builder.Services.AddCors();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    (string issuer, string audience, string key) = builder.Configuration.Jwt();
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromHexString(key))
    };
});

WebApplication app = builder.Build();

app.UseMiddleware(typeof(ExceptionHandlingMiddleware));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v3/swagger.json", "SimpleScheduler v3");
    });
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

app.UseAuthentication();
app.UseAuthorization();

app.MapHomeService();
app.MapJobsService();
app.MapLoginService();
app.MapSchedulesService();
app.MapWorkersService();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapFallbackToFile("index.html");

app.Run();
