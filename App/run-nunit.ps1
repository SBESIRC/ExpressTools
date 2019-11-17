Task Release.Build {
    $script:buildType = "Release"
}

Task Debug.Build {
    $script:buildType = "Debug"
}

Task Requires.XUnitConsole {

    $script:xunitExe = Resolve-Path "$env:USERPROFILE\.nuget\packages\nunit.consolerunner\3.10.0\tools\nunit3-console.exe"

    if ($xunitExe -ne $null) {
        Write-Host "Found XUnit.Console here: $xunitExe"
    }
}

Task Requires.GallioConsole {
    $Script:gallioExe = Resolve-Path "$env:USERPROFILE\.nuget\packages\galliobundle\3.4.14\bin\Gallio.Echo.exe"

    if ($gallioExe -ne $null)
    {
        Write-Host "Found Gallio Echo here: $gallioExe"
    }
}

Task Unit.Tests -Depends Requires.XUnitConsole {
    exec {
        & $xunitExe ".\build\bin\$buildType\ThAreaFrame.Test.dll"
    }
}

Task Gallio.Tests -Depends Requires.GallioConsole {
    Exec {
        & $gallioExe ".\build\bin\$buildType\ThXClip.Test.dll" /r:AutoCAD
    }
}