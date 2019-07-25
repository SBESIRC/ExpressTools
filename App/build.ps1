#
# PSake build script for ThCADPlugin Apps
#
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

# Release build for AutoCAD R18
Task Compile.Assembly.R18 -Depends Requires.MSBuild {
    exec { 
            & $msbuildExe /verbosity:minimal ".\ThCui\ThCui.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal ".\ThCui\ThCui.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal ".\ThParking\ThParking.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal ".\ThParking\ThParking.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal ".\ThAutoUpdate\ThAutoUpdate.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal ".\ThAutoUpdate\ThAutoUpdate.csproj" /p:Configuration=$buildType /t:rebuild
    }
}

# $buildType build for AutoCAD R19
Task Compile.Assembly.R19 -Depends Requires.MSBuild {
    exec { 
        & $msbuildExe /verbosity:minimal ".\ThCui\ThCui.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal ".\ThCui\ThCui.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
        & $msbuildExe /verbosity:minimal ".\ThParking\ThParking.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal ".\ThParking\ThParking.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
        & $msbuildExe /verbosity:minimal ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
        & $msbuildExe /verbosity:minimal ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
        & $msbuildExe /verbosity:minimal ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
        & $msbuildExe /verbosity:minimal ".\ThAutoUpdate\ThAutoUpdate.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal ".\ThAutoUpdate\ThAutoUpdate.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
    }
}

# $buildType build for AutoCAD R20
Task Compile.Assembly.R20 -Depends Requires.MSBuild {
    exec { 
        & $msbuildExe /verbosity:minimal ".\ThCui\ThCui.csproj" "${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal ".\ThCui\ThCui.csproj" "${buildType}-NET45" /t:rebuild
        & $msbuildExe /verbosity:minimal ".\ThParking\ThParking.csproj" "${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal ".\ThParking\ThParking.csproj" "${buildType}-NET45" /t:rebuild
        & $msbuildExe /verbosity:minimal ".\ThAreaFrame\ThAreaFrame.csproj" "${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal ".\ThAreaFrame\ThAreaFrame.csproj" "${buildType}-NET45" /t:rebuild
        & $msbuildExe /verbosity:minimal ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" "${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" "${buildType}-NET45" /t:rebuild
        & $msbuildExe /verbosity:minimal ".\ThAnalytics\ThAnalytics.csproj" "${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal ".\ThAnalytics\ThAnalytics.csproj" "${buildType}-NET45" /t:rebuild
    }
}

Task Requires.BuildType {
    if ($buildType -eq $null) {
        throw "No build type specified"
    }

    Write-Host "$buildType build confirmed"
}

# $buildType build for ThCADPluginInstaller
Task Compile.Installer -Depends Requires.BuildType, Compile.Assembly.R18, Compile.Assembly.R19 {
    if ($buildType -eq $null) {
        throw "No build type specified"
    }
    exec { 
        & $msbuildExe /verbosity:minimal ".\ThCui\ThCui.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThParking\ThParking.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration=Release /t:restore
        & $msbuildExe /verbosity:minimal ".\ThAutoUpdate\ThAutoUpdate.csproj" /p:Configuration=Release /t:restore
        & $msbuildExe /verbosity:minimal ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThCADInstaller\ThCADInstaller.wixproj" /p:Configuration=$buildType /t:rebuild
    }
}
