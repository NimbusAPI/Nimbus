/**********************************************************************
 * Tools
 **********************************************************************/
#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0"

/**********************************************************************
 * Arguments
 **********************************************************************/
// Build Configuration
var configuration = Argument("configuration", "Release");

// Build Targets
var target = Argument("target", "Default");


/**********************************************************************
 * Global Variables
 **********************************************************************/

// Are we on a Build Server
var isContinuousIntegrationBuild = !BuildSystem.IsLocalBuild;

// Nuget Server Settings
var nugetKey = EnvironmentVariable("PROGET_KEY") ?? "NO BUENOS";
var nugetServer = EnvironmentVariable("PROGET_SERVERURL") ?? "NO BUENOS";

// Octopus API Key & Server Url
var octopusDeployApiKey = EnvironmentVariable("OCTOPUS_API") ?? "NO BUENOS";
var octopusDeployServer = EnvironmentVariable("OCTOPUS_SERVERURL") ?? "NO BUENOS";

GitVersion gitVersionInfo;
string nugetVersion = "1.0.0";
string informationalVersion = "1.0.0";
bool isMasterBranch = false;

try {
    // Git Version \\(^_^)//
    gitVersionInfo = GitVersion(new GitVersionSettings {
        OutputType = GitVersionOutput.Json
    });

    // Package Version
    nugetVersion = gitVersionInfo.NuGetVersion;
    informationalVersion = gitVersionInfo.InformationalVersion;
    isMasterBranch = gitVersionInfo.BranchName == "master";
}
catch
{
    Error("Could not set GitVersionInfo");
}

/**********************************************************************
 * Artifacts
 **********************************************************************/

// Artifacts Directory
EnsureDirectoryExists("./artifacts");
var artifactsDirectory = Directory("./artifacts");

/**********************************************************************
 * Setup Tasks
 **********************************************************************/
Setup(context => {
    if (gitVersionInfo != null)
    {
        Information("Building Version {0} on {1}", nugetVersion, gitVersionInfo.BranchName);
    }
    else
    {
        Information("Building an unknown version");
    }
});

Teardown(context => {
    if (gitVersionInfo != null)
    {
        Information("Finished building Version {0} on {1}", nugetVersion, gitVersionInfo.BranchName);
    }
    else
    {
        Information("Finished building an unknown version");
    }
});


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
    .Does(() =>
    {
        DotNetCoreBuild("./", new DotNetCoreBuildSettings {
            Configuration = configuration,
            ArgumentCustomization = args =>
                args.Append($"/p:Version={nugetVersion}")
                    .Append($"/p:InformationalVersion={informationalVersion}")
        });
    });

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
                NoBuild = true,
                Configuration = configuration,
                OutputDirectory = artifactsDirectory,
                ArgumentCustomization = args =>
                    args.Append($"/p:Version={nugetVersion}")
                        .Append($"/p:InformationalVersion={informationalVersion}")
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