Task Release.Build {
    $script:buildType = "Release"
}

Task Debug.Build {
    $script:buildType = "Debug"
}

Task Requires.MSBuild {
    # Visual Studio 2017 Community
    $script:msbuildExe = resolve-path "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"

    if ($msbuildExe -eq $null)
    {
        throw "Failed to find MSBuild"
    }
    Write-Host "Found MSBuild here: $msbuildExe"
}

Task Requires.Gallio {
    $script:galliofolder = "C:\Users\$env:UserName\.nuget\packages\galliobundle\3.4.14\bin"
    SET-Variable GALLIO_RUNTIME_PATH=C:\Users\$env:UserName\.nuget\packages\galliobundle\3.4.14\bin
    $script:Gallioexe = resolve-path "${galliofolder}\Gallio.Echo.exe"
    if ($Gallioexe -eq $null)
    {
        Write-Host "Failed to find Gallio.Echo.exe"
        return
    }
    Write-Host "Found Gallio.Echo.exe here: $Gallioexe"
}

Task Compile.Installer.Test -Depends Requires.MSBuild, Requires.Gallio  {
    exec { 
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThHarness\ThHarness.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThHarness\ThHarness.csproj" /p:Configuration=$buildType /t:rebuild
    }
}

Task Gallio.Tests -Depends Requires.Gallio, Compile.Installer.Test {
    exec {
        COPY-item -r .\build\bin\$buildType\ThHarness.dll $galliofolder
        & $Gallioexe C:\Users\$env:UserName\.nuget\packages\galliobundle\3.4.14\bin\ThHarness.dll /r:AutoCAD
        #Unable to delete dll file, because CAD is using it
        #Remove-Item "C:\Users\$env:UserName\.nuget\packages\galliobundle\3.4.14\bin\${dllname}"
    }
}
