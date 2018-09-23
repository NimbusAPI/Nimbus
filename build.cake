var target = Argument("Target", "Default");  
var configuration = Argument("Configuration", "Release");

var buildNumber = Argument("buildNumber", "0");
var version = string.Format("1.0.0.{0}", buildNumber);

var packageId = "";

var distDirectory = "./dist";
var packageDirectory = "./artifacts";

Information($"Running target {target} in configuration {configuration}");


// Deletes the contents of the Artifacts folder if it contains anything from a previous build.
Task("Clean")  
    .Does(() =>
    {
        CleanDirectory(distDirectory);
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
                ArgumentCustomization = args => args.Append("--no-restore"),
            });
    });

// Look under a 'Tests' folder and run dotnet test against all of those projects.
// Then drop the XML test results file in the Artifacts folder at the root.
Task("Test")  
    .Does(() =>
    {
        var projects = GetFiles("./tests/Nimbus.UnitTests/Nimbus.UnitTests.csproj");
        foreach(var project in projects)
        {
            Information("Testing project " + project);
            DotNetCoreTest(project.FullPath);
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

Task("Package")
    .Does(() => 
    {
        

        StartProcess("dotnet", new ProcessSettings {
            Arguments = new ProcessArgumentBuilder()
                .Append("octo")
                .Append("pack")
                .Append($"--id={packageId}")
                .Append($"--version={version}")
                .Append($"--basePath=\"{distDirectory}\"")
                .Append($"--outFolder=\"{packageDirectory}\"")
            }
        );
    });

Task("PushPackages")
	.IsDependentOn("Package")
	.Does(() => {
		if (HasEnvironmentVariable("octopusurl"))
		{
			var server = EnvironmentVariable("octopusurl");
			var apikey = EnvironmentVariable("octopusapikey");
            var package = $"{packageDirectory}/{packageId}.{version}.nupkg";
			
            Information($"The Octopus variable was present. {server}");

            StartProcess("dotnet", new ProcessSettings {
                        Arguments = new ProcessArgumentBuilder()
                            .Append("octo")
                            .Append("push")
                            .Append($"--package={package}")
                            .Append($"--server=\"{server}\"")
                            .Append($"--apiKey=\"{apikey}\"")
                        }
                    );
		}
        else
        {
            Information("No Octopus variables present.");
        }

	});

Task("CI")
    .IsDependentOn("BuildAndTest")
    .IsDependentOn("Package")
    .IsDependentOn("PushPackages");

// Executes the task specified in the target argument.
RunTarget(target); 
