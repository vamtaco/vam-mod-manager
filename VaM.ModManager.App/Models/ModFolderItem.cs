
namespace VaM.ModManager.App.Models
{
    public class ModFolderItem
    {
        public string DisplayName { get; set; }
        public string FolderPath { get; set; }
        public bool IsExcludedFromFinalization { get; set; } = false;
    }
}
