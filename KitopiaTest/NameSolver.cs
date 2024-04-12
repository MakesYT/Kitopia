using System.Text;

using Core.Window;

namespace KitopiaTest;


public class NameSolver
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var name = "Windows Copilot Frame MSIX Pack";
        var keys = new System.Collections.Generic.List<string>();
        var initials = name.Split(" ",StringSplitOptions.RemoveEmptyEntries);
        AppTools.CreateCombinations(keys, 0, new StringBuilder(), initials);
        Assert.Pass();
    }
}