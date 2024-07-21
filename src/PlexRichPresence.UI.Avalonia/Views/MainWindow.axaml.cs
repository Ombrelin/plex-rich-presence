using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;

namespace PlexRichPresence.UI.Avalonia.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void Window_OnClosing(object? sender, CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }
    }
}