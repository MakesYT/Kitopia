using System.Runtime.InteropServices;
using System;
using System.Windows;
using System.Windows.Interop;

namespace Kitopia.View
{
    /// <summary>
    /// SearchView.xaml 的交互逻辑
    /// </summary>
    public partial class SearchView : Window
    {
        public SearchView()
        {
            InitializeComponent();

        }

        private void w_Deactivated(object sender, System.EventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }
        [DllImport("user32.dll")]
        static extern IntPtr SetFocus(IntPtr hWnd);
        private void w_Activated(object sender, System.EventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            SetFocus(hwnd);
        }
    }
}
