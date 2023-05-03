using Kitopia.SDKs.Everything;
using System.Text;

namespace Test
{
    public class Everything
    {
        public void e()
        {

            Everything64.Everything_SetSearchW("*.docx|*.doc|*.xls|*.xlsx|*.pdf|*.ppt|*.pptx");

            Everything64.Everything_SetMatchCase(true);
            Everything64.Everything_QueryW(true);


            Console.WriteLine(Everything64.Everything_QueryW(true));
            const int bufsize = 260;
            StringBuilder buf = new StringBuilder(bufsize);
            for (int i = 0; i < Everything64.Everything_GetNumResults(); i++)
            {

                // get the result's full path and file name.
                Everything64.Everything_GetResultFullPathNameW(i, buf, bufsize);
                FileInfo fileInfo = new FileInfo(buf.ToString());
                Console.WriteLine(fileInfo.Name);
                // add it to the list box				
                //Console.WriteLine(buf.ToString());
            }
        }
    }
}
