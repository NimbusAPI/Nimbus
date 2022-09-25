
var target = Argument ("Target", "Default");
var configuration = Argument ("Configuration", "Release");
var version = EnvironmentVariable ("GITVERSION_NUGETVERSIONV2") ?? Argument ("buildVersion", "0.0.0");
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
        DotNetClean ("./src",
            new DotNetCleanSettings () {
            Configuration = configuration,
            ArgumentCustomization = builder => builder
                .Append($"/p:Version={version}")
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
        DotNetBuild ("./src",
            new DotNetBuildSettings () {
                Configuration = configuration,
                ArgumentCustomization = builder => builder
                    .Append($"/p:Version={version}"),
                NoRestore = true,
                MSBuildSettings = new DotNetMSBuildSettings {
                }
            });
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

Task("Test")
    .IsDependentOn("ConventionTest")
    .IsDependentOn("UnitTest")
    ;

Task ("CollectPackages")
    .Does(()=>{
        var packages = GetFiles ($"./**/bin/Release/Nimbus.*.{version}.nupkg");
        CopyFiles(packages, packageDirectory);
    });

// A meta-task that runs all the steps to Build and Test the app
Task ("BuildAndTest")
    .IsDependentOn ("Clean")
    .IsDependentOn ("Restore")
    .IsDependentOn ("Build")
    .IsDependentOn ("Test")
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