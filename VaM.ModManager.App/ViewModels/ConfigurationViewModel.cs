using ReactiveUI;
using VaM.ModManager.App.Models;
using VaM.ModManager.App.Services;

namespace VaM.ModManager.App.ViewModels
{
    public class ConfigurationViewModel : ViewModelBase
    {
        ConfigurationItem config;

        public ConfigurationViewModel()
        {
            Config = ConfigurationService.Instance;
        }

        public ConfigurationItem Config
        {
            get => config;
            set => this.RaiseAndSetIfChanged(ref config, value);
        }

        public void OnSave()
        {
            ConfigurationService.ReplaceDefaultJsonWithBlank(config.ReplaceDefaultJson);
            ConfigurationService.SaveConfiguration(config);
        }
    }
}
