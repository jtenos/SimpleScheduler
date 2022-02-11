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
