using System.Management;

namespace KitopiaTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        try
        {
            ManagementObjectSearcher searcher =
                new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem");

            foreach (ManagementObject queryObj in searcher.Get())
            {
                Console.WriteLine("Caption: {0}", queryObj["Caption"]);
            }
        }
        catch (ManagementException e)
        {
            Console.WriteLine("An error occurred while querying for WMI data: " + e.Message);
        }

        Assert.Pass();
    }
}