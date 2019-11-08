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
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThCui\ThCui.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThCui\ThCui.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThParking\ThParking.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThParking\ThParking.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThIdentity\ThIdentity.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThIdentity\ThIdentity.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThLicense\ThLicense.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThLicense\ThLicense.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThAutoUpdate\ThAutoUpdate.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThAutoUpdate\ThAutoUpdate.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThAreaFrameConfig\ThAreaFrameConfig.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThAreaFrameConfig\ThAreaFrameConfig.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThPlot\ThPlot.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThPlot\ThPlot.csproj" /p:Configuration=$buildType /t:rebuild
		    & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThSpray\ThSpray.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThSpray\ThSpray.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThRoomBoundary\ThRoomBoundary.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThRoomBoundary\ThRoomBoundary.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThXClip\ThXClip.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThXClip\ThXClip.csproj" /p:Configuration=$buildType /t:rebuild
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThMirror\ThMirror.csproj" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir=..\build\bin\$buildType\,IntermediateOutputPath=..\build\obj\$buildType\ ".\ThMirror\ThMirror.csproj" /p:Configuration=$buildType /t:rebuild
    }
}

# $buildType build for AutoCAD R19
Task Compile.Assembly.R19 -Depends Requires.MSBuild {
    exec { 
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThCui\ThCui.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThCui\ThCui.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThParking\ThParking.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThParking\ThParking.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThIdentity\ThIdentity.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThIdentity\ThIdentity.csproj" /p:Configuration=$buildType /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThLicense\ThLicense.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThLicense\ThLicense.csproj" /p:Configuration=$buildType /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThAutoUpdate\ThAutoUpdate.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThAutoUpdate\ThAutoUpdate.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThAreaFrameConfig\ThAreaFrameConfig.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThAreaFrameConfig\ThAreaFrameConfig.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThPlot\ThPlot.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThPlot\ThPlot.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
		& $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThSpray\ThSpray.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThSpray\ThSpray.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThRoomBoundary\ThRoomBoundary.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThRoomBoundary\ThRoomBoundary.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThXClip\ThXClip.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThXClip\ThXClip.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThMirror\ThMirror.csproj" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThMirror\ThMirror.csproj" /p:Configuration="${buildType}-NET40" /t:rebuild
    }
}

# $buildType build for AutoCAD R20
Task Compile.Assembly.R20 -Depends Requires.MSBuild {
    exec { 
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThCui\ThCui.csproj" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThCui\ThCui.csproj" /p:Configuration="${buildType}-NET45" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThParking\ThParking.csproj" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThParking\ThParking.csproj" /p:Configuration="${buildType}-NET45" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration="${buildType}-NET45" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration="${buildType}-NET45" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration="${buildType}-NET45" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThIdentity\ThIdentity.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThIdentity\ThIdentity.csproj" /p:Configuration=$buildType /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThLicense\ThLicense.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThLicense\ThLicense.csproj" /p:Configuration=$buildType /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThAutoUpdate\ThAutoUpdate.csproj" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThAutoUpdate\ThAutoUpdate.csproj" /p:Configuration="${buildType}-NET45" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThAreaFrameConfig\ThAreaFrameConfig.csproj" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThAreaFrameConfig\ThAreaFrameConfig.csproj" /p:Configuration="${buildType}-NET45" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThPlot\ThPlot.csproj" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThPlot\ThPlot.csproj" /p:Configuration="${buildType}-NET45" /t:rebuild
		& $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThSpray\ThSpray.csproj" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThSpray\ThSpray.csproj" /p:Configuration="${buildType}-NET45" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThRoomBoundary\ThRoomBoundary.csproj" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThRoomBoundary\ThRoomBoundary.csproj" /p:Configuration="${buildType}-NET45" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThXClip\ThXClip.csproj" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThXClip\ThXClip.csproj" /p:Configuration="${buildType}-NET45" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThMirror\ThMirror.csproj" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThMirror\ThMirror.csproj" /p:Configuration="${buildType}-NET45" /t:rebuild
    }
}

Task Requires.BuildType {
    if ($buildType -eq $null) {
        throw "No build type specified"
    }

    Write-Host "$buildType build confirmed"
}

Task Sign {
    exec { 
        cd Sign
        .\sign.ps1
        cd ..
    }
}

# $buildType build for ThCADPluginInstaller
Task Compile.Installer -Depends Requires.BuildType, Compile.Assembly.R18, Compile.Assembly.R19, Compile.Assembly.R20, Sign {
    if ($buildType -eq $null) {
        throw "No build type specified"
    }
    exec { 
        & $msbuildExe /verbosity:minimal ".\ThCui\ThCui.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThParking\ThParking.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThAreaFrame\ThAreaFrame.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThAnalytics\ThAnalytics.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThIdentity\ThIdentity.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThLicense\ThLicense.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThAutoUpdate\ThAutoUpdate.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThAreaFrameConfig\ThAreaFrameConfig.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThElectricalSysDiagram\ThElectricalSysDiagram.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThPlot\ThPlot.csproj" /p:Configuration=$buildType /t:restore
		& $msbuildExe /verbosity:minimal ".\ThSpray\ThSpray.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThRoomBoundary\ThRoomBoundary.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThXClip\ThXClip.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThMirror\ThMirror.csproj" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal ".\ThCADInstaller\ThCADInstaller.wixproj" /p:Configuration=$buildType /t:rebuild
    }
}
