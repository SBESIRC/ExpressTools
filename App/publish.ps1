param($script:releasemsi, $script:dsapriv, $script:appcast, $script:releasenote)

$monthenum = "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
[string]$script:weekday = $((Get-Date).DayOfWeek)
$weekdayshort = $weekday.Substring(0, 3)
$script:day = $(Get-Date -Format 'dd')
[int]$script:month = $(Get-Date -Format 'MM')
$enmonth = $monthenum[$month - 1]
$script:year = $(Get-Date -Format 'yyyy')
$script:hour = $(Get-Date -Format 'HH')
$script:minute = $(Get-Date -Format 'mm')
$script:second = $(Get-Date -Format 'ss')
$script:date = "${weekdayshort}, ${day} ${enmonth} ${year} ${hour}:${minute}:${second} +0800"

$commond_parameter_missing = "please ckeck your commond and use commond like:<.\shellname> <*NetSparkle_DSA.priv> <*appcast.xml> <*.html/*.md> <*.msi>"

if ($null -eq $releasemsi)
{
    Write-Host $commond_parameter_missing
    return
}
elseif (-not ($releasemsi -like "*.msi"))
{
    Write-Host "Msi file name wrong"
    return
}
$script:msilength = (Get-Item $releasemsi).length
if ($null -eq $dsapriv)
{
    $dsapriv = ".\ThAutoUpdate\NetSparkle_DSA.priv"
}
elseif (-not ($dsapriv -like "*NetSparkle_DSA.priv"))
{
    Write-Host "Dsa priv file name wrong"
    return
}
if ($null -eq $appcast)
{
    $appcast = ".\ThAutoUpdate\appcast.xml"
}
elseif (-not ($appcast -like "*appcast.xml"))
{
    Write-Host "Appcast.xml priv file name wrong"
    return
}
if ($null -eq $releasenote)
{
    $releasenote = ".\ThAutoUpdate\release-note.md"
}
elseif (-not (($releasenote -like "*.html") -or ($releasenote -like "*.md")))
{
    Write-Host "Html/Markdown file name wrong"
    return
}

#sign msi
$script:dsafile = $appcast + ".dsa"
$script:DSAHelperExe = resolve-path "C:\Users\${env:UserName}\.nuget\packages\netsparkle.new.tools\0.16.6\tools\NetSparkle.DSAHelper.exe"
if ($null -eq $DSAHelperExe)
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

$script:date_old = "<pubDate>.*</pubDate>"
$script:date_new = "<pubDate>$date</pubDate>"

#change dsasignicure in appcast.xml
(Get-Content $appcast) -replace ($sparklesige_old, $sparklesige_new)|out-file $appcast -encoding utf8
Write-Host "Change appcast dsaSignature completely"
(Get-Content $appcast) -replace ($length_old, $length_new)|out-file $appcast -encoding utf8
Write-Host "Change appcast msi length completely"
(Get-Content $appcast) -replace ($date_old, $date_new)|out-file $appcast -encoding utf8
Write-Host "Change appcast pubdate completely"

#sign appcast
& $DSAHelperExe /sign_update $appcast $dsapriv|out-file $dsafile -encoding utf8
Write-Host "Sign ($appcast) completely"

#publish to server
$script:pscpexe = resolve-path "C:\Program Files\PuTTY\pscp.exe"
if ($null -eq $pscpexe)
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
