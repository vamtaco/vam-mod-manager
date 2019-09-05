using ReactiveUI;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VaM.ModManager.App.Enums;
using VaM.ModManager.App.Models;
using VaM.ModManager.App.Services;

namespace VaM.ModManager.App.ViewModels
{
    public class ManageModsViewModel : ViewModelBase
    {
        private IList<VmmItem> _availableMods;
        private IList<VmmItem> _installedMods;
        private VmmItem _selectedAvailableMod;
        private VmmItem _selectedInstalledMod;

        public ManageModsViewModel()
        {
            var ms = new ModService();
            AvailableMods = ms.ListAvailableMods();
            InstalledMods = ms.ListInstalledMods();
        }

        public IList<VmmItem> AvailableMods
        {
            get => _availableMods;
            private set => this.RaiseAndSetIfChanged(ref _availableMods, value);
        }

        public IList<VmmItem> InstalledMods
        {
            get => _installedMods;
            private set => this.RaiseAndSetIfChanged(ref _installedMods, value);
        }

        public VmmItem SelectedInstalledMod
        {
            get => _selectedInstalledMod;
            set => this.RaiseAndSetIfChanged(ref _selectedInstalledMod, value);
        }

        public VmmItem SelectedAvailableMod
        {
            get => _selectedAvailableMod;
            set => this.RaiseAndSetIfChanged(ref _selectedAvailableMod, value);
        }

        public bool HasMods
        {
            get => (AvailableMods != null && AvailableMods.Count > 0) || (InstalledMods != null && InstalledMods.Count > 0);
        }

        public void OnInstall(VmmItem vmm)
        {
            var ms = new ModService();
            string modPath = Path.Combine(ConfigurationService.Instance.ModLocation, vmm.VacFilename);
            new ManagedModService().InstallMod(modPath);
            InstalledMods = ms.ListInstalledMods();
            AvailableMods = ms.ListAvailableMods();
        }

        public void OnUninstall(VmmItem vmm)
        {
            var ms = new ManagedModService();
            ms.DeleteMod(vmm);
            InstalledMods = new ModService().ListInstalledMods();
        }

        public void OnDeleteMod(VmmItem vmm)
        {
            var ms = new ManagedModService();
            ms.DeleteMod(vmm);
            InstalledMods = new ModService().ListInstalledMods();
        }

        public void OnLaunch(VmmItem vmm)
        {
            //This won't work until there is a way to launch a scene via command line...
            //var ms = new ModService();
            //string fullModPath = Path.Combine(ms.ModTypeToVamPath(vmm.ModType), vmm.ModPathAndFilename);
            //fullModPath = ConfigurationService.GetConfiguration().VamLocation;
            //ProcessStartInfo pInfo = new ProcessStartInfo();
            //pInfo.FileName = Path.Combine(fullModPath, @"vam.exe");
            //pInfo.Arguments = $"-scene \"c:\\zupdate\\vam\\saves\\scene\\unknown\\jenniC\\1543095808.json\" -vrmode None";
            //Process.Start(pInfo);
            var vamExe = ConfigurationService.Instance.VamLocation;
            vamExe = Path.Combine(vamExe, @"VaM_Updater.exe");
            Process.Start(vamExe);
        }
    }
}
