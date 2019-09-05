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

Task Requires.XUnitConsole {

    $script:xunitExe =
        resolve-path "C:\Users\$env:UserName\.nuget\packages\nunit.consolerunner\3.10.0\tools\nunit3-console.exe"

    if ($xunitExe -eq $null)
    {
        throw "Failed to find XUnit.Console.exe"
    }

    Write-Host "Found XUnit.Console here: $xunitExe"
}

Task Compile.Installer.Test -Depends Requires.MSBuild, Requires.XUnitConsole  {
    exec { 
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThHarness\ThHarness.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThHarness\ThHarness.csproj" /p:Configuration=$buildType /t:rebuild
    }
}

Task Harness -Depends Requires.XUnitConsole, Compile.Installer.Test {
    $script:windows = New-Object -ComObject shell.application
    $windows.MinimizeAll()
    exec {
        & $xunitExe ".\build\bin\$buildType\ThHarness.dll"
    }
    $windows.UndoMinimizeALL()
}
