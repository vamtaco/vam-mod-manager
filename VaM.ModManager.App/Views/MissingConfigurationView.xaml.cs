using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace VaM.ModManager.App.Views
{
    public class MissingConfigurationView : UserControl
    {
        public MissingConfigurationView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
