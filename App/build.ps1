#
# PSake build script for ThCADPlugin Apps
#

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
            & $msbuildExe ".\ThCui\ThCui.csproj" /p:Configuration=Release /t:restore
            & $msbuildExe ".\ThCui\ThCui.csproj" /p:Configuration=Release /t:rebuild
            & $msbuildExe ".\ThParking\ThParking.csproj" /p:Configuration=Release /t:restore
            & $msbuildExe ".\ThParking\ThParking.csproj" /p:Configuration=Release /t:rebuild
            & $msbuildExe ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration=Release /t:restore
            & $msbuildExe ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration=Release /t:rebuild
            & $msbuildExe ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration=Release /t:restore
            & $msbuildExe ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration=Release /t:rebuild
            & $msbuildExe ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration=Release /t:restore
            & $msbuildExe ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration=Release /t:rebuild
    }
}

# Release build for AutoCAD R19
Task Compile.Assembly.R19 -Depends Requires.MSBuild {
    exec { 
        & $msbuildExe ".\ThCui\ThCui.csproj" /p:Configuration=Release-NET40 /t:restore
        & $msbuildExe ".\ThCui\ThCui.csproj" /p:Configuration=Release-NET40 /t:rebuild
        & $msbuildExe ".\ThParking\ThParking.csproj" /p:Configuration=Release-NET40 /t:restore
        & $msbuildExe ".\ThParking\ThParking.csproj" /p:Configuration=Release-NET40 /t:rebuild
        & $msbuildExe ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration=Release-NET40 /t:restore
        & $msbuildExe ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration=Release-NET40 /t:rebuild
        & $msbuildExe ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration=Release-NET40 /t:restore
        & $msbuildExe ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration=Release-NET40 /t:rebuild
        & $msbuildExe ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration=Release-NET40 /t:restore
        & $msbuildExe ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration=Release-NET40 /t:rebuild
    }
}

# Release build for AutoCAD R20
Task Compile.Assembly.R20 -Depends Requires.MSBuild {
    exec { 
        & $msbuildExe ".\ThCui\ThCui.csproj" /p:Configuration=Release-NET45 /t:restore
        & $msbuildExe ".\ThCui\ThCui.csproj" /p:Configuration=Release-NET45 /t:rebuild
        & $msbuildExe ".\ThParking\ThParking.csproj" /p:Configuration=Release-NET45 /t:restore
        & $msbuildExe ".\ThParking\ThParking.csproj" /p:Configuration=Release-NET45 /t:rebuild
        & $msbuildExe ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration=Release-NET45 /t:restore
        & $msbuildExe ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration=Release-NET45 /t:rebuild
        & $msbuildExe ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration=Release-NET45 /t:restore
        & $msbuildExe ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration=Release-NET45 /t:rebuild
        & $msbuildExe ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration=Release-NET45 /t:restore
        & $msbuildExe ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration=Release-NET45 /t:rebuild
    }
}

# Release build for ThCADPluginInstaller
Task Compile.Installer -Depends Compile.Assembly.R18, Compile.Assembly.R19 {
    exec { 
        & $msbuildExe ".\ThCui\ThCui.csproj" /p:Configuration=Release /t:restore
        & $msbuildExe ".\ThParking\ThParking.csproj" /p:Configuration=Release /t:restore
        & $msbuildExe ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration=Release /t:restore
        & $msbuildExe ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration=Release /t:restore
        & $msbuildExe ".\ThCADInstaller\ThCADInstaller.wixproj" /p:Configuration=Release /t:rebuild
    }
}