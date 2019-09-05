using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using VaM.ModManager.App.Models;
using VaM.ModManager.App.Services;

namespace VaM.ModManager.App.ViewModels
{
    public class FinalizeModViewModel : ViewModelBase
    {
        private VmmItem _vmm;
        private List<ModFolderItem> _excludeModFolders;

        public FinalizeModViewModel(VmmItem vmm)
        {
            Vmm = vmm;

            //list excluded mod folder in service, takes vmm.  check each dir for empty and set bool
            ExcludedModFolders = new ManagedModService().ListEmptyModFolders(vmm)
                .Where(f => f.DisplayName != "VaM Mod Manager")
                .ToList();
        }

        public VmmItem Vmm
        {
            get => _vmm;
            set => this.RaiseAndSetIfChanged(ref _vmm, value);
        }

        public List<ModFolderItem> ExcludedModFolders
        {
            get => _excludeModFolders;
            set => this.RaiseAndSetIfChanged(ref _excludeModFolders, value);
        }

        public void OnFinalize()
        {
            var excluded = ExcludedModFolders.Where(f => f.IsExcludedFromFinalization).ToList();
            new ManagedModService().FinalizeMod(Vmm, excluded);
        }
    }
}
