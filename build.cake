var target = Argument("Target", "Default");  
var configuration = Argument("Configuration", "Release");

var buildNumber = Argument("buildNumber", "0");
var version = string.Format("1.0.0.{0}", buildNumber);

var packageId = "";

var packageDirectory = "./artifacts";

Information($"Running target {target} in configuration {configuration}");


// Deletes the contents of the Artifacts folder if it contains anything from a previous build.
Task("Clean")  
    .Does(() =>
    {
        DotNetCoreClean("./");
        CleanDirectory(packageDirectory);
    });

// Run dotnet restore to restore all package references.
Task("Restore")  
    .Does(() =>
    {
        var settings = new DotNetCoreRestoreSettings{
        };
        DotNetCoreRestore("./", settings);
    });


// Build using the build configuration specified as an argument.
 Task("Build")
    .Does(() =>
    {
        DotNetCoreBuild(".",
            new DotNetCoreBuildSettings()
            {
                Configuration = configuration,
                NoRestore = true
            });
    });

// Look under a 'Tests' folder and run dotnet test against all of those projects.
// Then drop the XML test results file in the Artifacts folder at the root.
Task("Test")  
    .Does(() =>
    {
        var projects = GetFiles("./tests/Nimbus.UnitTests/Nimbus.UnitTests.csproj");
        var settings = new DotNetCoreTestSettings
            {
                NoBuild = true,
                NoRestore = true,
                Configuration = configuration,
            };
        foreach(var project in projects)
        {
            
            Information("Testing project " + project);
            DotNetCoreTest(project.FullPath, settings);
        }
    });

Task("IntegrationTest")  
    .Does(() =>
    {
        var projects = GetFiles("./tests/Nimbus.IntegrationTests/Nimbus.IntegrationTests.csproj");
        var settings = new DotNetCoreTestSettings
            {
                NoBuild = true,
                NoRestore = true,
                Configuration = configuration,
            };
        foreach(var project in projects)
        {
            
            Information("Testing project " + project);
            DotNetCoreTest(project.FullPath, settings);
        }
    });


// Publish th

// A meta-task that runs all the steps to Build and Test the app
Task("BuildAndTest")  
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    //.IsDependentOn("GenerateVersionFile")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

// The default task to run if none is explicitly specified. In this case, we want
// to run everything starting from Clean, all the way up to Publish.
Task("Default")  
    .IsDependentOn("BuildAndTest");

// Executes the task specified in the target argument.
RunTarget(target); 
