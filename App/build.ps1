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
			& $msbuildExe ".\TianHuaCADApp.sln" /p:Configuration=Release /t:restore
			& $msbuildExe ".\ThCui\ThCui.csproj" /p:Configuration=Release /t:rebuild
			& $msbuildExe ".\ThParking\ThParking.csproj" /p:Configuration=Release /t:rebuild
    }
}

# Release build for AutoCAD R19
Task Compile.Assembly.R19 -Depends Requires.MSBuild {
	exec { 
		& $msbuildExe ".\TianHuaCADApp.sln" /p:Configuration=Release-NET40 /t:restore
		& $msbuildExe ".\ThCui\ThCui.csproj" /p:Configuration=Release-NET40 /t:rebuild
		& $msbuildExe ".\ThParking\ThParking.csproj" /p:Configuration=Release-NET40 /t:rebuild
	}
}

# Release build for AutoCAD R20
Task Compile.Assembly.R20 -Depends Requires.MSBuild {
	exec { 
		& $msbuildExe ".\TianHuaCADApp.sln" /p:Configuration=Release-NET45 /t:restore
		& $msbuildExe ".\ThCui\ThCui.csproj" /p:Configuration=Release-NET45 /t:build
		& $msbuildExe ".\ThParking\ThParking.csproj" /p:Configuration=Release-NET45 /t:build
	}
}

Task Compile.Assembly -Depends Compile.Assembly.R18, Compile.Assembly.R19, Compile.Assembly.R20 {
}