using System.Windows;
using Core.SDKs.Services;
using Kitopia.View;

namespace Kitopia.Services;

public class LabelWindowService : ILabelWindowService
{
    public void Show()
    {
        throw new System.NotImplementedException();
    }

    public void Show(string content)
    {
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            var labelWindow = ((LabelWindow)ServiceManager.Services!.GetService(typeof(LabelWindow))!);
            labelWindow.Show();
            labelWindow.RichTextBox.Text = (content);
        });
    }
}