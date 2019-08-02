param($script:gitlocation, $script:version)

$commond_parameter_missing = "please ckeck your commond and use commond like:<.\shellname> <https://github.com/*> <*.*.*>"

if($gitlocation -eq $null)
{
    Write-Host $commond_parameter_missing
    return
}
elseif($gitlocation -notlike "https`:`/`/github`.com`/*")
{
    Write-Host "git location wrong"
    return
}
if($version -eq $null)
{
    Write-Host $commond_parameter_missing
    return
}
elseif(-not ($version -match "([0-9]{1,}\.[0-9]{1,}\.[0-9]{1,}\.[0-9]{1,}|[0-9]{1,}\.[0-9]{1,}\.[0-9]{1,})"))
{
    Write-Host "version wrong"
    return
}

#git clone
git clone $gitlocation

#cd file
$file = Get-ChildItem .\ | ?{$_.psiscontainer -eq $true} | %{$_.FullName}
cd $file

#git pull first
git pull

#git reset by version
$taglog = git show "v$version"
for ($i=0; $i -le $taglog.length; $i++)
{
    if($taglog[$i] -like 'commit *')　
    {
        $commitid = $taglog[$i].Substring(7, 6)
        break
    }
}
git reset --hard $commitid

#cd back
cd ../