param (
    [string]$configuration = "Release",
    [Parameter(Mandatory=$true)][string]$tag,
    [switch]$public
)

Write-Host Building $configuration NuGet package for tag $tag...
if (!$public)
{
    Write-Host This is a development build. Pass "-public" to remove the git commit from the package ID.
}

Write-Host Downloading native binaries from GitHub Releases...

if (Test-Path $PSScriptRoot\download\)
{
    Remove-Item $PSScriptRoot\download\ -Force -Recurse | Out-Null
}
New-Item -ItemType Directory -Force -Path $PSScriptRoot\download\Release | Out-Null
New-Item -ItemType Directory -Force -Path $PSScriptRoot\download\x86\Release | Out-Null
New-Item -ItemType Directory -Force -Path $PSScriptRoot\download\x64\Release | Out-Null

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$client = New-Object System.Net.WebClient
$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libveldrid-spirv.win-x86.dll",
    "$PSScriptRoot/download/x86/$configuration/libveldrid-spirv.dll")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download x86 libveldrid-spirv.dll. This most likely indicates the Windows native build failed."
    exit
}

Write-Host "- libveldrid-spirv.dll (x86)"

$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libveldrid-spirv.win-x64.dll",
    "$PSScriptRoot/download/x64/$configuration/libveldrid-spirv.dll")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download x64 libveldrid-spirv.dll. This most likely indicates the Windows native build failed."
    exit
}

Write-Host "- libveldrid-spirv.dll (x64)"

$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libveldrid-spirv.so",
    "$PSScriptRoot/download/$configuration/libveldrid-spirv.so")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download libveldrid-spirv.so. This most likely indicates the Linux native build failed."
    exit
}

Write-Host - libveldrid-spirv.so

$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libveldrid-spirv.dylib",
    "$PSScriptRoot/download/$configuration/libveldrid-spirv.dylib")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download libveldrid-spirv.dylib. This most likely indicates the macOS native build failed."
    exit
}
Write-Host - libveldrid-spirv.dylib (64-bit macOS)

$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libveldrid-spirv.a",
    "$PSScriptRoot/download/$configuration/libveldrid-spirv.a")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download libveldrid-spirv.a. This most likely indicates the iOS native build failed."
    exit
}
Write-Host - libveldrid-spirv.a (64-bit iOS)

$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libshaderc_combined.a",
    "$PSScriptRoot/download/$configuration/libshaderc_combined.a")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download libshaderc_combined.a. This most likely indicates the iOS native build failed."
    exit
}
Write-Host - libshaderc_combined.a (64-bit iOS)

Write-Host Generating NuGet package...

dotnet restore src\Veldrid.SPIRV\Veldrid.SPIRV.csproj

dotnet msbuild src\Veldrid.SPIRV\Veldrid.SPIRV.csproj /p:Configuration=$configuration /t:Pack /p:NativeAssetsPath=$PSScriptRoot/download/ /p:PublicRelease=$public
