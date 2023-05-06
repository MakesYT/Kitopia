using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Animation;
using log4net;

namespace Kitopia.View;

/// <summary>
///     WarnDialog.xaml 的交互逻辑
/// </summary>
public partial class ErrorDialog : Window
{
    private static readonly ILog log = LogManager.GetLogger("ErrorDialog");

    public ErrorDialog(string infostr, string str)
    {
        InitializeComponent();
        info.Text = infostr;
        text.Text = str;
        var storyboard = (Storyboard)FindResource("Storyboard1");
        storyboard.Begin();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void logs_Click(object sender, RoutedEventArgs e)
    {
        ExecuteInCmd("start \"\" \"" + Directory.GetCurrentDirectory() + "\\logs\"", "");
        // System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() );
    }

    public string ExecuteInCmd(string cmdline, string dir)
    {
        using (var process = new Process())
        {
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = false;
            // process.StartInfo.
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            // process.StandardInput.AutoFlush = true;
            process.StandardInput.WriteLine("cd /d " + dir);
            process.StandardInput.WriteLine(cmdline + "&exit");
            process.StandardInput.Close();
            string line;
            while ((line = process.StandardOutput.ReadLine()) != null) log.Debug(line);

            //获取cmd窗口的输出信息  
            // string output = process.StandardOutput.ReadToEnd();
            // process.StandardOutput.
            process.WaitForExit();
            process.Close();

            return "";
        }
    }
}