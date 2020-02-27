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

Task Requires.Dotfuscator {
    $script:dotfuscatorCli = resolve-path "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\Extensions\PreEmptiveSolutions\DotfuscatorCE\dotfuscatorCLI.exe"
    
    if ($dotfuscatorCli -ne $null)
    {
        Write-Host "Found dotfuscatorCLI here: $dotfuscatorCli"
    }
}

# $buildType build for AutoCAD R18
Task Compile.Assembly.R18 -Depends Requires.MSBuild {
    exec {
            & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\$buildType\",IntermediateOutputPath="..\build\obj\$buildType\" ".\TianHuaCADApp.sln" /p:Configuration=$buildType /t:restore
            & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\$buildType\",IntermediateOutputPath="..\build\obj\$buildType\" ".\TianHuaCADApp.sln" /p:Configuration=$buildType /t:rebuild
    }
}

Task Compile.Resource.R18 -Depends Requires.MSBuild {
    exec {
            & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\$buildType\Dark\",IntermediateOutputPath="..\build\obj\$buildType\Dark\" ".\ThCuiRes\ThCuiRes.vcxproj" /t:rebuild
            & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\$buildType\Light\",IntermediateOutputPath="..\build\obj\$buildType\Light\" ".\ThCuiRes\ThCuiRes_light.vcxproj" /t:rebuild
    }
}

Task Dotfuscator.Assembly.R18 -Depends Requires.Dotfuscator, Compile.Assembly.R18, Compile.Resource.R18 {
	if (($buildType -eq "Release") -and ($dotfuscatorCli -ne $null)) {
		exec {
			& $dotfuscatorCli ".\dotfuscator_config_${buildType}.xml"
		}	
	}
}

# $buildType build for AutoCAD R19
Task Compile.Assembly.R19 -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\TianHuaCADApp.sln" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\TianHuaCADApp.sln" /p:Configuration="${buildType}-NET40" /t:rebuild
    }
}

Task Compile.Resource.R19 -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\Dark\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\Dark\" ".\ThCuiRes\ThCuiRes.vcxproj" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\Light\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\Light\" ".\ThCuiRes\ThCuiRes_light.vcxproj" /t:rebuild
    }
}

Task Dotfuscator.Assembly.R19 -Depends Requires.Dotfuscator, Compile.Assembly.R19, Compile.Resource.R19 {
	if (($buildType -eq "Release") -and ($dotfuscatorCli -ne $null)) {
		exec {
			& $dotfuscatorCli ".\dotfuscator_config_${buildType}-NET40.xml"
		}	
	}
}

# $buildType build for AutoCAD R20
Task Compile.Assembly.R20 -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\TianHuaCADApp.sln" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\TianHuaCADApp.sln" /p:Configuration="${buildType}-NET45" /t:rebuild
    }
}

Task Compile.Resource.R20 -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\Dark\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\Dark\" ".\ThCuiRes\ThCuiRes.vcxproj" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\Light\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\Light\" ".\ThCuiRes\ThCuiRes_light.vcxproj" /t:rebuild
    }
}

Task Dotfuscator.Assembly.R20 -Depends Requires.Dotfuscator, Compile.Assembly.R20, Compile.Resource.R20 {
	if (($buildType -eq "Release") -and ($dotfuscatorCli -ne $null)) {
		exec {
			& $dotfuscatorCli ".\dotfuscator_config_${buildType}-NET45.xml"
		}	
	}
}

# $buildType build for AutoCAD R22
Task Compile.Assembly.R22 -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET46\",IntermediateOutputPath="..\build\obj\${buildType}-NET46\" ".\TianHuaCADApp.sln" /p:Configuration="${buildType}-NET46" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET46\",IntermediateOutputPath="..\build\obj\${buildType}-NET46\" ".\TianHuaCADApp.sln" /p:Configuration="${buildType}-NET46" /t:rebuild
    }
}

Task Compile.Resource.R22 -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET46\Dark\",IntermediateOutputPath="..\build\obj\${buildType}-NET46\Dark\" ".\ThCuiRes\ThCuiRes.vcxproj" /t:rebuild
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET46\Light\",IntermediateOutputPath="..\build\obj\${buildType}-NET46\Light\" ".\ThCuiRes\ThCuiRes_light.vcxproj" /t:rebuild
    }
}

Task Dotfuscator.Assembly.R22 -Depends Requires.Dotfuscator, Compile.Assembly.R22, Compile.Resource.R22 {
	if (($buildType -eq "Release") -and ($dotfuscatorCli -ne $null)) {
		exec {
			& $dotfuscatorCli ".\dotfuscator_config_${buildType}-NET46.xml"
		}	
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
        Push-Location Sign
        .\sign.ps1
        Pop-Location
    }
}

# temporarily disable code sign
# $buildType build for ThCADPluginInstaller
Task Compile.Installer -Depends Requires.BuildType, Dotfuscator.Assembly.R18, Dotfuscator.Assembly.R19, Dotfuscator.Assembly.R20, Dotfuscator.Assembly.R22 {
    if ($buildType -eq $null) {
        throw "No build type specified"
    }
    exec {
        & $msbuildExe /verbosity:minimal ".\ThCADInstaller\ThCADInstaller.wixproj" /p:Configuration=$buildType /t:rebuild
    }
}
