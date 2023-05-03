
using Core.SDKs;

namespace Test
{
    public class InstalledSoftware
    {
        public void e()
        {
            DateTime startTime = DateTime.Now;


            foreach (string path in Directory.EnumerateDirectories("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs", "*", SearchOption.AllDirectories))
            {
                foreach (string file in Directory.EnumerateFiles(path, "*.lnk"))
                {


                    Console.WriteLine(LnkSolver.ResolveShortcut(file));

                }

            }
            DateTime endTime = DateTime.Now;
            TimeSpan span = endTime.Subtract(startTime);
            Console.WriteLine("程序运行时间：{0}ms", span.TotalMilliseconds);

        }


    }
}
