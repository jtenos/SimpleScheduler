To build from command line:

SimpleSchedulerAngular:
`ng build --prod --base-href /{{url-path-on-web-server}}/`

This puts the distribution files in ./dist/sch

If you're in IIS, make sure you're using the URL Rewrite module, and you'll need a Web.config that looks something like this:

--todo

API:
`dotnet build SimpleSchedulerAPI.csproj /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile`

This will put your output into ./bin/Release/net5.0/publish/

