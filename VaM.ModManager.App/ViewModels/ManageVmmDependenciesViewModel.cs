using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using VaM.ModManager.App.Models;
using VaM.ModManager.App.Services;

namespace VaM.ModManager.App.ViewModels
{
    public class ManageVmmDependenciesViewModel : ViewModelBase
    {
        private VmmItem _vmm;
        private List<VmmDependencyItem> _dependencies;
        private List<VmmItem> _installedMods;

        public ManageVmmDependenciesViewModel(VmmItem vmm)
        {
            Vmm = vmm;
            Dependencies = Vmm.Dependencies.ToList();

            ListInstalledMods();
        }

        public VmmItem Vmm
        {
            get => _vmm;
            set => this.RaiseAndSetIfChanged(ref _vmm, value);
        }

        public List<VmmDependencyItem> Dependencies
        {
            get => _dependencies;
            set => this.RaiseAndSetIfChanged(ref _dependencies, value);
        }

        public List<VmmItem> InstalledMods
        {
            get => _installedMods;
            private set => this.RaiseAndSetIfChanged(ref _installedMods, value);
        }

        public void OnRemoveDependency(VmmDependencyItem dependency)
        {
            Vmm.Dependencies.Remove(dependency);
            new ModService().SaveVmmFile(Vmm);

            Dependencies = Vmm.Dependencies.ToList();

            ListInstalledMods();
        }

        public void OnAddDependency(VmmItem dependencyVmm)
        {
            if (!Vmm.Dependencies.Any(d => d.VmmFilename == dependencyVmm.VmmFilename))
            {
                Vmm.Dependencies.Add(
                    new VmmDependencyItem
                    {
                        DisplayName = dependencyVmm.DisplayName,
                        VmmFilename = dependencyVmm.VmmFilename
                    }
                );

                new ModService().SaveVmmFile(Vmm);
                Dependencies = Vmm.Dependencies.ToList();

                ListInstalledMods();
            }
        }

        public void ListInstalledMods()
        {
            //can't reference your own mod. or any of the current dependencies
            InstalledMods = new ModService().ListInstalledMods()
                .Where(m => m.Author != Vmm.Author && m.ModName != Vmm.ModName)
                .Where(m => !Dependencies.Any(d => d.VmmFilename == m.VmmFilename))
                .ToList();
        }
    }
}
