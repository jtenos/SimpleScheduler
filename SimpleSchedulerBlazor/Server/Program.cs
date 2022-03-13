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
using SimpleSchedulerBlazor.Server.Config;

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

builder.Services.AddScoped<IJobManager, JobManager>();
builder.Services.AddScoped<IScheduleManager, ScheduleManager>();
builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<IWorkerManager, WorkerManager>();

builder.Services.AddScoped<SqlDatabase>();

builder.Services.AddSingleton<AsyncRetryPolicy>(Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(retryCount: 2,
        sleepDurationProvider: times => TimeSpan.FromSeconds(3),
        onRetry: async (ex, ts) => { await Task.CompletedTask; })
);

builder.Services.AddSingleton<IEmailer>((sp) =>
{
    AppSettings appSettings = sp.GetRequiredService<AppSettings>();
    Emailer emailer = new(
        appSettings.MailSettings,
        appSettings.EnvironmentName
    );
    EmailSink.SetEmailer(emailer);
    return emailer;
});

AppSettings appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
builder.Services.AddSingleton(appSettings);

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
