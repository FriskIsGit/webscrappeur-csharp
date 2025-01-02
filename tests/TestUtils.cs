using NUnit.Framework;

namespace WebScrapper.tests;

class TestUtils {
    private const string rootNamePrefix = "webscrap";
    private static string projectRootPath = "";

    [Test]
    public void findRoot() {
        var root = GetProjectRootDirectory();
        if (root.Length == 0) {
            Assert.Fail("Project root not found.");
        }
    }
    public static string GetProjectRootDirectory() {
        if (projectRootPath.Length > 0) {
            return projectRootPath;
        }
        string currentDir = Directory.GetCurrentDirectory();
        if (Path.GetFileName(currentDir).StartsWith(rootNamePrefix)) {
            projectRootPath = currentDir;
            return projectRootPath;
        }
        DirectoryInfo? dir = Directory.GetParent(currentDir);
        while (dir != null) {
            if (dir.Name.ToLower().StartsWith(rootNamePrefix)) {
                projectRootPath = dir.FullName;
                return projectRootPath;
            }
            dir = dir.Parent;
        }
        return "";
    }
}