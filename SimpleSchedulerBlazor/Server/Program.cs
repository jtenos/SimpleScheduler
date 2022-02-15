global using SimpleSchedulerModels;
global using System.Collections.Immutable;

using SimpleSchedulerBlazor.Server;
using SimpleSchedulerBusiness;
using SimpleSchedulerData;
using SimpleSchedulerEmail;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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

builder.Services.AddCors();

builder.Services.AddAuthorization(config =>
{
    // Only one policy - you must be a valid user. Valid user can see all pages.
    config.AddPolicy("ValidUser", policy => policy.RequireClaim(ClaimTypes.Email));
});

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

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseMiddleware<JwtMiddleware>();

//app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/EnvironmentName", (IConfiguration config) =>
{
    return Results.Ok(config["EnvironmentName"]);
});

app.MapGet("/HelloThere", () =>
{
    return Results.Ok(new { Message = "Howdy" });
});

app.MapGet("/GetUtcNow", () =>
{
    return Results.Ok(JsonSerializer.Serialize(DateTime.UtcNow.ToString("MMM dd yyyy HH\\:mm\\:ss")));
});

app.MapFallbackToFile("index.html");

app.Run();
