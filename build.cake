var target = Argument("target", "Default");
var buildConfiguration = Argument("configuration", "Release");
var buildVerbosity = (DotNetCoreVerbosity)Enum.Parse(typeof(DotNetCoreVerbosity), Argument("buildVerbosity", "Quiet"));
var buildNumber = Argument("buildNumber", "0");

var msBuildSettings = new DotNetCoreMSBuildSettings { NoLogo = true };
var cleanSettings = new DotNetCoreCleanSettings { MSBuildSettings = msBuildSettings, Verbosity = buildVerbosity };
var restoreSettings = new DotNetCoreRestoreSettings { MSBuildSettings = msBuildSettings, Verbosity = buildVerbosity };
var buildSettings = new DotNetCoreBuildSettings { MSBuildSettings = msBuildSettings, Configuration = buildConfiguration, Verbosity = buildVerbosity };
var testSettings = new DotNetCoreTestSettings { NoBuild = true, Configuration = buildConfiguration, Verbosity = buildVerbosity };
var packSettings = new DotNetCorePackSettings { MSBuildSettings = msBuildSettings, Configuration = buildConfiguration, Verbosity = buildVerbosity };
var publishSettings = new DotNetCorePublishSettings { MSBuildSettings = msBuildSettings, Configuration = buildConfiguration, Verbosity = buildVerbosity };

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

Task("Clean")
    .Does(() =>
    {
        foreach (var folder in projectFolders)
        {
            Information(folder);
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
            DotNetCoreTest(folder, testSettings);
        }
    });

Task("Version")
    .Does(() =>
    {
        foreach (var folder in codeFolders)
        {
            var path = String.Concat(folder, "\\", folder, ".csproj");
            var content = System.IO.File.ReadAllText(path);
            var document = new System.Xml.XmlDocument();
            document.LoadXml(content);
            var element = document.DocumentElement["PropertyGroup"]["VersionPrefix"];
            var segments = element.InnerText.Split('.');
            var version = String.Format("{0}.{1}.{2}", segments[0], segments[1], buildNumber);
            element.InnerText = version;
            document.DocumentElement["PropertyGroup"]["AssemblyVersion"].InnerText = version;
            document.DocumentElement["PropertyGroup"]["FileVersion"].InnerText = version;
            System.IO.File.WriteAllText(path, document.InnerXml);
        }
    });

Task("Default")
    .IsDependentOn("Test");

Task("Pack")
    .IsDependentOn("Test")
    .IsDependentOn("Version")
    .Does(() =>
    {
        foreach (var folder in codeFolders)
        {
            Information(folder);
            DotNetCorePack(folder, packSettings);
        }
    });

Task("Publish")
    .IsDependentOn("Pack")
    .Does(() =>
    {
        foreach (var folder in codeFolders)
        {
            Information(folder);
            DotNetCorePublish(folder, publishSettings);
        }
    });

RunTarget(target);