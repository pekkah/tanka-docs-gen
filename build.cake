#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0-beta0014&prerelease"
#addin "Cake.Npm"
#addin nuget:?package=Cake.DoInDirectory
#addin nuget:?package=Cake.Git

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");
var artifactsDir = Directory(Argument<string>("artifactsDir", "./artifacts"));
var publishDir = Directory(Argument<string>("publishDir", "./publish"));
var runtime = Argument<string>("runtime", "win-x64");
var sln = Argument<string>("sln", "./tanka-docs-gen.sln");
var gitName = Argument<string>("gitName");
var gitEmail = Argument<string>("gitEmail");
var gitCommit = Argument<bool>("gitCommit", false);

var projectFiles = GetFiles("./src/**/*.csproj").Select(f => f.FullPath);
var version = "0.0.0-dev";

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
        Information($"Version: {version}, FullSemVer: {result.FullSemVer}");
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
        Information("Cleaning gh-pages directory");
        DeleteDirectory("./gh-pages", new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });
            
        Information("Pruning worktrees");
        var exitCode = StartProcess("git", new ProcessSettings{ Arguments = "worktree prune" });
        if (exitCode != 0) {
            throw new InvalidOperationException("Failed to prune worktrees");
        }

        Information("Adding worktree gh-pages");
        exitCode = StartProcess("git", new ProcessSettings{ Arguments = "worktree add -B gh-pages gh-pages origin/gh-pages" });
        if (exitCode != 0) {
            throw new InvalidOperationException("Failed to add worktree for gh-pages");
        }

        Information("Generate docs");
        var settings = new DotNetCoreRunSettings
        {
            Framework = "netcoreapp2.2",
            Configuration = "Release"
        };

        DotNetCoreRun("./src/generateDocs", "", settings);

        if (gitCommit) 
        {
            Information("Committing and pushing gh-pages");
            DoInDirectory("./gh-pages", () =>
            {
                GitAddAll(".");
                GitCommit(".", gitName, gitEmail, $"docs: build {version}");
                exitCode = StartProcess("git", new ProcessSettings{ Arguments = "push" });
                if (exitCode != 0) {
                    throw new InvalidOperationException("Failed to push changes");
                }
            });  
        } else 
        {
            Information("Skipping committing to Git");
        }
    });

RunTarget(target);