param($script:dsapriv, $script:appcast, $script:releasenote, $script:releasemsi)

$script:msilength = (Get-Item $releasemsi).length
$commond_parameter_missing = "please ckeck your commond and use commond like:<.\shellname> <*NetSparkle_DSA.priv> <*appcast.xml> <*.html/*.md> <*.msi>"
if ($dsapriv -eq $null)
{
    Write-Host $commond_parameter_missing
    return
}
elseif (-not ($dsapriv -like "*NetSparkle_DSA.priv"))
{
    Write-Host "Dsa priv file name wrong"
    return
}
if ($appcast -eq $null)
{
    Write-Host $commond_parameter_missing
    return
}
elseif (-not ($appcast -like "*appcast.xml"))
{
    Write-Host "Appcast.xml priv file name wrong"
    return
}
if ($releasenote -eq $null)
{
    Write-Host $commond_parameter_missing
    return
}
elseif (-not (($releasenote -like "*.html") -or ($releasenote -like "*.md")))
{
    Write-Host "Html/Markdown file name wrong"
    return
}
if ($releasemsi -eq $null)
{
    Write-Host $commond_parameter_missing
    return
}
elseif (-not ($releasemsi -like "*.msi"))
{
    Write-Host "Msi file name wrong"
    return
}

#sign msi
$script:dsafile = $appcast + ".dsa"
$script:DSAHelperExe = resolve-path "C:\Users\${env:UserName}\.nuget\packages\netsparkle.new.tools\0.16.6\tools\NetSparkle.DSAHelper.exe"
if ($DSAHelperExe -eq $null)
{
    Write-Host "Failed to find DSAHelper"
    return
}

$script:msisign = & $DSAHelperExe /sign_update $releasemsi $dsapriv
Write-Host "Sign ($releasemsi) completely"

$script:sparklesige_old = "sparkle:dsaSignature="+'"'+".*"+'"'
$script:sparklesige_new = "sparkle:dsaSignature="+'"'+"$msisign"+'"'

$script:length_old = "length="+'"'+".*"+'"'
$script:length_new = "length="+'"'+"$msilength"+'"'

#change dsasignicure in appcast.xml
(type $appcast) -replace ($sparklesige_old, $sparklesige_new)|out-file $appcast -encoding utf8
(type $appcast) -replace ($length_old, $length_new)|out-file $appcast -encoding utf8
Write-Host "Change appcast sparkle:dsaSignature completely"

#sign appcast
& $DSAHelperExe /sign_update $appcast $dsapriv|out-file $dsafile -encoding utf8
Write-Host "Sign ($appcast) completely"

#publish to server
$script:pscpexe = resolve-path "C:\Program Files\PuTTY\pscp.exe"
if ($pscpexe -eq $null)
{
    Write-Host "Failed to find pscp"
    return
}
#appcast.xml
& $pscpexe -l root -pw Tianhuaai19 "$appcast" 49.234.60.227:/root/countly-server-master/frontend/express/public/AI/thcad
#appcast.xml.dsa
& $pscpexe -l root -pw Tianhuaai19 "$dsafile" 49.234.60.227:/root/countly-server-master/frontend/express/public/AI/thcad
#releasenote.html
& $pscpexe -l root -pw Tianhuaai19 "$releasenote" 49.234.60.227:/root/countly-server-master/frontend/express/public/AI/thcad/ReleaseNote
#release.msi
& $pscpexe -l root -pw Tianhuaai19 "$releasemsi" 49.234.60.227:/root/countly-server-master/frontend/express/public/AI/thcad/Release
Write-Host "Publish completely"
