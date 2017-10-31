var srcDir = Argument<string>("srcDir");
var toolsDir = Argument<string>("toolsDir");
var target = Argument<string>("target", "Test");
var buildConfiguration = Argument<string>("buildConfiguration", "Release");
var buildVerbosity = (DotNetCoreVerbosity)Enum.Parse(typeof(DotNetCoreVerbosity), Argument<string>("buildVerbosity", "Minimal"));
var softwareVersion = target == "Pack" ? Argument<string>("softwareVersion") : Argument<string>("softwareVersion", string.Empty);
var buildNumber = Argument<int>("buildNumber", 0);
var commitHash = Argument<string>("commitHash");
var nuGetApiKey = Argument<string>("nuGetApiKey", string.Empty);

var srcFolder = new DirectoryInfo(srcDir);
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
var artifactsDirectory = System.IO.Path.Combine(toolsDir, "artifacts");

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
        
        var content = System.IO.File.ReadAllText(System.IO.Path.Combine(codeToPackFolder, projectToPack + ".csproj"));
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
        var assemblyVersion = String.Format("{0}.{1}.{1}.{1}", versionPrefix[0], 0);
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
        var packageSearchPattern = System.IO.Path.Combine(artifactsDirectory, projectToPack + "*.nupkg");
        var nuGetPushSettings = new DotNetCoreNuGetPushSettings { Source = "https://www.nuget.org/api/v2/package", ApiKey = nuGetApiKey };
        DotNetCoreNuGetPush(packageSearchPattern, nuGetPushSettings);
    });

RunTarget(target);