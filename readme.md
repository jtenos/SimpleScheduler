To build from command line (specifically PowerShell on Windows):

## API (also hosts the Blazor WASM frontend):

`dotnet publish .\SimpleSchedulerAPI\ --configuration Release`

This will put your output into `.\SimpleSchedulerAPI\bin\Release\net10.0\publish\`.

The API project references `SimpleSchedulerBlazorWasm`, so the WASM static files
are copied into `wwwroot/` automatically and served by the same process.

### Running as a Windows Service

The API is configured to run either as a console app (during development) or as
a Windows Service (in production) — `Host.UseWindowsService()` detects which.

1. Publish the project (see above) and copy the publish folder to your server,
   for example `C:\Services\SimpleSchedulerAPI`.
2. Set the listen URL in `appsettings.json` (`"Urls": "http://+:5266"` to bind
   on all interfaces, or pick whatever host/port you want). Configure
   connection strings, JWT settings, etc. in the same file.
3. Open an **elevated** PowerShell prompt and run:

   ```powershell
   New-Service -Name SimpleSchedulerAPI `
       -BinaryPathName "C:\Services\SimpleSchedulerAPI\SimpleSchedulerAPI.exe" `
       -DisplayName "Simple Scheduler API" `
       -Description "Simple Scheduler API and Blazor WASM frontend" `
       -StartupType Automatic
   # Optional: run under a specific account so it has rights to SQL / the
   # workers folder. Add: -Credential (Get-Credential MyDomain\MyUser)

   Start-Service SimpleSchedulerAPI
   ```

   Or with `sc.exe`:

   ```powershell
   sc.exe create SimpleSchedulerAPI binPath= "C:\Services\SimpleSchedulerAPI\SimpleSchedulerAPI.exe" start= auto DisplayName= "Simple Scheduler API"
   sc.exe description SimpleSchedulerAPI "Simple Scheduler API and Blazor WASM frontend"
   sc.exe start SimpleSchedulerAPI
   ```

4. To uninstall:

   ```powershell
   Stop-Service SimpleSchedulerAPI
   Remove-Service SimpleSchedulerAPI   # or: sc.exe delete SimpleSchedulerAPI
   ```

The service account needs:
- Permission to bind the configured URL (`http://+:port` requires either
  admin rights or a `netsh http add urlacl` reservation).
- Read/write access to the workers folder and any log paths in
  `appsettings.json`.
- SQL access (Windows auth uses the service account; otherwise put the
  credentials in the connection string).

If you want HTTPS, configure a `Kestrel:Endpoints:Https` block in
`appsettings.json` with a certificate, or put a reverse proxy (IIS, nginx,
YARP) in front of the service. By default the app listens on plain HTTP only;
the `UseHttpsRedirection` middleware is a no-op until an HTTPS endpoint exists.

-- TODO: Add instructions for running this in Mac or Linux

## Blazor WASM (standalone hosting — optional):

The WASM frontend ships inside the API service by default, so no separate
hosting is required. If you would rather host the static files on their own
(CDN, static site, separate web server), publish the project directly:

`dotnet publish .\SimpleSchedulerBlazorWasm\ --configuration Release`

This puts the static output in
`.\SimpleSchedulerBlazorWasm\bin\Release\net10.0\publish\wwwroot\`. Point that
deployment at your API by editing `wwwroot/appsettings.json` (`ApiUrl`).

## Service:

`dotnet publish .\SimpleSchedulerService\ --configuration Release`

This will put your output into `.\SimpleSchedulerService\bin\Release\net10.0\publish\`.

Copy the output to your server and install as a Windows Service:

```powershell
New-Service -Name SimpleScheduler `
    -BinaryPathName "C:\Services\SimpleSchedulerService\SimpleSchedulerService.exe" `
    -DisplayName "Simple Scheduler" `
    -Description "Simple Scheduler job runner" `
    -StartupType Automatic
Start-Service SimpleScheduler
```

-- TODO: Add instructions for running this in Mac or Linux

## ServiceChecker:

`dotnet publish .\SimpleSchedulerServiceChecker\ --configuration Release`

This will put your output into `.\SimpleSchedulerServiceChecker\bin\Release\net10.0\publish\`.

Install as a Windows Service the same way:

```powershell
New-Service -Name SimpleSchedulerChecker `
    -BinaryPathName "C:\Services\SimpleSchedulerServiceChecker\SimpleSchedulerServiceChecker.exe" `
    -DisplayName "Simple Scheduler Checker" `
    -Description "Simple Scheduler health checker" `
    -StartupType Automatic
Start-Service SimpleSchedulerChecker
```

-- TODO: Add instructions for running this in Mac or Linux

## SQL Server:

You can publish from the Visual Studio `SimpleSchedulerSqlServerDB.publish.xml` file, or create the objects manually.

You'll need to add an email address to the Users table manually.

Run this:
```
ALTER DATABASE [TheDatabaseName] SET ALLOW_SNAPSHOT_ISOLATION ON;
```

-- TODO: Provide instructions or at least a link for running SQL Server from Docker

-- TODO: Make a better SQL Server install script

-- TODO: More instructions on how to use scheduler

-- TODO: On start of API, create account for AdminEmail (from config file) in database if it doesn't already exist
