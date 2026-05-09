$PublishDir = "C:\Scheduler_Publish"
# The API now references and bundles the BlazorWasm project, so it does not
# need to be published separately.
$DotNetProjects = "SimpleSchedulerAPI", "SimpleSchedulerService", "SimpleSchedulerServiceChecker"

$FrameworkDependentDir = Join-Path -Path $PublishDir -ChildPath "framework-dependent"
$SelfContainedDir = Join-Path -Path $PublishDir -ChildPath "win-x64-self-contained"

if (Test-Path -Path $PublishDir) {
	Remove-Item $PublishDir -Recurse -Force
}

# Create publish directories
New-Item -ItemType Directory -Path $PublishDir
New-Item -ItemType Directory -Path $FrameworkDependentDir
New-Item -ItemType Directory -Path $SelfContainedDir

# Build each project and publish assuming the target machine has .NET 10.0 installed
foreach ($Project in $DotNetProjects) {
	dotnet publish ".\$($Project)" --configuration Release
	$DestDir = Join-Path -Path $FrameworkDependentDir -ChildPath $Project
	Move-Item -Path ".\$($Project)\bin\Release\net10.0\publish" -Destination $DestDir
}

# Build each project and publish self-contained for Windows x64
foreach ($Project in $DotNetProjects) {
	dotnet publish ".\$($Project)" --configuration Release --self-contained --runtime win-x64
	$DestDir = Join-Path -Path $SelfContainedDir -ChildPath $Project
	Move-Item -Path ".\$($Project)\bin\Release\net10.0\win-x64\publish" -Destination $DestDir
}
