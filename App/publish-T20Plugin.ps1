param($script:plugin)

#publish to server
$script:pscpexe = resolve-path "C:\Program Files\PuTTY\pscp.exe"
if ($null -eq $pscpexe)
{
    Write-Host "Failed to find pscp"
    return
}

#T20-PlugIn V4.0
$script:v4exe = "T20-PlugIn V4.0.exe"
$script:v4path = Join-Path -Path $script:plugin -ChildPath $script:v4exe
& $pscpexe -l root -pw Tianhuaai19 $script:v4path 49.234.60.227:/root/countly-server-master/frontend/express/public/AI/thcad/Release

#T20-PlugIn V5.0
$script:v5exe = "T20-PlugIn V5.0.exe"
$script:v5path = Join-Path -Path $script:plugin -ChildPath $script:v5exe
& $pscpexe -l root -pw Tianhuaai19 $script:v5path 49.234.60.227:/root/countly-server-master/frontend/express/public/AI/thcad/Release