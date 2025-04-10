using RE_Editor.Generator;

namespace RE_Editor.Tests;

[TestClass]
public class TestGeneration {
    [TestMethod]
    public void TestWhitelistGeneration() {
        new GenerateFiles().Go(["useWhitelist", "dryRun"]);
    }

    [TestMethod]
    public void TestGreylistGeneration() {
        new GenerateFiles().Go(["useGreylist", "dryRun"]);
    }
}