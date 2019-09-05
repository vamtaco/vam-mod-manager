using Avalonia;
using ReactiveUI;
using VaM.ModManager.App.Models;
using VaM.ModManager.App.Services;

namespace VaM.ModManager.App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public object content = "Error loading content...";
        public object previousContent;

        public MainWindowViewModel()
        {
            LoadMainWindow();
        }

        public object Content
        {
            get => content;
            set
            {
                previousContent = content;
                this.RaiseAndSetIfChanged(ref content, value);
            } 
        }

        public void OnAddVmm()
        {
            Content = new AddVmmViewModel();
        }

        public void OnEditVmm(VmmItem vmm)
        {
            if(vmm != null)
                Content = new EditVmmViewModel(vmm);
        }

        public void OnManageVmmDependencies(VmmItem vmm)
        {
            Content = new ManageVmmDependenciesViewModel(vmm);
        }

        public void OnNewMod(VmmItem vmm)
        {
            Content = new NewModViewModel();
        }

        public void OnFinalizeMod(VmmItem vmm)
        {
            Content = new FinalizeModViewModel(vmm);
        }

        public void OnConfig()
        {
            Content = new ConfigurationViewModel();
        }

        public void OnExit()
        {
            Application.Current.Exit();
        }

        public void OnPrevious()
        {
            Content = previousContent;
        }

        public void LoadMainWindow()
        {
            if (!ConfigurationService.HasConfiguration)
            {
                Content = new MissingConfigurationViewModel();
                return;
            }

            var manageModsView = new ManageModsViewModel();

            if (manageModsView.HasMods)
                Content = manageModsView;
            else
                Content = "No mods found.  Download some mods and place them in your mod folder, or click on the File menu and select New Mod... to create your own.";
        }
    }
}
