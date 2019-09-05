using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace VaM.ModManager.App.Views
{
    public class ConfigurationView : UserControl
    {
        public ConfigurationView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
