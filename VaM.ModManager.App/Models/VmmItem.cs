using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using VaM.ModManager.App.Enums;
using VaM.ModManager.App.Extensions;

namespace VaM.ModManager.App.Models
{
    public class VmmItem
    {
        private List<VmmDependencyItem> _dependencies;

        public string Author { get; set; }

        public string ModName { get; set; }

        public ModTypeEnum ModType { get; set; } = ModTypeEnum.Unsupported;

        public float ModVersion { get; set; } = 0.1f;

        [JsonIgnore]
        public string ModTypeDisplay
        {
            get => ModType.ToName();
        }

        [JsonIgnore]
        public bool MaintainOriginalDirectoryStructure { get; set; }

        [JsonIgnore]
        public string DisplayName
        {
            get => $"{ModName} {ModVersion.ToString("0.0#")}: {ModTypeDisplay} by {Author}";
        }

        [JsonIgnore]
        public string ModPath
        {
            get
            {
                var path = Path.Combine(Author, ModName);

                if (ModType == ModTypeEnum.InDevelopment)
                    return Path.Combine(path, $"dev");

                return Path.Combine(path, $"{ModVersion.ToString("0.0#")}");
            }
        }

        [JsonIgnore]
        private string FilenameNoExtension
        {
            get
            {
                string ver = ModType == ModTypeEnum.InDevelopment ? "dev" : ModVersion.ToString("0.0#");
                return $"{Author}.{ModName}.{ver}";
            }
        }

        [JsonIgnore]
        public string ModPathAndFilename
        {
            get => Path.Combine(ModPath, VmmFilename);
        }

        [JsonIgnore]
        public string VmmFilename
        {
            get => $"{FilenameNoExtension}.vmm";
        }

        [JsonIgnore]
        public string VacFilename
        {
            get => $"{FilenameNoExtension}.vac";
        }

        [JsonIgnore]
        public string ToJson
        {
            get => JsonConvert.SerializeObject(this);
        }

        [JsonIgnore]
        public bool IsDev
        {
            get => ModType == ModTypeEnum.InDevelopment;
        }

        [JsonIgnore]
        public List<string> InstallLog { get; set; } = new List<string>();

        public List<VmmDependencyItem> Dependencies
        {
            get
            {
                if (_dependencies == null)
                    _dependencies = new List<VmmDependencyItem>();

                return _dependencies; 
            }
            set => _dependencies = value;
        }
    }
}
