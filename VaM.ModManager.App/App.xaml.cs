using Avalonia;
using Avalonia.Markup.Xaml;

namespace VaM.ModManager.App
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
