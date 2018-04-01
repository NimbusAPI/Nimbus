// Tools
#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=OctopusTools"

// Namespaces
using System.Xml;
using System.Xml.Linq;

// Build Configuration
var configuration = Argument("configuration", "Release");

// Build Target
var target = Argument("target", "Default");

// Local Variables..
var isContinuousIntegrationBuild = !BuildSystem.IsLocalBuild;

// Git Version \\(^_^)//
var gitVersionInfo = GitVersion(new GitVersionSettings {
    OutputType = GitVersionOutput.Json,
    UpdateAssemblyInfo = false
});

// What branch are we on for conditional Tasks?
var isMasterBranch = gitVersionInfo.BranchName == "master" ? true : false;

// Package Version
var nugetVersion = gitVersionInfo.NuGetVersion;

// Artifacts Directory
EnsureDirectoryExists("./artifacts");

/********************************************************************
 * Actual Build Steps
 *******************************************************************/
Task("Clean")
    .Does(() =>
    {
        // Build Artifacts
        CleanDirectories("./src/**/bin");
        CleanDirectories("./src/**/obj");

        // Testing Artifacts
        CleanDirectories("./test/**/bin");
        CleanDirectories("./test/**/obj");
        DeleteFiles("./test/**/TestResult.xml");
    }
);

Task("Build")
    .IsDependentOn("SetVersion")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        var buildSettings = new DotNetCoreBuildSettings {
            Configuration = configuration
        };

        switch((int)Environment.OSVersion.Platform) {
            // Unix
            case 4: 
            {
                Information("Building on {0}", Environment.OSVersion.Platform);
                
                // Set Mono Reference Libraries. 
                buildSettings.ArgumentCustomization = args => args.Append("/p:MonoReferenceAssemblies=/usr/lib/mono");
                break;
            }
            
            // Assumed Windows.. 
            default: 
            {
                Information("Building on {0}", Environment.OSVersion.Platform);
                break;
            }
        }

        DotNetCoreBuild("./", buildSettings);
    }
);

Task("SetVersion")
    .Does(() =>
    {
        var projects = GetFiles("./src/**/*.csproj");

        foreach(var project in projects)
        {
            var document = XDocument.Load(project.FullPath);
            var propertyGroup = document.Descendants("PropertyGroup")
                .FirstOrDefault();
            var versionPrefix = propertyGroup.Descendants("VersionPrefix")
                .FirstOrDefault();
            var versionSuffix = propertyGroup.Descendants("VersionSuffix")
                .FirstOrDefault();

            if (versionPrefix != null)
            {
                Information("Version Prefix is present, Setting {0}", gitVersionInfo.MajorMinorPatch);
                versionPrefix.SetValue(gitVersionInfo.MajorMinorPatch);
            } else
            {
                Information("Version Prefix is not present, Writing {0}", gitVersionInfo.MajorMinorPatch);
                propertyGroup.SetElementValue("VersionPrefix", gitVersionInfo.MajorMinorPatch);
            }

            if (versionSuffix != null)
            {
                Information("Version Suffix is present, Setting {0}", gitVersionInfo.PreReleaseTag);
                versionSuffix.SetValue(gitVersionInfo.PreReleaseTag);
            }
            else
            {
                Information("Version Suffix is not present, Writing {0}", gitVersionInfo.PreReleaseTag);
                propertyGroup.SetElementValue("VersionSuffix", gitVersionInfo.PreReleaseTag);
            }

            document.Save(project.FullPath);
        }
    }
);

Task("Restore")
    .Does(() =>
    {
        DotNetCoreRestore();
    }
);

Task("Test")
    .Does(() =>
    {
        var testProjects = GetFiles("./test/**/*.csproj");

        foreach(var testProject in testProjects)
        {
            DotNetCoreTest(testProject.FullPath);
        }
    });

Task("Pack")
    .WithCriteria(isContinuousIntegrationBuild)
    .Does(() =>
    {
        var projects = GetFiles("./src/**/*.csproj");

        foreach(var project in projects)
        {
            DotNetCorePack(project.FullPath, new DotNetCorePackSettings
            {
                Configuration = configuration,
            });
        }
    });

/********************************************************************
 * Default Build Target
 *******************************************************************/
Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Pack")
    .Does(() => {
        Information("Finished building branch: {0}. Package version: {1}", gitVersionInfo.BranchName, nugetVersion);
    });

RunTarget(target);