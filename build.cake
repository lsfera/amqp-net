var target = Argument<string>("target");
var buildConfiguration = Argument<string>("buildConfiguration");
var buildVerbosity = (DotNetCoreVerbosity)Enum.Parse(typeof(DotNetCoreVerbosity), Argument<string>("buildVerbosity"));
var softwareVersion = Argument<string>("softwareVersion");
var buildNumber = Argument<int>("buildNumber");
var commitHash = Argument<string>("commitHash");
var nuGetApiKey = Argument<string>("nuGetApiKey");

var srcFolder = new DirectoryInfo(".\\src");
var solutionFile = srcFolder
    .GetFiles("*.sln")
    .Select(x => x.FullName)
    .First();
var projectFolders = srcFolder
    .GetDirectories()
    .Where(x => x.GetFiles("*.csproj").Length > 0)
    .Select(x => x.FullName);
var codeFolders = projectFolders.Where(x => !x.Contains("Tests"));
var testFolders = projectFolders.Except(codeFolders);
var projectToPack = "Amqp.Net.Client";
var artifactsDirectory = ".\\artifacts";

Task("Clean")
    .Does(() =>
    {
        foreach (var folder in projectFolders)
        {
            Information(folder);
            var cleanSettings = new DotNetCoreCleanSettings
            {
                MSBuildSettings = new DotNetCoreMSBuildSettings { NoLogo = true },
                Verbosity = buildVerbosity
            };
            DotNetCoreClean(folder, cleanSettings);
        }
    });

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        foreach (var folder in projectFolders)
        {
            Information(folder);
            var buildSettings = new DotNetCoreBuildSettings
            {
                MSBuildSettings = new DotNetCoreMSBuildSettings { NoLogo = true },
                Configuration = buildConfiguration,
                Verbosity = buildVerbosity
            };
            DotNetCoreBuild(folder, buildSettings);
        }
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        foreach (var folder in testFolders)
        {
            Information(folder);
            var testSettings = new DotNetCoreTestSettings { NoBuild = true, Configuration = buildConfiguration, Verbosity = buildVerbosity };
            DotNetCoreTest(folder, testSettings);
        }
    });

Task("Pack")
    .IsDependentOn("Test")
    .Does(() =>
    {
        var codeToPackFolder = codeFolders.Single(x => x.Contains(projectToPack));
        
        var content = System.IO.File.ReadAllText(String.Concat(codeToPackFolder, "\\", projectToPack, ".csproj"));
        var document = new System.Xml.XmlDocument();
        document.LoadXml(content);
        var csprojVersion = document.DocumentElement["PropertyGroup"]["Version"].InnerText.Split(new [] { '-' }, 2, StringSplitOptions.RemoveEmptyEntries);

        var providedVersion = softwareVersion.Split(new [] { '-' }, 2, StringSplitOptions.RemoveEmptyEntries);

        var versionPrefix = providedVersion.Length <= 0 ? csprojVersion[0].Split('.') : providedVersion[0].Split('.');
        var versionSuffix = (providedVersion.Length <= 1 ? csprojVersion[1] : providedVersion[1]).Replace("commitHash", commitHash);
        
        // AssemblyVersion
        // 1.0.0
        // 
        // AssemblyFileVersion
        // 1.2.3.<BUILD_NUMBER>
        // 
        // AssemblyInformationalVersion
        // 1.2.3(-alpha-commitHash)
        var assemblyVersion = String.Format("{0}.{0}.{0}.{0}", versionPrefix[0]);
        var assemblyFileVersion = String.Format("{0}.{1}.{2}.{3}", versionPrefix[0], versionPrefix[1], versionPrefix[2], buildNumber);
        var assemblyInformationalVersion = String.Format("{0}.{1}.{2}-{3}", versionPrefix[0], versionPrefix[1], versionPrefix[2], versionSuffix);

        Information("AssemblyVersion: {0}", assemblyVersion);
        Information("AssemblyFileVersion: {0}", assemblyFileVersion);
        Information("AssemblyInformationalVersion/NuGet package version: {0}", assemblyInformationalVersion);

        CreateDirectory(artifactsDirectory);
        var packSettings = new DotNetCorePackSettings
        {
            MSBuildSettings = new DotNetCoreMSBuildSettings { NoLogo = true }
                .WithProperty("AssemblyVersion", assemblyVersion)
                .WithProperty("FileVersion", assemblyFileVersion)
                .WithProperty("InformationalVersion", assemblyInformationalVersion)
                .WithProperty("Version", assemblyInformationalVersion),
            Configuration = buildConfiguration,
            Verbosity = buildVerbosity,
            OutputDirectory = artifactsDirectory
        };
        DotNetCorePack(codeToPackFolder, packSettings);
    });

Task("NuGetPush")
    .IsDependentOn("Pack")
    .Does(() =>
    {
        var packageSearchPattern = string.Format("{0}/{1}*.nupkg", artifactsDirectory, projectToPack);
        var nuGetPushSettings = new DotNetCoreNuGetPushSettings { Source = "https://www.nuget.org/api/v2/package", ApiKey = nuGetApiKey };
        DotNetCoreNuGetPush(packageSearchPattern, nuGetPushSettings);
    });

RunTarget(target);