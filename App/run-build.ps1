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

# $buildType build for AutoCAD R18
Task Compile.Assembly.R18.Common -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\$buildType\",IntermediateOutputPath="..\build\obj\$buildType\" ".\TianHuaCADApp.sln" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\$buildType\",IntermediateOutputPath="..\build\obj\$buildType\" ".\TianHuaCADApp.sln" /p:Configuration=$buildType /t:rebuild
    }
}

Task Compile.Assembly.R18.Structure -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\$buildType\",IntermediateOutputPath="..\build\obj\$buildType\" ".\TianHuaCAD.Structure.sln" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\$buildType\",IntermediateOutputPath="..\build\obj\$buildType\" ".\TianHuaCAD.Structure.sln" /p:Configuration=$buildType /t:rebuild
    }
}

Task Compile.Assembly.R18.WSS -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\$buildType\",IntermediateOutputPath="..\build\obj\$buildType\" ".\TianHuaCAD.WSS.sln" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\$buildType\",IntermediateOutputPath="..\build\obj\$buildType\" ".\TianHuaCAD.WSS.sln" /p:Configuration=$buildType /t:rebuild
    }
}

Task Compile.Assembly.R18.HAVC -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\$buildType\",IntermediateOutputPath="..\build\obj\$buildType\" ".\TianHuaCAD.HAVC.sln" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\$buildType\",IntermediateOutputPath="..\build\obj\$buildType\" ".\TianHuaCAD.HAVC.sln" /p:Configuration=$buildType /t:rebuild
    }
}

Task Compile.Resource.R18 -Depends Requires.MSBuild {
    exec {
            & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\$buildType\Dark\",IntermediateOutputPath="..\build\obj\$buildType\Dark\" ".\ThCuiRes\ThCuiRes.vcxproj" /t:rebuild
            & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\$buildType\Light\",IntermediateOutputPath="..\build\obj\$buildType\Light\" ".\ThCuiRes\ThCuiRes_light.vcxproj" /t:rebuild
    }
}

Task Build.Assembly.R18 -Depends Compile.Assembly.R18.Common, Compile.Assembly.R18.Structure, Compile.Assembly.R18.WSS, Compile.Assembly.R18.HAVC, Compile.Resource.R18 
{
}

# $buildType build for AutoCAD R19
Task Compile.Assembly.R19.Common -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\TianHuaCADApp.sln" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\TianHuaCADApp.sln" /p:Configuration="${buildType}-NET40" /t:rebuild
    }
}

Task Compile.Assembly.R19.Structure -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\TianHuaCAD.Structure.sln" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\TianHuaCAD.Structure.sln" /p:Configuration="${buildType}-NET40" /t:rebuild
    }
}

Task Compile.Assembly.R19.WSS -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\TianHuaCAD.WSS.sln" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\TianHuaCAD.WSS.sln" /p:Configuration="${buildType}-NET40" /t:rebuild
    }
}

Task Compile.Assembly.R19.HAVC -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\TianHuaCAD.HAVC.sln" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\TianHuaCAD.HAVC.sln" /p:Configuration="${buildType}-NET40" /t:rebuild
    }
}

Task Compile.Resource.R19 -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\Dark\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\Dark\" ".\ThCuiRes\ThCuiRes.vcxproj" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\Light\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\Light\" ".\ThCuiRes\ThCuiRes_light.vcxproj" /t:rebuild
    }
}

Task Build.Assembly.R19 -Depends Compile.Assembly.R19.Common, Compile.Assembly.R19.Structure, Compile.Assembly.R19.WSS, Compile.Assembly.R19.HAVC, Compile.Resource.R19 
{
    #
}

# $buildType build for AutoCAD R20
Task Compile.Assembly.R20.Common -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\TianHuaCADApp.sln" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\TianHuaCADApp.sln" /p:Configuration="${buildType}-NET45" /t:rebuild
    }
}

Task Compile.Assembly.R20.Structure -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\TianHuaCAD.Structure.sln" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\TianHuaCAD.Structure.sln" /p:Configuration="${buildType}-NET45" /t:rebuild
    }
}

Task Compile.Assembly.R20.WSS -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\TianHuaCAD.WSS.sln" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\TianHuaCAD.WSS.sln" /p:Configuration="${buildType}-NET45" /t:rebuild
    }
}

Task Compile.Assembly.R20.HAVC -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\TianHuaCAD.HAVC.sln" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\TianHuaCAD.HAVC.sln" /p:Configuration="${buildType}-NET45" /t:rebuild
    }
}

Task Compile.Resource.R20 -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\Dark\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\Dark\" ".\ThCuiRes\ThCuiRes.vcxproj" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\Light\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\Light\" ".\ThCuiRes\ThCuiRes_light.vcxproj" /t:rebuild
    }
}

Task Build.Assembly.R20 -Depends Compile.Assembly.R20.Common, Compile.Assembly.R20.Structure, Compile.Assembly.R20.WSS, Compile.Assembly.R20.HAVC, Compile.Resource.R20 
{
    #
}

# $buildType build for AutoCAD R22
Task Compile.Assembly.R22.Common -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET46\",IntermediateOutputPath="..\build\obj\${buildType}-NET46\" ".\TianHuaCADApp.sln" /p:Configuration="${buildType}-NET46" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET46\",IntermediateOutputPath="..\build\obj\${buildType}-NET46\" ".\TianHuaCADApp.sln" /p:Configuration="${buildType}-NET46" /t:rebuild
    }
}

Task Compile.Assembly.R22.Structure -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET46\",IntermediateOutputPath="..\build\obj\${buildType}-NET46\" ".\TianHuaCAD.Structure.sln" /p:Configuration="${buildType}-NET46" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET46\",IntermediateOutputPath="..\build\obj\${buildType}-NET46\" ".\TianHuaCAD.Structure.sln" /p:Configuration="${buildType}-NET46" /t:rebuild
    }
}

Task Compile.Assembly.R22.WSS -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET46\",IntermediateOutputPath="..\build\obj\${buildType}-NET46\" ".\TianHuaCAD.WSS.sln" /p:Configuration="${buildType}-NET46" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET46\",IntermediateOutputPath="..\build\obj\${buildType}-NET46\" ".\TianHuaCAD.WSS.sln" /p:Configuration="${buildType}-NET46" /t:rebuild
    }
}

Task Compile.Assembly.R22.HAVC -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET46\",IntermediateOutputPath="..\build\obj\${buildType}-NET46\" ".\TianHuaCAD.HAVC.sln" /p:Configuration="${buildType}-NET46" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET46\",IntermediateOutputPath="..\build\obj\${buildType}-NET46\" ".\TianHuaCAD.HAVC.sln" /p:Configuration="${buildType}-NET46" /t:rebuild
    }
}

Task Compile.Resource.R22 -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET46\Dark\",IntermediateOutputPath="..\build\obj\${buildType}-NET46\Dark\" ".\ThCuiRes\ThCuiRes.vcxproj" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET46\Light\",IntermediateOutputPath="..\build\obj\${buildType}-NET46\Light\" ".\ThCuiRes\ThCuiRes_light.vcxproj" /t:rebuild
    }
}

Task Build.Assembly.R22 -Depends Compile.Assembly.R22.Common, Compile.Assembly.R22.Structure, Compile.Assembly.R22.WSS, Compile.Assembly.R22.HAVC, Compile.Resource.R22 
{
    #
}

Task Requires.BuildType {
    if ($buildType -eq $null) {
        throw "No build type specified"
    }

    Write-Host "$buildType build confirmed"
}

Task Sign {
    exec { 
        Push-Location Sign
        .\sign.ps1
        Pop-Location
    }
}

# temporarily disable code sign
# $buildType build for ThCADPluginInstaller
Task Compile.Installer -Depends Requires.BuildType, Build.Assembly.R18, Build.Assembly.R19, Build.Assembly.R20, Build.Assembly.R22 {
    if ($buildType -eq $null) {
        throw "No build type specified"
    }
    exec {
        & $msbuildExe /verbosity:minimal ".\ThCADInstaller\ThCADInstaller.wixproj" /p:Configuration=$buildType /t:rebuild
    }
}
