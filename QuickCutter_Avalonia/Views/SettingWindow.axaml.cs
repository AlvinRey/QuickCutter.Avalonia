using Avalonia.Controls;
using QuickCutter_Avalonia.ViewModels;

namespace QuickCutter_Avalonia.Views
{
    public partial class SettingWindow : Window
    {
        private SettingWindowViewModel? viewModel;

        public SettingWindow()
        {
            InitializeComponent();
            this.Loaded += SettingWindow_Loaded;
            ConfirmBtn.Click += ConfirmBtn_Click;
            CencelBtn.Click += CencelBtn_Click;
        }

        private void SettingWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            viewModel = this.DataContext as SettingWindowViewModel;
        }

        private void ConfirmBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            viewModel?.SaveConfig();
            this.Close();
        }

        private void CencelBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
