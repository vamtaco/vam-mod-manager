using ReactiveUI;
using VaM.ModManager.App.Models;
using VaM.ModManager.App.Services;

namespace VaM.ModManager.App.ViewModels
{
    public class NewModViewModel : ViewModelBase
    {
        VmmItem _vmm;

        public NewModViewModel()
        {
            Vmm = new VmmItem
            {
                Author = ConfigurationService.Instance.Author,
                ModVersion = 0.1f
            };
        }

        public VmmItem Vmm
        {
            get => _vmm;
            set => this.RaiseAndSetIfChanged(ref _vmm, value);
        }

        public void OnCreate()
        {
            new ManagedModService().CreateMod(_vmm);
        }
    }
}
