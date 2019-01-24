#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0-beta0014&prerelease"
#addin "Cake.Npm"

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");
var artifactsDir = Directory(Argument<string>("artifactsDir", "./artifacts"));
var publishDir = Directory(Argument<string>("publishDir", "./publish"));
var runtime = Argument<string>("runtime", "win-x64");
var sln = Argument<string>("sln", "./tanka-docs-gen.sln");

var projectFiles = GetFiles("./src/**/*.csproj").Select(f => f.FullPath);
var version = "0.0.0-dev";
var preRelease = true;

Task("Default")
  .IsDependentOn("SetVersion")
  .IsDependentOn("Pack")
  .IsDependentOn("Docs");

Task("Publish")
  .IsDependentOn("Build")
  .Does(()=>
  {
      var settings = new DotNetCorePublishSettings
      {
          //Framework = framework,
          Configuration = configuration,
          OutputDirectory = publishDir,
          Runtime = runtime
      };

      foreach(var projectFile in projectFiles)
      {
        DotNetCorePublish(projectFile, settings);
      }
  });

Task("Pack")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .Does(()=>
  {
      Information($"Pack to: {artifactsDir}");
      var buildSettings = new DotNetCoreMSBuildSettings();
      buildSettings.SetVersion(version);
      var settings = new DotNetCorePackSettings
      {
          Configuration = configuration,
          OutputDirectory = artifactsDir,
          IncludeSymbols = true,
          MSBuildSettings = buildSettings
      };

      foreach(var projectFile in projectFiles)
      {
        DotNetCorePack(projectFile, settings);
      }
  });

Task("Build")
  .IsDependentOn("Clean")
  .IsDependentOn("Restore")
  .Does(() =>
  {
      var settings = new DotNetCoreBuildSettings
      {
          Configuration = configuration
      };

      foreach(var projectFile in projectFiles)
      {
        DotNetCoreBuild(projectFile, settings);
      }
  });

Task("Clean")
  .Does(()=>
  {
      Information($"Cleaning: {artifactsDir}");
      CleanDirectory(artifactsDir);
      Information($"Cleaning: {publishDir}");
      CleanDirectory(publishDir);
  });

Task("Restore")
  .Does(()=>
  {
      foreach(var projectFile in projectFiles)
      {
        DotNetCoreRestore(projectFile);
      }
  });

Task("SetVersion")
    .Does(()=> {
        var result = GitVersion();
        
        version = result.SemVer;
        preRelease = result.PreReleaseNumber.HasValue;
        Information($"Version: {version}, FullSemVer: {result.FullSemVer}, PreRelease: {preRelease}");
        Information($"##vso[build.updatebuildnumber]{version}");
    });

Task("Test")
  .IsDependentOn("Build")
  .Does(()=> {
      var projectFiles = GetFiles("./tests/**/*Tests.csproj");
      var settings = new DotNetCoreTestSettings()
      {
         ResultsDirectory = new DirectoryPath(artifactsDir),
         Logger = "trx"
      };

      foreach(var file in projectFiles)
      {
          DotNetCoreTest(file.FullPath, settings);
      }
    });

Task("Docs")
    .IsDependentOn("SetVersion")
    .Does(()=> {
        Information("Generate docs");
        var settings = new DotNetCoreRunSettings
        {
            Framework = "netcoreapp2.2",
            Configuration = "Release"
        };

        var targetFolder = $"{artifactsDir}\\gh-pages";
        var basepath = "/tanka-docs-gen/";
        if (preRelease)
        {
            targetFolder += "\\beta";
            basepath += "/beta/";
        }

        DotNetCoreRun(
            "./src/generateDocs", 
            $"--output=\"{targetFolder}\" --basepath=\"{basepath}\"", 
            settings);
    });

RunTarget(target);
