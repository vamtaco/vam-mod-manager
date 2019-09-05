using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace VaM.ModManager.App.Views
{
    public class ManageModsView : UserControl
    {
        public ManageModsView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
