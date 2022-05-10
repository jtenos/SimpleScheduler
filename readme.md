To build from command line (specifically PowerShell on Windows):

## API:

`dotnet publish .\SimpleSchedulerAPI\ --configuration Release`

This will put your output into ./bin/Release/net6.0/publish/

Add this to IIS, and ensure its application pool is running under a user that has rights to the database (if you're using Windows Authentication for SQL Server), and rights to the filesystem for the workers directory.

-- TODO: Add instructions for running this in Mac or Linux

## Blazor WASM:

`dotnet publish .\SimpleSchedulerBlazorWasm\ --configuration Release`

This will put your output into ./bin/Release/net6.0/publish/

This is a static WASM application, so you should be able to host this on any web server that you'd like, including a static site.

## Service:

`dotnet publish .\SimpleSchedulerService\ --configuration Release`

This will put your output into ./bin/Release/net6.0/publish/

Copy the output to your server, and execute the following as administrator (This is for Windows):

`sc.exe create MyServiceName binpath="X:\path-to-app\SimpleSchedulerService.exe"`

-- TODO: Provide the PowerShell equivalent

-- TODO: Add instructions for running this in Mac or Linux

## ServiceChecker:

`dotnet publish .\SimpleSchedulerServiceChecker\ --configuration Release`

This will put your output into ./bin/Release/net6.0/publish/

Copy the output to your server, and execute the following as administrator (This is for Windows):

`sc.exe create MyServiceName binpath="X:\path-to-app\SimpleSchedulerServiceChecker.exe"`

-- TODO: Provide the PowerShell equivalent

-- TODO: Add instructions for running this in Mac or Linux

## SQL Server:

You can publish from the Visual Studio `SimpleSchedulerSqlServerDB.publish.xml` file, or create the objects manually.

You'll need to add an email address to the Users table manually.

-- TODO: Provide instructions or at least a link for running SQL Server from Docker

-- TODO: Make a better SQL Server install script

-- TODO: More instructions on how to use scheduler

-- TODO: On start of API, create account for AdminEmail (from config file) in database if it doesn't already exist
