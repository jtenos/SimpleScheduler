To build from command line (specifically PowerShell on Windows):

## API:

`dotnet publish .\SimpleSchedulerAPI\ --configuration Release`

This will put your output into ./bin/Release/net6.0/publish/

## Blazor WASM:

`dotnet publish .\SimpleSchedulerBlazorWasm\ --configuration Release`

This will put your output into ./bin/Release/net6.0/publish/

## Service:

`dotnet publish .\SimpleSchedulerService\ --configuration Release`

This will put your output into ./bin/Release/net6.0/publish/

## ServiceChecker:

`dotnet publish .\SimpleSchedulerServiceChecker\ --configuration Release`

This will put your output into ./bin/Release/net6.0/publish/

## SQL Server:

You can publish from the Visual Studio `SimpleSchedulerSqlServerDB.publish.xml` file, or create the objects manually.

You'll need to add an email address to the Users table manually.

-- TODO: Make a better SQL Server install script

-- TODO: More instructions on how to use scheduler

-- TODO: On start of API, read config file, and create account for AdminEmail if it doesn't already exist
