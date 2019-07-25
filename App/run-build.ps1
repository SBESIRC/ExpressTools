param($buildType, $assembly_version)

if($buildType -ne "Release")
{
    if($buildType -ne "Debug")
    {
        echo "please check your command about ($buildType)"
        return
    }
}

if($assembly_version -ne $null)
{
    .\bump_assembly_version.ps1 ".\" "ALL" $assembly_version
}

Invoke-psake .\build.ps1 -Task "${buildType}.Build", Compile.Installer
