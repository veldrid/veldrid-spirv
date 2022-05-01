param (
    [string]$configuration = "Release",
    [Parameter(Mandatory=$true)][string]$version,
    [switch]$public
)

Write-Host Building $configuration NuGet package for version $version...
$tag = "v"+$version
if (!$public)
{
    Write-Host This is a development build. Pass "-public" to remove the git commit from the package ID.
}

Write-Host Downloading native binaries from GitHub Releases...

if (Test-Path $PSScriptRoot\download\)
{
    Remove-Item $PSScriptRoot\download\ -Force -Recurse | Out-Null
}
New-Item -ItemType Directory -Force -Path $PSScriptRoot\download\$configuration | Out-Null
New-Item -ItemType Directory -Force -Path $PSScriptRoot\download\$configuration\win-x86 | Out-Null
New-Item -ItemType Directory -Force -Path $PSScriptRoot\download\$configuration\win-x64 | Out-Null
New-Item -ItemType Directory -Force -Path $PSScriptRoot\download\$configuration\linux-x64 | Out-Null
New-Item -ItemType Directory -Force -Path $PSScriptRoot\download\$configuration\osx | Out-Null
New-Item -ItemType Directory -Force -Path $PSScriptRoot\download\$configuration\ios | Out-Null
New-Item -ItemType Directory -Force -Path $PSScriptRoot\download\$configuration\android-arm64-v8a | Out-Null
New-Item -ItemType Directory -Force -Path $PSScriptRoot\download\$configuration\android-x86_64 | Out-Null
New-Item -ItemType Directory -Force -Path $PSScriptRoot\download\$configuration\android-armeabi-v7a | Out-Null

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$client = New-Object System.Net.WebClient
$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libveldrid-spirv.win-x86.dll",
    "$PSScriptRoot/download/$configuration/win-x86/libveldrid-spirv.dll")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download x86 libveldrid-spirv.dll. This most likely indicates the Windows native build failed."
    exit
}

Write-Host "- libveldrid-spirv.dll (x86)"

$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libveldrid-spirv.win-x64.dll",
    "$PSScriptRoot/download/$configuration/win-x64/libveldrid-spirv.dll")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download x64 libveldrid-spirv.dll. This most likely indicates the Windows native build failed."
    exit
}

Write-Host "- libveldrid-spirv.dll (x64)"

$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libveldrid-spirv.so",
    "$PSScriptRoot/download/$configuration/linux-x64/libveldrid-spirv.so")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download libveldrid-spirv.so (64-bit Linux). This most likely indicates the Linux native build failed."
    exit
}

Write-Host "- libveldrid-spirv.so (64-bit Linux)"

$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libveldrid-spirv.dylib",
    "$PSScriptRoot/download/$configuration/osx/libveldrid-spirv.dylib")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download libveldrid-spirv.dylib. This most likely indicates the macOS native build failed."
    exit
}
Write-Host "- libveldrid-spirv.dylib (macOS universal)"

$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libveldrid-spirv-combined.a",
    "$PSScriptRoot/download/$configuration/ios/libveldrid-spirv-combined.a")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download libveldrid-spirv-combined.a. This most likely indicates the iOS native build failed."
    exit
}
Write-Host "- libveldrid-spirv-combined.a (64-bit iOS)"

$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libveldrid-spirv.android-arm64-v8a.so",
    "$PSScriptRoot/download/$configuration/android-arm64-v8a/libveldrid-spirv.so")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download libveldrid-spirv.so (64-bit Android arm64-v8a). This most likely indicates the arm64-v8a Android native build failed."
    exit
}
Write-Host "- libveldrid-spirv.so (Android arm64-v8a)"

$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libveldrid-spirv.android-armeabi-v7a.so",
    "$PSScriptRoot/download/$configuration/android-armeabi-v7a/libveldrid-spirv.so")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download libveldrid-spirv.so (Android armeabi-v7a). This most likely indicates the armeabi-v7a Android native build failed."
    exit
}
Write-Host "- libveldrid-spirv.so (Android armeabi-v7a)"

$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libveldrid-spirv.android-x86_64.so",
    "$PSScriptRoot/download/$configuration/android-x86_64/libveldrid-spirv.so")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download libveldrid-spirv.so (Android android-x86_64). This most likely indicates the android-x86_64 Android native build failed."
    exit
}
Write-Host "- libveldrid-spirv.so (Android android-x86_64)"

Write-Host Generating NuGet package...

$env:VeldridSPIRVVersion = $version
$env:Configuration = $configuration
$env:NativeAssetsPath = "${PSScriptRoot}/download/"
$env:PublicRelease = ${public}

dotnet restore src\Veldrid.SPIRV\Veldrid.SPIRV.csproj

dotnet msbuild src\Veldrid.SPIRV\Veldrid.SPIRV.csproj /t:Pack

dotnet restore src\Veldrid.SPIRV.VariantCompiler\Veldrid.SPIRV.VariantCompiler.csproj --source ${PSScriptRoot}\bin\Packages\Release --no-cache

dotnet msbuild src\Veldrid.SPIRV.VariantCompiler\Veldrid.SPIRV.VariantCompiler.csproj

dotnet restore src\Veldrid.SPIRV.BuildTools\Veldrid.SPIRV.BuildTools.csproj --source ${PSScriptRoot}\bin\Packages\Release

dotnet msbuild src\Veldrid.SPIRV.BuildTools\Veldrid.SPIRV.BuildTools.csproj /t:Pack
