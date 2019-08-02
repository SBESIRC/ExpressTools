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
    .\bump-version.ps1 ".\" "ALL" $assembly_version
}

Invoke-psake .\run-build.ps1 -Task "${buildType}.Build", Compile.Installer
Invoke-psake .\run-nunit.ps1 -Task "${buildType}.Build", Unit.Tests
Invoke-psake .\run-harness.ps1 -Task "${buildType}.Build", Gallio.Tests
