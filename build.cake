//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=NUnit.Extension.NUnitV2Driver"
#tool "nuget:?package=NUnit.Extension.VSProjectLoader"
#tool "nuget:?package=NUnit.ConsoleRunner"
#tool "nuget:?package=NUnit.Extension.TeamCityEventListener"
#tool "nuget:?package=OctopusTools"
#tool "nuget:?package=JetBrains.dotCover.CommandLineTools"


//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");


///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
var isCIBuild			= !BuildSystem.IsLocalBuild;
var solutionPath        = "./Checkout.ApiClient.Net40.sln";
var testPath            = "./Checkout.ApiClient.Tests/bin/" + configuration + "/Tests.dll";
var buildArtifacts      = Directory("./artifacts");
var libs                = Directory("./packages/_lib");
//var coverageResult      = buildArtifacts + File("result.dcvr");

var gitVersionInfo = GitVersion(new GitVersionSettings {
	OutputType = GitVersionOutput.Json
});

var nugetVersion = gitVersionInfo.NuGetVersion;


///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup(context =>
{
	Information("Building Checkout NET Library v{0} with configuration {1}", nugetVersion, configuration);
});

Teardown(context =>
{
	Information("Finished running tasks. Test Path: {0}, config: {1}", testPath, configuration);
});

//////////////////////////////////////////////////////////////////////
//  PRIVATE TASKS
//////////////////////////////////////////////////////////////////////

Task("__Clean")
	.Does(() =>
	{
		CleanDirectories(new DirectoryPath[] { buildArtifacts, libs });

		DotNetBuild(solutionPath, settings => settings
			.SetConfiguration(configuration)
			.WithTarget("Clean")
			.SetVerbosity(Verbosity.Minimal));
	});

Task("__Restore")
	.Does(() =>
	{
		NuGetRestore(solutionPath);
	});

Task("__UpdateAssemblyVersionInformation")
	.WithCriteria(isCIBuild)
	.Does(() =>
	{
		GitVersion(new GitVersionSettings {
			UpdateAssemblyInfo = true, // Don't update all assembly files
			UpdateAssemblyInfoFilePath = "./src/shared/ApiClientAssemblyInfo.cs",
			OutputType = GitVersionOutput.BuildServer
		});

		Information("AssemblyVersion -> {0}", gitVersionInfo.AssemblySemVer);
		Information("AssemblyFileVersion -> {0}.0", gitVersionInfo.MajorMinorPatch);
		Information("AssemblyInformationalVersion -> {0}", gitVersionInfo.InformationalVersion);
	});

Task("__Build")
	.Does(() =>
	{
		var packagePath = string.Concat("\"", MakeAbsolute(buildArtifacts).FullPath, "\"");

		DotNetBuild(solutionPath, settings => settings
			.SetConfiguration(configuration)
			.WithTarget("Rebuild")
			.SetVerbosity(Verbosity.Minimal)
			.WithProperty("WarningLevel", "0")

			// There's no point running build twice so create the in the initial build
			.WithProperty("RunOctoPack", "true")
			.WithProperty("OctoPackPackageVersion", nugetVersion)
			.WithProperty("OctoPackPublishPackageToFileShare", packagePath)
			.WithProperty("WarningLevel", "0"));		
	});

Task("__Test")
	.Does(() =>
	{
		NUnit3(testPath, new NUnit3Settings {
			Configuration = configuration,
			NoHeader = true,
			Results = buildArtifacts + File("TestResults.xml"),
			OutputFile = buildArtifacts + File("TestOutput.txt"), // Have to do that to redirect Console logging
			TeamCity = isCIBuild
		});
	});



Task("__OctoPush")
	.WithCriteria(isCIBuild)
	.Does(() =>
	{
		var packages = GetFiles("./artifacts/*.nupkg");

		OctoPush(
			EnvironmentVariable("Octopus_Server"),
			EnvironmentVariable("Octopus_ApiKey"),
			packages,
			new OctopusPushSettings {
				ReplaceExisting = true
			}
		);
	});


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Package")
	.IsDependentOn("__Clean")
	.IsDependentOn("__Restore")
	.IsDependentOn("__UpdateAssemblyVersionInformation")
	.IsDependentOn("__Build")
    .IsDependentOn("__Test");


Task("Deploy")
	.IsDependentOn("Package")
	.IsDependentOn("__OctoPush");

Task("Default")
	.IsDependentOn("Package");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
RunTarget(target);