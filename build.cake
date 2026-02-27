
var target = Argument ("Target", "Default");
var configuration = Argument ("Configuration", "Release");
var buildNumber = Argument ("buildNumber", "");
var versionSuffix = string.IsNullOrEmpty(buildNumber) ? "" : $"ci.{buildNumber}";

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
        DotNetClean ("./src",
            new DotNetCleanSettings () {
            Configuration = configuration,
            });
    });

// Run dotnet restore to restore all package references.
Task ("Restore")
    .Does (() => {
        var settings = new DotNetRestoreSettings();
        DotNetRestore ("./src", settings);
    });

// Build using the build configuration specified as an argument.
Task ("Build")
    .Does (() => {
        var settings = new DotNetBuildSettings () {
            Configuration = configuration,
            NoRestore = true,
            MSBuildSettings = new DotNetMSBuildSettings {}
        };
        if (!string.IsNullOrEmpty(versionSuffix))
            settings.ArgumentCustomization = builder => builder.Append($"/p:VersionSuffix={versionSuffix}");
        DotNetBuild ("./src", settings);
    });


Task ("ConventionTest")
    .Does (() => {
        var projects = GetFiles ("./src/Nimbus.Tests.*/Nimbus.Tests.*.csproj");
        var settings = new DotNetTestSettings {
            NoBuild = true,
            NoRestore = true,
            Configuration = configuration,
            Filter = "Category=\"Convention\"",
            ResultsDirectory = packageDirectory.Path.Combine("ConventionTests"),
            Loggers = new []{"trx"},
        };
        foreach (var project in projects) {

            Information ("Testing project " + project);
            DotNetTest (project.FullPath, settings);
        }
    });

Task ("UnitTest")
    .Does (() => {
        var projects = GetFiles ("./src/Nimbus.Tests.*/Nimbus.Tests.*.csproj");
        var settings = new DotNetTestSettings {
            NoBuild = true,
            NoRestore = true,
            Configuration = configuration,
            Filter = "Category=\"UnitTest\"",
            ResultsDirectory = packageDirectory.Path.Combine("UnitTests"),
            Loggers = new []{"trx"},
        };
        foreach (var project in projects) {

            Information ("Testing project " + project);
            DotNetTest (project.FullPath, settings);
        }
    });

Task ("IntegrationTest")
    .Does (() => {
        var projects = GetFiles ("./src/Nimbus.Tests.*/Nimbus.Tests.*.csproj");
        var settings = new DotNetTestSettings {
            NoBuild = true,
            NoRestore = true,
            Configuration = configuration,
            Filter = "Category!=\"Convention\" & Category!=\"UnitTest\"",
            ResultsDirectory = packageDirectory.Path.Combine("IntegrationTests"),
            Loggers = new []{"trx"},
        };
        foreach (var project in projects) {

            Information ("Testing project " + project);
            DotNetTest (project.FullPath, settings);

        }
    });


Task ("CollectPackages")
    .Does(()=>{
        var packages = GetFiles ("./**/bin/Release/Nimbus*.nupkg")
            .Where(p => !p.FullPath.Contains("/Tests/") && !p.FullPath.Contains(".Tests."));
        CopyFiles(packages, packageDirectory);
    });

Task("Test")
    .IsDependentOn("ConventionTest")
    .IsDependentOn("UnitTest")
    ;


// A meta-task that runs all the steps to Build and Test the app
Task ("BuildAndTest")
    .IsDependentOn ("Clean")
    .IsDependentOn ("Restore")
    .IsDependentOn ("Build")
    .IsDependentOn ("Test")
    ;

// Build and collect packages without running tests — used by the release workflow
Task ("Package")
    .IsDependentOn ("Clean")
    .IsDependentOn ("Restore")
    .IsDependentOn ("Build")
    .IsDependentOn ("CollectPackages")
    ;

// Build + integration tests only (no unit/convention tests) — used by nightly workflow
Task ("NightlyTest")
    .IsDependentOn ("Clean")
    .IsDependentOn ("Restore")
    .IsDependentOn ("Build")
    .IsDependentOn ("IntegrationTest")
    ;

Task ("CI")
    .IsDependentOn ("BuildAndTest")
    .IsDependentOn ("IntegrationTest")
    .IsDependentOn ("CollectPackages")
    ;

Task ("FastCI")
    .IsDependentOn ("BuildAndTest")
    .IsDependentOn ("CollectPackages")
    ;

// The default task to run if none is explicitly specified. In this case, we want
// to run everything starting from Clean, all the way up to Publish.
Task ("Default")
    .IsDependentOn ("BuildAndTest")
    ;

// Executes the task specified in the target argument.
RunTarget (target);
