# Script
Use the scripts without "-" in the name(gitpull, build, release)  
gitpull.ps1 Run it will git clone a new project in "./", best place it in a empty folder  
build.ps1 Run it located in */ExpressTools/App only  
release.ps1 It have no limit of location  
## gitpull  
**step**  
1:Copy gitpull.ps1 to a new folder  
2:Run it  
**usage**  
.\gitpull url version  
**example**  
.\gitpull.ps1 https://github.com/shichongdong/ExpressTools 1.0.1  
  
## build  
**usage**  
.\build buildtype (version)  
(The parameter version is not obligatory.If parameter with version, the build.ps1 will call script "bump-version.ps1")  
**example**  
.\build.ps1 release 1.0.1  
.\build.ps1 release  
  
## pubilsh  
**usage**  
.\pubilsh msi  
**example**  
.\pubilsh.ps1 *.msi  
(Pulling file in powershell as parameters is permited)  
  
## other script(do the percise active)  
### bump-version  
**usage**  
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
**usage**  
Invoke-psake .\run-nunit.ps1 -Task "${buildType}.Build", Unit.Tests
**example**  
Invoke-psake .\run-nunit.ps1 -Task release.Build, Unit.Tests

### run-harness(Install paske)  
**usage**  
Invoke-psake .\run-harness.ps1 -Task "${buildType}.Build", Gallio.Tests
**example**  
Invoke-psake .\run-harness.ps1 -Task release.Build, Gallio.Tests
