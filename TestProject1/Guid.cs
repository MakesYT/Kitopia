namespace TestProject1;

[TestClass]
public class Guid
{
    [TestMethod]
    public void GuidGen()
    {
        var newGuid = System.Guid.NewGuid();
        Console.WriteLine($"New GUID: {newGuid}");
    }
}