using Newtonsoft.Json;
using System.IO;
using VaM.ModManager.App.Models;

namespace VaM.ModManager.App.Services
{
    public class ConfigurationService
    {
        private static readonly string CONFIG_NAME = "config.json";

        private static ConfigurationItem _instance;
        public static ConfigurationItem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = GetConfiguration;

                return _instance;
            }
            private set => _instance = value;
        }
        
        public static bool HasConfiguration
        {
            get => File.Exists(CONFIG_NAME);
        }

        public static string SavesFolder
        {
            get => Path.Combine(Instance.VamLocation, @"saves");
        }

        public static string SceneFolder
        {
            get => Path.Combine(SavesFolder, @"scene");
        }

        public static string SceneFolderMeshedVR
        {
            get => Path.Combine(SceneFolder, @"MeshedVR");
        }

        public static string PersonFolder
        {
            get => Path.Combine(SavesFolder, @"Person");
        }

        public static string PersonAppearanceFolder
        {
            get => Path.Combine(PersonFolder, @"appearance");
        }

        public static string PersonPoseFolder
        {
            get => Path.Combine(PersonFolder, @"pose");
        }

        public static string PersonFullFolder
        {
            get => Path.Combine(PersonFolder, @"full");
        }

        public static string ImportFolder
        {
            get => Path.Combine(Instance.VamLocation, @"Import");
        }

        public static string MorphsFolder
        {
            get => Path.Combine(ImportFolder, @"morphs");
        }

        public static string MorphsFemaleFolder
        {
            get => Path.Combine(MorphsFolder, @"female");
        }

        public static string MorphsFemaleGenitaliaFolder
        {
            get => Path.Combine(MorphsFolder, @"female_genitalia");
        }

        public static string MorphsMaleFolder
        {
            get => Path.Combine(MorphsFolder, @"male");
        }

        public static string MorphsMaleGenitaliaFolder
        {
            get => Path.Combine(MorphsFolder, @"male_genitalia");
        }

        public static string ScriptsFolder
        {
            get => Path.Combine(SavesFolder, @"Scripts");
        }

        public static string TexturesFolder
        {
            get => Path.Combine(Instance.VamLocation, @"Textures");
        }

        public static string ClothingFolder
        {
            get => Path.Combine(Instance.VamLocation, @"Custom\Clothing");
        }

        public static string ClothingFemaleFolder
        {
            get => Path.Combine(Instance.VamLocation, @"Custom\Clothing\Female");
        }

        public static string ClothingMaleFolder
        {
            get => Path.Combine(Instance.VamLocation, @"Custom\Clothing\Male");
        }

        public static string VamModManagerFolder
        {
            get => Path.Combine(Instance.VamLocation, @"vmm");
        }

        public static string FinalizationFolder
        {
            get => @"C:\temp\vmm\finalization";
        }

        private static ConfigurationItem GetConfiguration
        {
            get
            {
                if (HasConfiguration)
                {
                    var json = File.ReadAllText(CONFIG_NAME);
                    return JsonConvert.DeserializeObject<ConfigurationItem>(json);
                }
                return GetEmptyConfiguration;
            }
        }

        private static ConfigurationItem GetEmptyConfiguration
        {
            get => new ConfigurationItem { ModLocation=string.Empty, VamLocation=string.Empty, ReplaceDefaultJson=false };
        }

        public static void ReplaceDefaultJsonWithBlank(bool replace)
        {
            string defaultjsonfile = Path.Combine(ConfigurationService.SceneFolderMeshedVR, "default.json");
            string vmmdefaultjsonfile = Path.Combine(ConfigurationService.VamModManagerFolder, "default.json.bak");

            if (replace)
            {
                //vmm path may not exist and we need it to store the backup
                Directory.CreateDirectory(ConfigurationService.VamModManagerFolder);

                //backup original default json and replace with blank scene
                if (File.Exists(defaultjsonfile) && !File.Exists(vmmdefaultjsonfile))
                {
                    //File.CreateText(vmmdefaultjsonfile).Dispose();
                    File.Copy(defaultjsonfile, vmmdefaultjsonfile, true);
                    File.Copy("default.blank.json", defaultjsonfile, true);
                }

                return;
            }

            //go back to the original default json
            if(File.Exists(vmmdefaultjsonfile))
            {
                File.Copy(vmmdefaultjsonfile, defaultjsonfile, true);
                File.Delete(vmmdefaultjsonfile);
            }
        }

        public static void SaveConfiguration(ConfigurationItem config)
        {
            var json = JsonConvert.SerializeObject(config);
            File.WriteAllText(CONFIG_NAME, json);
            Instance = config;
        }
    }
}
