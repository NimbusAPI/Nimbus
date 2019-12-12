var target = Argument ("Target", "Default");
var configuration = Argument ("Configuration", "Release");
var version = EnvironmentVariable ("BUILD_NUMBER") ?? Argument ("buildVersion", "0.0.0");
var nugetApiKey = EnvironmentVariable ("NUGET_API_KEY") ?? Argument ("nugetApiKey", "");

Information ($"Running target {target} in configuration {configuration} with version {version}");

var packageDirectory = Directory ("./dist_package");

// Deletes the contents of the Artifacts folder if it contains anything from a previous build.
Task ("Clean")
    .Does (() => {
        CleanDirectory (packageDirectory);
        DotNetCoreClean ("./src");
    });

// Run dotnet restore to restore all package references.
Task ("Restore")
    .Does (() => {
        var settings = new DotNetCoreRestoreSettings { };
        DotNetCoreRestore ("./src", settings);
    });

private IEnumerable<FilePath> GetShippableProjects () {
    var files = GetFiles ("./src/**/*.csproj").Where (f => !f.FullPath.Contains (".Tests.")).ToArray ();
    return files;
}

Task ("GenerateVersionFile")
    .Does (() => {
        foreach (var project in GetShippableProjects ()) {
            var directory = project.GetDirectory ();
            var projectName = project.GetFilenameWithoutExtension ().ToString ();

            var file = $"{directory}/AssemblyInfo.cs";
            Information ($"Writing {file} for project {projectName}");
            CreateAssemblyInfo (file, new AssemblyInfoSettings {
                Product = projectName,
                    Version = version,
                    FileVersion = version,
                    InformationalVersion = version,
            });

        }

    });

// Build using the build configuration specified as an argument.
Task ("Build")
    .Does (() => {
        DotNetCoreBuild ("./src",
            new DotNetCoreBuildSettings () {
                Configuration = configuration,
                    NoRestore = true
            });
    });

Task ("Test")
    .Does (() => {
        var projects = GetFiles ("./src/Nimbus.Tests.Unit/Nimbus.Tests.Unit.csproj");
        var settings = new DotNetCoreTestSettings {
            NoBuild = true,
            NoRestore = true,
            Configuration = configuration,
        };
        foreach (var project in projects) {

            Information ("Testing project " + project);
            DotNetCoreTest (project.FullPath, settings);
        }
    });

Task ("IntegrationTest")
    .Does (() => {
        var projects = GetFiles ("./src/Nimbus.Tests.Integration/Nimbus.Tests.Integration.csproj");
        var settings = new DotNetCoreTestSettings {
            NoBuild = true,
            NoRestore = true,
            Configuration = configuration,
        };
        foreach (var project in projects) {

            Information ("Testing project " + project);
            DotNetCoreTest (project.FullPath, settings);
        }
    });

Task ("BuildPackages")
    .Does (() => {
        var settings = new DotNetCorePackSettings {
        Configuration = configuration,
        NoBuild = true,
        OutputDirectory = packageDirectory,
        ArgumentCustomization = args => args
        .Append ($"/p:PackageVersion={version}")

        };
        foreach (var project in GetShippableProjects ()) {
            DotNetCorePack (project.ToString (), settings);
        }
    });

Task ("PushPackages")
    .Does (() => {

        if (!String.IsNullOrEmpty (nugetApiKey)) {
        var settings = new DotNetCoreNuGetPushSettings {
        ApiKey = nugetApiKey,
        Source = "https://www.myget.org/F/nimbusapi/api/v2/package"
            };
            var packages = GetFiles ($"{packageDirectory}/Nimbus.*.nupkg");
            foreach (var file in packages) {
                Information ($"Pushing package {file}");
                DotNetCoreNuGetPush ($"{file}", settings);
            }
        } else {
            Information ($"No Nuget keys found. Skipping step");
        }
    });

// A meta-task that runs all the steps to Build and Test the app
Task ("BuildAndTest")
    .IsDependentOn ("Clean")
    .IsDependentOn ("Restore")
    .IsDependentOn ("GenerateVersionFile")
    .IsDependentOn ("Build")
    .IsDependentOn ("Test")
    .IsDependentOn ("BuildPackages");
//.IsDependentOn("IntegrationTest");

// The default task to run if none is explicitly specified. In this case, we want
// to run everything starting from Clean, all the way up to Publish.
Task ("Default")
    .IsDependentOn ("BuildAndTest");

Task ("CI")
    .IsDependentOn ("GenerateVersionFile")
    .IsDependentOn ("Build")
    .IsDependentOn ("BuildPackages");

Task ("Init").Does (() => {
    //nothing
});

// Executes the task specified in the target argument.
RunTarget (target);