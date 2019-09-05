using ReactiveUI;
using VaM.ModManager.App.Models;
using VaM.ModManager.App.Services;

namespace VaM.ModManager.App.ViewModels
{
    public class EditVmmViewModel : ViewModelBase
    {
        private VmmItem _vmm;

        public EditVmmViewModel(VmmItem vmm)
        {
            Vmm = vmm;
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
