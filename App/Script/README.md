# Script
Use the scripts without "-" in the name(gitpull, build, release)  
gitpull.ps1 run it will git clone a new project in "./", best place it in a empty folder  
build.ps1 run it located in */ExpressTools/App only  
release.ps1 have no limit of location
## gitpull  
**step**  
1:copy gitpull.ps1 to a new folder  
2:run it  
**Usage**  
.\gitpull url version  
**example**  
.\gitpull.ps1 https://github.com/shichongdong/ExpressTools 1.0.1  
  
## build  
**Usage**  
.\build buildtype (version)  
(The parameter version is not obligatory.If parameter with version, the build.ps1 will call script "bump-version.ps1")  
**example**  
.\build.ps1 release 1.0.1  
.\build.ps1 release  
  
## release  
**Usage**  
.\release dsaprivatefile appcast.xml releasenote msi  
**example**  
.\release.ps1 NetSparkle_DSA.priv appcast.xml release-note.md *.msi  
(Pulling file in powershell as parameters is permited)  
  
## other script(do the percise active)  
### bump-version  
**Usage**  
.\bump-version folder version version  
**example**  
.\bump-version.ps1 ./ 1.0.0 1.0.1
(Change the version 1.0.0 to 1.0.1, if an assembly version is not 1.0.0, it won't be changed.)
(The first version can be "all". If you do this, all the assemblies will be changed whater their versions are)  

### run-build(Install paske)  
**Usage**  
Invoke-psake .\run-build.ps1 -Task "${buildType}.Build", Compile.Installer  
**example**  
Invoke-psake .\run-build.ps1 -Task release.build, Compile.Installer
  
### run-nunit(Install paske)  
**Usage**  
Invoke-psake .\run-nunit.ps1 -Task "${buildType}.Build", Unit.Tests
**example**  
Invoke-psake .\run-nunit.ps1 -Task release.Build, Unit.Tests