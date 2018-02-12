Sms.Utilities.BuildTargets.NuGet
================================

IMPORTANT: Remember to add the "build" folder to source control.

The package contents have been moved to a "build" folder at the 
solution level. 

*	Edit the build.targets file as needed. For instance, 
	the project to be packaged needs to be set.
*	Create a .nuspec file for the published project.
	(Note: after modifying build.targets, you can create the nuspec
	by running msbuild build.targets /t:nuspec
	

Builds BOTH Solution and Project
---------------------------
By default, the entire solution is built using default output folders. Then,
the project being published is built using the build/BIN output folder.

Targets
-------
There are three build targets. They do NOT depend on each other. For example,
running package does NOT run build.
*	build
*	package
*	publish

So, to run all three:
msbuild build.targets /t:build;package;publish

The default is to run the Release configuration. This can be overridden from the command line,
without modifying build.targets.

msbuild build.targets /t:build /p:Configuration=Stage

Build Folder
------------
build.targets outputs the project to be published to the build\Bin folder, 
not the default output folders, so that it's easy to package
and publish. This is controlled in the Build target via the OutDir property.

    <Message Text="Build the project that will be published, using the custom BIN folder" />
    <MSBuild
      Projects="$(ProjectToPublish)"
      Targets="Rebuild"
      Properties="OutDir=%(BinDir.FullPath);Configuration=$(Configuration);Platform=$(Platform)"

Likewise, NuGet pack overrides the OutputPath property from the project file so that it gets the built files
from the build/Bin folder.

<PackageCommand>"$(NuGetExe)" pack "$(NuGetFileToPack)" -o $(NuGetPackageDir) -p OutputPath="@(BinDir->'%(FullPath)')"</PackageCommand>


Overwriting a NuGet Package
---------------------------
By default, NuGet packages are not overwritten. To do so, call 
msbuild build.targets /t:publish /p:OverwritePackage=true.
