var target = Argument("Target", "Default");  
var configuration = Argument("Configuration", "Release");
var version = EnvironmentVariable("BUILD_NUMBER") ?? Argument("buildVersion", "0.0.0");
var nugetApiKey = EnvironmentVariable("NUGET_API_KEY") ?? "";

Information($"Running target {target} in configuration {configuration} with version {version}");

var packageDirectory = Directory("./dist_package");


// Deletes the contents of the Artifacts folder if it contains anything from a previous build.
Task("Clean")  
    .Does(() =>
    {
        CleanDirectory(packageDirectory);
        DotNetCoreClean("./");
    });

// Run dotnet restore to restore all package references.
Task("Restore")  
    .Does(() =>
    {
        var settings = new DotNetCoreRestoreSettings{
        };
        DotNetCoreRestore("./", settings);
    });


Task("GenerateVersionFile")
    .Does(() =>
    {
        var file = "./src/Nimbus/AssemblyInfo.cs";
        CreateAssemblyInfo(file, new AssemblyInfoSettings {
            Product = "Nimbus",
            Version = version,
            FileVersion = version,
            InformationalVersion = version,
        });
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



Task("BuildPackages")
    .Does(()=>
    {
        var settings = new DotNetCorePackSettings
        {
            Configuration = configuration,
            NoBuild = true,
            OutputDirectory = packageDirectory,
            ArgumentCustomization = args => args
                        .Append($"/p:PackageVersion={version}")
        
        };
       DotNetCorePack("./src/Nimbus/Nimbus.csproj", settings); 
    });



Task("PushPackages")
	.Does(() => {

        var package = $"./{packageDirectory}/Nimbus.{version}.nupkg";
        if (! System.IO.File.Exists(package))
        {
            Information($"File {package} doesn't exist");
        }
        if ( !String.IsNullOrEmpty(nugetApiKey))
        {
            var settings = new DotNetCoreNuGetPushSettings
            {
                ApiKey = nugetApiKey,
                Source = "https://www.nuget.org/api/v2/package"
            };
            Information($"Pushing package {package}");
            DotNetCoreNuGetPush($"{package}", settings);
        }
        else
        {
            Information($"No Nuget keys found. Skipping package {package}");
        }
	});



// A meta-task that runs all the steps to Build and Test the app
Task("BuildAndTest")  
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("GenerateVersionFile")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("BuildPackages")
    .IsDependentOn("IntegrationTest");

// The default task to run if none is explicitly specified. In this case, we want
// to run everything starting from Clean, all the way up to Publish.
Task("Default")  
    .IsDependentOn("BuildAndTest");

Task("CI")
    .IsDependentOn("GenerateVersionFile")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("BuildPackages");

Task("Init").Does(()=>{
    //nothing
});

// Executes the task specified in the target argument.
RunTarget(target); 
