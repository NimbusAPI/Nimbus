var target = Argument ("Target", "Default");
var configuration = Argument ("Configuration", "Release");
var version = EnvironmentVariable ("BUILD_NUMBER") ?? Argument ("buildVersion", "0.0.0");
var nugetApiKey = EnvironmentVariable ("NUGET_API_KEY") ?? Argument ("nugetApiKey", "");

Information ($"Running target {target} in configuration {configuration} with version {version}");

var packageDirectory = Directory ("./dist_package");

Task ("Init").Does (() => {
    //nothing
});

private IEnumerable<FilePath> GetAllProjects () {
    var files = GetFiles ("./src/**/*.csproj").ToArray ();
    return files;
}

// Deletes the contents of the Artifacts folder if it contains anything from a previous build.
Task ("Clean")
    .Does (() => {
        CleanDirectory (packageDirectory);
        DotNetCoreClean ("./src",
            new DotNetCoreCleanSettings () {
            Configuration = configuration,
            ArgumentCustomization = builder => builder
                .Append($"/p:Version={version}")
            });
    });

// Run dotnet restore to restore all package references.
Task ("Restore")
    .Does (() => {
        var settings = new DotNetCoreRestoreSettings();
        DotNetCoreRestore ("./src", settings);
    });

// Build using the build configuration specified as an argument.
Task ("Build")
    .IsDependentOn ("Restore")
    .Does (() => {
        DotNetCoreBuild ("./src",
            new DotNetCoreBuildSettings () {
                Configuration = configuration,
                ArgumentCustomization = builder => builder
                    .Append($"/p:Version={version}"),
                NoRestore = true,
                MSBuildSettings = new DotNetCoreMSBuildSettings {
                }
            });
    });

Task ("ConventionTest")
    .Does (() => {
        var projects = GetFiles ("./src/Nimbus.Tests.*/Nimbus.Tests.*.csproj");
        var settings = new DotNetCoreTestSettings {
            NoBuild = true,
            NoRestore = true,
            Configuration = configuration,
            Filter = "Category=\"Convention\""
        };
        foreach (var project in projects) {

            Information ("Testing project " + project);
            DotNetCoreTest (project.FullPath, settings);
        }
    });

Task ("UnitTest")
    .Does (() => {
        var projects = GetFiles ("./src/Nimbus.Tests.*/Nimbus.Tests.*.csproj");
        var settings = new DotNetCoreTestSettings {
            NoBuild = true,
            NoRestore = true,
            Configuration = configuration,
            Filter = "Category=\"UnitTest\""
        };
        foreach (var project in projects) {

            Information ("Testing project " + project);
            DotNetCoreTest (project.FullPath, settings);
        }
    });

Task ("IntegrationTest")
    .Does (() => {
        var projects = GetFiles ("./src/Nimbus.Tests.*/Nimbus.Tests.*.csproj");
        var settings = new DotNetCoreTestSettings {
            NoBuild = true,
            NoRestore = true,
            Configuration = configuration,
            Filter = "Category!=\"Convention\" & Category!=\"UnitTest\""
        };
        foreach (var project in projects) {

            Information ("Testing project " + project);
            DotNetCoreTest (project.FullPath, settings);
        }
    });

Task("Test")
    .IsDependentOn("ConventionTest")
    .IsDependentOn("UnitTest")
    ;

Task ("PushPackages")
    .Does (() => {

        if (String.IsNullOrEmpty (nugetApiKey)) {
            Information ($"No Nuget keys found. Skipping step");
            return;
        }

        var settings = new DotNetCoreNuGetPushSettings {
            ApiKey = nugetApiKey,
            Source = "https://www.myget.org/F/nimbusapi/api/v2/package"
        };
        var packages = GetFiles ($"./**/bin/Release/Nimbus.*.{version}.nupkg");
        foreach (var file in packages) {
            Information ($"Pushing package {file}");
            DotNetCoreNuGetPush ($"{file}", settings);
        }
        packages = GetFiles ($"./**/bin/Release/Nimbus.{version}.nupkg");
        foreach (var file in packages) {
            Information ($"Pushing package {file}");
            DotNetCoreNuGetPush ($"{file}", settings);
        }
        
    });

// A meta-task that runs all the steps to Build and Test the app
Task ("BuildAndTest")
    .IsDependentOn ("Clean")
    .IsDependentOn ("Build")
    .IsDependentOn ("Test")
    .IsDependentOn ("IntegrationTest")
    ;

Task ("CI")
    .IsDependentOn ("Build")
    ;

// The default task to run if none is explicitly specified. In this case, we want
// to run everything starting from Clean, all the way up to Publish.
Task ("Default")
    .IsDependentOn ("BuildAndTest")
    ;

// Executes the task specified in the target argument.
RunTarget (target);