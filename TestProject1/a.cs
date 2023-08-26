namespace TestProject1;

[TestClass]
public class a
{
    [TestMethod]
    public void aa()
    {
        var directoryInfo = new DirectoryInfo(@"H:\Ai\总数据\v2\wangzai\新建文件夹");
        foreach (var enumerateFile in directoryInfo.EnumerateFiles())
        {
            //20_旺仔小乔 - 雾里 (官方女版)_(Vocals)_5.wav
            var filename = enumerateFile.Name;
            filename = filename.Remove(filename.IndexOf("_"), filename.LastIndexOf(")_") - filename.IndexOf("_") + 1);
            File.Move(enumerateFile.FullName, directoryInfo.FullName + "//" + filename);
        }
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