param($dllfile)

for ($i = $dllfile.length; $i -gt 0; $i--)
{
    if($dllfile[$i] -eq "\")
    {
        break
    }
}

$script:dllname = $dllfile.Substring($i + 1)
$script:galliofolder = "C:\Users\$env:UserName\.nuget\packages\galliobundle\3.4.14\bin"

COPY-item -r $dllfile $galliofolder
SET-Variable GALLIO_RUNTIME_PATH=C:\Users\$env:UserName\.nuget\packages\galliobundle\3.4.14\bin
$script:Gallioexe = resolve-path "${galliofolder}\Gallio.Echo.exe"
if ($Gallioexe -eq $null)
{
    Write-Host "Failed to find Gallio.Echo.exe"
    return
}
Write-Host "Found Gallio.Echo.exe here: $Gallioexe"

& $Gallioexe C:\Users\$env:UserName\.nuget\packages\galliobundle\3.4.14\bin\$dllname /r:AutoCAD

#Unable to delete dll file, because CAD is using it
#Remove-Item "C:\Users\$env:UserName\.nuget\packages\galliobundle\3.4.14\bin\${dllname}"
