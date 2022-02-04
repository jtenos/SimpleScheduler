To build from command line:

## SimpleSchedulerAngular:
`ng build --configuration production --base-href /{{url-path-on-web-server}}/`

This puts the distribution files in ./dist/sch

If you're in IIS, make sure you're using the URL Rewrite module, and you'll need a Web.config that looks something like this:

```
<configuration>
<system.webServer>
  <rewrite>
    <rules>
      <rule name="Angular Routes" stopProcessing="true">
        <match url=".*" />
        <conditions logicalGrouping="MatchAll">
          <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
          <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
        </conditions>
        <action type="Rewrite" url="/My-Directory-Name/" />
        <!--<action type="Rewrite" url="/" />-->
      </rule>
    </rules>
  </rewrite>
</system.webServer>
</configuration>
```

## API:

`dotnet publish SimpleSchedulerAPI.csproj --configuration Release --output bin/Release/net6.0/publish --runtime win-x64 --no-self-contained`

This assumes Windows and assumes that .NET 6.0 is installed. Modify appropriately for your needs.

This will put your output into ./bin/Release/net6.0/publish/

## Service:

`dotnet publish SimpleSchedulerService.csproj --configuration Release --output bin/Release/net6.0/publish --runtime win-x64 --no-self-contained`

This assumes Windows and assumes that .NET 6.0 is installed. Modify appropriately for your needs.

This will put your output into ./bin/Release/net6.0/publish/

## ServiceChecker:

`dotnet publish SimpleSchedulerServiceChecker.csproj --configuration Release --output bin/Release/net6.0/publish --runtime win-x64 --no-self-contained`

This assumes Windows and assumes that .NET 6.0 is installed. Modify appropriately for your needs.

This will put your output into ./bin/Release/net6.0/publish/

## SQL Server:

If you're using SQL Server, you can publish from Visual Studio, or there are just six tables, so run them in this order:

* Users.sql
* LoginAttempts.sql
* Workers.sql
* Schedules.sql
* Jobs.sql
* JobsArchive.sql

You'll need to add an email address to the Users table manually.

-- TODO: Make a better SQL Server install script

## SQLite:

Create your SQLite database, and you can execute the script found in SimpleSchedulerData/SqliteDatabase.cs/DATABASE_CREATION_SCRIPT

-- TODO: Make a better SQLite install script or a skeleton database file
