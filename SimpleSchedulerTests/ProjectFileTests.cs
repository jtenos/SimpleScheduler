using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Xml;

namespace SimpleSchedulerTests;

[TestClass]
public class ProjectFileTests
{
    private const string TARGET_FRAMEWORK = "net6.0";
    private const string VERSION = "5.0.0";

    [TestMethod]
    public void TestTargetFramework()
    {
        CheckXmlValueInAllProjects("/Project/PropertyGroup/TargetFramework/text()", TARGET_FRAMEWORK);
    }

    [TestMethod]
    public void TestVersion()
    {
        CheckXmlValueInAllProjects("/Project/PropertyGroup/Version/text()", VERSION);
    }

    [TestMethod]
    public void TestAssemblyVersion()
    {
        CheckXmlValueInAllProjects("/Project/PropertyGroup/AssemblyVersion/text()", VERSION);
    }

    [TestMethod]
    public void TestNullable()
    {
        CheckXmlValueInAllProjects("/Project/PropertyGroup/Nullable/text()", "enable");
    }

    [TestMethod]
    public void TestTreatWarningsAsErrors()
    {
        CheckXmlValueInAllProjects("/Project/PropertyGroup/TreatWarningsAsErrors/text()", "true");
    }

    [TestMethod]
    public void TestWarningsAsErrors()
    {
        CheckXmlNodeExistsInAllProjects("/Project/PropertyGroup/WarningsAsErrors");
    }

    [TestMethod]
    public void TestImplicitUsings()
    {
        CheckXmlValueInAllProjects("/Project/PropertyGroup/ImplicitUsings/text()", "enable");
    }

    private static void CheckXmlNodeExistsInAllProjects(string xpath)
    {
        foreach (FileInfo csprojFile in GetCsprojFiles())
        {
            CheckXmlNodeExists(csprojFile, xpath);
        }
    }

    private static void CheckXmlNodeExists(FileInfo xmlFile, string xpath)
    {
        using Stream fileStream = xmlFile.OpenRead();
        XmlDocument xmlDoc = new();
        xmlDoc.Load(fileStream);
        if (xmlDoc.SelectNodes(xpath)?.Cast<XmlNode>().Any() == true)
        {
            return;
        }

        Assert.Fail($"XML file {xmlFile.Name} does not contain {xpath}");
    }

    private static void CheckXmlValueInAllProjects(string xpath, string expectedValue)
    {
        foreach (FileInfo csprojFile in GetCsprojFiles())
        {
            CheckXmlValue(csprojFile, xpath, expectedValue);
        }
    }

    private static void CheckXmlValue(FileInfo xmlFile, string xpath, string expectedValue)
    {
        using Stream fileStream = xmlFile.OpenRead();
        XmlDocument xmlDoc = new();
        xmlDoc.Load(fileStream);
        foreach (XmlNode node in xmlDoc.SelectNodes(xpath)!)
        {
            if (node.Value == expectedValue) { return; }
        }

        Assert.Fail($"XML file {xmlFile.Name} does not contain value {expectedValue} in {xpath}");
    }

    private static IEnumerable<FileInfo> GetCsprojFiles()
    {
        DirectoryInfo currentDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!;
        while (!currentDir.GetFiles("SimpleScheduler.sln").Any())
        {
            currentDir = new(Path.Combine(currentDir.FullName, ".."));
            currentDir.Refresh();
        }

        FileInfo getFile(string projectName)
        {
            return new(Path.Combine(currentDir.FullName, projectName, $"{projectName}.csproj"));
        }

        yield return getFile("SimpleSchedulerAPI");
        yield return getFile("SimpleSchedulerApiModels");
        yield return getFile("SimpleSchedulerAppServices");
        yield return getFile("SimpleSchedulerBlazorWasm");
        yield return getFile("SimpleSchedulerData");
        yield return getFile("SimpleSchedulerDataEntities");
        yield return getFile("SimpleSchedulerDomainModels");
        yield return getFile("SimpleSchedulerEmail");
        yield return getFile("SimpleSchedulerSerilogEmail");
        yield return getFile("SimpleSchedulerService");
        yield return getFile("SimpleSchedulerServiceChecker");
        yield return getFile("SimpleSchedulerServiceClient");
        yield return getFile("SimpleSchedulerTests");
    }
}
