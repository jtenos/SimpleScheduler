global using SimpleSchedulerModels;
global using System.Collections.Immutable;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SimpleSchedulerBusiness;
using SimpleSchedulerData;
using SimpleSchedulerEmail;
using SimpleSchedulerBlazor.Server.Auth;
using Microsoft.IdentityModel.Tokens;

// TODO: Strongly typed configuration

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Configuration.AddJsonFile("secrets.json", optional: true);

builder.Services.AddSingleton<ITokenService, TokenService>();

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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromHexString(builder.Configuration["Jwt:Key"]))
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
