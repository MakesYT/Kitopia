namespace TestProject1;

[TestClass]
public class a
{
    [TestMethod]
    public void aa()
    {
    }

    [TestMethod]
    public void aa1()
    {
        //20_旺仔小乔 - 雾里 (官方女版)_(Vocals)_5.wav
        var filename = "20_旺仔小乔 - 雾里 (官方女版)_(Vocals)_5_1";
        filename = filename.Remove(filename.IndexOf("_"), filename.LastIndexOf(")_") - filename.IndexOf("_") + 1);
        Console.WriteLine(filename);
    }
}