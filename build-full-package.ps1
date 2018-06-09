param (
    [string]$configuration = "Debug",
    [Parameter(Mandatory=$true)][string]$tag
)

Write-Host Building $configuration for tag $tag

Remove-Item $PSScriptRoot\download\ -Force -Recurse
New-Item -ItemType Directory -Force -Path $PSScriptRoot\download\

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$client = New-Object System.Net.WebClient
$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libveldrid-spirv.dll",
    "$PSScriptRoot/download/libveldrid-spirv.dll")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download libveldrid-spirv.dll. This most likely indicates the Windows native build failed."
    exit
}

$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libveldrid-spirv.so",
    "$PSScriptRoot/download/libveldrid-spirv.so")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download libveldrid-spirv.so. This most likely indicates the Linux native build failed."
    exit
}

$client.DownloadFile(
    "https://github.com/mellinoe/veldrid-spirv/releases/download/$tag/libveldrid-spirv.dylib",
    "$PSScriptRoot/download/libveldrid-spirv.dylib")
if( -not $? )
{
    $msg = $Error[0].Exception.Message
    Write-Error "Couldn't download libveldrid-spirv.dylib. This most likely indicates the macOS native build failed."
    exit
}

dotnet msbuild src\Veldrid.SPIRV\Veldrid.SPIRV.csproj /p:Configuration=$configuration /p:NativeAssetsPath=$PSScriptRoot/download/ /t:Pack