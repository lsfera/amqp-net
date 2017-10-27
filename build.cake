var target = Argument("target", "Default");
var buildnumber = Argument("buildnumber", "0");
var configuration = Argument("configuration", "Release");

var projectFolders = new DirectoryInfo(".\\src")
    .GetDirectories()
    .Where(x => x.GetFiles("*.csproj").Length > 0)
    .Select(x => x.FullName);
var codeFolders = projectFolders.Where(x => x.Contains("Tests"));
var testFolders = projectFolders.Except(codeFolders);

Task("Clean").Does(() => {
    foreach (var folder in projectFolders) {
        Information(folder);
	    DotNetCoreClean(folder);
    }
});

Task("Restore").IsDependentOn("Clean").Does(() => {
    foreach (var folder in projectFolders) {
        Information(folder);
        DotNetCoreRestore(folder);
    }
});

Task("Build").IsDependentOn("Restore").Does(() => {
    foreach (var folder in testFolders) {
        Information(folder);
        DotNetCoreBuild(folder);
    }
});

Task("Test").IsDependentOn("Build").Does(() => {
    foreach (var folder in testFolders) {
        Information(folder);
        DotNetCoreRestore(folder);
    }
});

Task("Version").Does(() => {
    foreach (var folder in codeFolders) {
        var path = String.Concat(folder, "\\", folder, ".csproj");
        var content = System.IO.File.ReadAllText(path);
        var document = new System.Xml.XmlDocument();
        document.LoadXml(content);
        var element = document.DocumentElement["PropertyGroup"]["VersionPrefix"];
        var segments = element.InnerText.Split('.');
	    var version = String.Format("{0}.{1}.{2}", segments[0], segments[1], buildnumber);
        element.InnerText = version;
	    document.DocumentElement["PropertyGroup"]["AssemblyVersion"].InnerText = version;
	    document.DocumentElement["PropertyGroup"]["FileVersion"].InnerText = version;
        System.IO.File.WriteAllText(path, document.InnerXml);
	}
});

Task("Pack").IsDependentOn("Test").Does(() => {
    foreach (var folder in codeFolders) {
        Information(folder);
        DotNetCorePack(folder);
    }
});

Task("Default").IsDependentOn("Test");

Task("Release").IsDependentOn("Version").IsDependentOn("Pack");

Task("Publish").IsDependentOn("Release").Does(() => {
    foreach (var folder in codeFolders) {
        Information(folder);
        DotNetCorePublish(folder);
    }
});

RunTarget(target);