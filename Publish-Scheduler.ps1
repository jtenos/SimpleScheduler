$PublishDir = "C:\Scheduler_Publish"
$DotNetProjects = "SimpleSchedulerAPI", "SimpleSchedulerService", "SimpleSchedulerServiceChecker"
$WasmProject = "SimpleSchedulerBlazorWasm"

$FrameworkDependentDir = Join-Path -Path $PublishDir -ChildPath "framework-dependent"
$SelfContainedDir = Join-Path -Path $PublishDir -ChildPath "win-x64-self-contained"

if (Test-Path -Path $PublishDir) {
	Remove-Item $PublishDir -Recurse -Force
}

# Create publish directories
New-Item -ItemType Directory -Path $PublishDir
New-Item -ItemType Directory -Path $FrameworkDependentDir
New-Item -ItemType Directory -Path $SelfContainedDir

# Build each project and publish assuming the target machine has .NET 6.0 installed
foreach ($Project in $DotNetProjects) {
	dotnet publish ".\$($Project)" --configuration Release
	$DestDir = Join-Path -Path $FrameworkDependentDir -ChildPath $Project
	Move-Item -Path ".\$($Project)\bin\Release\net6.0\publish" -Destination $DestDir
}

# Build each project and publish self-contained for Windows x64
foreach ($Project in $DotNetProjects) {
	dotnet publish ".\$($Project)" --configuration Release --self-contained --runtime win-x64
	$DestDir = Join-Path -Path $SelfContainedDir -ChildPath $Project
	Move-Item -Path ".\$($Project)\bin\Release\net6.0\win-x64\publish" -Destination $DestDir
}

# Build the WASM project (not a .NET project) and copy to both locations
dotnet publish ".\$($WasmProject)" --configuration Release
$DestDir = Join-Path -Path $FrameworkDependentDir -ChildPath $WasmProject
Copy-Item -Path ".\$($WasmProject)\bin\Release\net6.0\publish" -Destination $DestDir -Recurse
$DestDir = Join-Path -Path $SelfContainedDir -ChildPath $WasmProject
Move-Item -Path ".\$($WasmProject)\bin\Release\net6.0\publish" -Destination $DestDir
