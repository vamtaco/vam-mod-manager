using ReactiveUI;
using VaM.ModManager.App.Models;
using VaM.ModManager.App.Services;

namespace VaM.ModManager.App.ViewModels
{
    public class AddVmmViewModel : ViewModelBase
    {
        VmmItem _vmm;

        public AddVmmViewModel()
        {
            Vmm = new VmmItem();
        }

        public VmmItem Vmm
        {
            get => _vmm;
            set => this.RaiseAndSetIfChanged(ref _vmm, value);
        }

        public void OnSave()
        {
            new ModService().SaveVmmFile(_vmm);
        }
    }
}
