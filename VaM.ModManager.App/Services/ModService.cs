using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using VaM.ModManager.App.Enums;
using VaM.ModManager.App.Models;

namespace VaM.ModManager.App.Services
{
    public class ModService
    {
        public IList<VmmItem> ListAvailableMods()
        {
            var mods = new List<VmmItem>();

            if (ConfigurationService.HasConfiguration)
            {
                var modLocation = ConfigurationService.Instance.ModLocation;
                if (Directory.Exists(modLocation))
                {
                    //NEED TO FILTER FILE TYPES
                    var files = Directory.GetFiles(modLocation);

                    foreach(var path in files)
                    {
                        if (File.Exists(path))
                        {
                            using (var zip = ZipFile.Open(path, ZipArchiveMode.Read))
                            {
                                var vmmFile = zip.Entries.SingleOrDefault(f => f.FullName.Contains(".vmm"));
                                VmmItem vmm = null;
                                if (vmmFile != null)
                                {
                                    using (var stream = vmmFile.Open())
                                    {
                                        using (var reader = new StreamReader(stream))
                                        {
                                            var json = reader.ReadToEnd();
                                            vmm = JsonConvert.DeserializeObject<VmmItem>(json);
                                            //vmm.VacName = Path.GetFileName(path);
                                        }
                                    }
                                }
                                else if (vmm == null)
                                {
                                    vmm = new VmmItem
                                    {
                                        Author = "UNSUPPORTED MOD",
                                        ModType = ModTypeEnum.Unsupported,
                                        //VacName = Path.GetFileName(path)
                                    };
                                }

                                mods.Add(vmm);
                            }
                        }
                    }
                }
            }

            var installed = ListInstalledMods();
            mods = mods.Where(m => !installed.Any(i => i.ModType != ModTypeEnum.InDevelopment && i.VmmFilename == m.VmmFilename)).ToList();
            return mods.OrderBy(m => m.DisplayName).ToList();
        }

        public List<VmmItem> ListInstalledMods()
        {
            var vmmList = new List<VmmItem>();

            if (ConfigurationService.HasConfiguration)
            {
                var files = Directory.GetFiles(
                        ConfigurationService.Instance.VamLocation
                        , "*.vmm"
                        , SearchOption.AllDirectories
                    );

                foreach (var path in files)
                {
                    var vmm = new VmmItem();
                    using (var file = File.OpenText(path))
                    {
                        var json = file.ReadToEnd();
                        vmm = JsonConvert.DeserializeObject<VmmItem>(json);
                        vmmList.Add(vmm);
                    }
                }
            }

            return vmmList.OrderBy(m => m.DisplayName).ToList();
        }

        /// <summary>
        /// Load the mod's .json file to a MemoryStrem
        /// </summary>
        /// <param name="path">Full path to the mod's .json file.</param>
        /// <returns>MemoryStream representation of .json file</returns>
        public string LoadModJson(string path)
        {
            if (File.Exists(path))
            {
                using (var file = new StreamReader(path))
                {
                    return file.ReadToEnd();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Determine the mod type by parsing the json. Type is identified by 
        /// containing a particular node that other types don’t have.
        /// 
        /// Person Appearance – Geometry only.  No other main nodes.
        /// Person Pose – Control only
        /// Person Full – Geometry and Control
        /// Scene – WindowCamera, CoreControl, VRController, CameraControl
        /// </summary>
        /// <param name="json">Entire contents of the mods .json file.</param>
        public ModTypeEnum GetModType(string json)
        {
            var mji = JsonConvert.DeserializeObject<ModJsonItem>(json);

            if (mji != null)
            {
                if (mji.UseSceneLoadPosition.HasValue)
                {
                    return ModTypeEnum.Scene;
                }

                if(mji.Atoms.Count > 0)
                {
                    //bool hasGeometry = false;
                    bool hasControl = false;

                    foreach (var atom in mji.Atoms.Where(a => a.Type == "Person"))
                    {
                        hasControl = atom.Storables.Any(s => s.Id == "control");
                    }

                    //NOTE: the FULL folder doesn't seem to store any user created content that is generated in game.
                    //  Research showed that json with geometry and control defined were actually poses.

                    //pose: storables must have id == control
                    if (hasControl)
                    {
                        return ModTypeEnum.Pose;
                    }
                    //look: no control 
                    else
                    {
                        return ModTypeEnum.Look;
                    }
                }
            }

            return ModTypeEnum.Unsupported;
        }

        public void InstallVac(string path)
        {
            if(File.Exists(path))
            {
                using (var zip = ZipFile.Open(path, ZipArchiveMode.Read))
                {
                    var vmmFile = zip.Entries.SingleOrDefault(f => f.FullName.Contains(".vmm"));
                    VmmItem vmm = null;
                    if(vmmFile != null)
                    {
                        using (var stream = vmmFile.Open())
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                var json = reader.ReadToEnd();
                                vmm = JsonConvert.DeserializeObject<VmmItem>(json);
                            }
                        }
                    }
                    
                    if(vmm == null)
                    {
                        vmm = new VmmItem
                        {
                            ModType = ModTypeEnum.Unsupported,
                            //VacName = Path.GetFileNameWithoutExtension(path)
                        };
                    }

                    //There should only be one json file for a mod
                    var jsonFile = zip.Entries.SingleOrDefault(f => f.FullName.Contains(".json"));

                    if (vmm.ModType == ModTypeEnum.Unsupported && jsonFile != null)
                    {
                        using (var stream = jsonFile.Open())
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                var json = reader.ReadToEnd();
                                vmm.ModType = GetModType(json);
                            }
                        }
                    }
                    
                    switch (vmm.ModType)
                    {
                        case ModTypeEnum.Scene:
                            ExtractScene(zip, vmm);
                            ExtractAudio(zip, vmm);
                            ExtractMorphs(zip, vmm);
                            ExtractScripts(zip, vmm);
                            break;
                        case ModTypeEnum.Look:
                            ExtractPersonAppearance(zip, vmm);
                            break;
                        case ModTypeEnum.Pose:
                            ExtractPersonPose(zip, vmm);
                            break;
                        case ModTypeEnum.Plugin:
                            ExtractScripts(zip, vmm);
                            ExtractMorphs(zip, vmm);
                            break;
                        case ModTypeEnum.Texture:
                            ExtractTextures(zip, vmm);
                            break;
                        //case ModTypeEnum.Wardrobe:
                        //    ExtractWardrobe(zip, vmm);
                        //    break;
                    }

                    SaveVmmFile(vmm);
                }
            }
        }

        /// <summary>
        /// Uninstall a mod that was install with VaM Mod Manager
        /// </summary>
        /// <param name="path">Path to the .vmm file for the mod.</param>
        public void UninstallMod(string path)
        {
            if(File.Exists(path))
            {
                var vmm = new VmmItem();
                using (var file = File.OpenText(path))
                {
                    var json = file.ReadToEnd();
                    vmm = JsonConvert.DeserializeObject<VmmItem>(json);
                }

                foreach (var item in vmm.InstallLog)
                {
                    Directory.Delete(item, true);

                    var parent = Directory.GetParent(item);
                    if(parent.GetFiles().Count() == 0 && parent.GetDirectories().Count() == 0)
                    {
                        Directory.Delete(parent.FullName);
                    }
                }
            }
        }

        public void SaveVmmFile(VmmItem vmm, bool isFinalize = false)
        {
            string path = ModTypeToVamPath(vmm.ModType);
            path = Path.Combine(path, vmm.ModPath);

            if (isFinalize)
                path = Path.Combine(ConfigurationService.FinalizationFolder, vmm.ModPath);

            Directory.CreateDirectory(path);

            using (var file = File.CreateText(Path.Combine(path, vmm.VmmFilename)))
            {
                var json = JsonConvert.SerializeObject(vmm);
                file.Write(json);
            }
        }

        public string ModTypeToVamPath(ModTypeEnum modType)
        {
            string path = string.Empty;

            switch (modType)
            {
                case ModTypeEnum.InDevelopment:
                    path = ConfigurationService.VamModManagerFolder;
                    break;
                case ModTypeEnum.Scene:
                    path = ConfigurationService.SceneFolder;
                    break;
                case ModTypeEnum.Look:
                    path = ConfigurationService.PersonAppearanceFolder;
                    break;
                case ModTypeEnum.Pose:
                    path = ConfigurationService.PersonPoseFolder;
                    break;
                case ModTypeEnum.Plugin:
                    path = ConfigurationService.ScriptsFolder;
                    break;
                case ModTypeEnum.Texture:
                    path = ConfigurationService.TexturesFolder;
                    break;
                //case ModTypeEnum.Wardrobe:
                //    path = ConfigurationService.WardrobeFolder;
                //    break;
                case ModTypeEnum.Unsupported:
                default:
                    path = @"c:\temp\VMM";
                    break;
            }

            return path;
        }

        public void ExtractScene(ZipArchive zip, VmmItem vmm)
        {
            string sceneRoot = ConfigurationService.SceneFolder;
            string sceneModPath = Path.Combine(sceneRoot, vmm.ModPath);

            var jsonFile = zip.Entries.Single(e => e.Name.Contains(".json"));
            var textureFiles = zip.Entries.Where(e => e.Name.Contains(".jpg") || e.Name.Contains(".png"));
            var textFiles = zip.Entries.Where(e => e.Name.Contains(".txt"));

            if (!Directory.Exists(sceneModPath))
                Directory.CreateDirectory(sceneModPath);

            jsonFile.ExtractToFile(Path.Combine(sceneModPath, jsonFile.Name));

            foreach(var texture in textureFiles)
            {
                texture.ExtractToFile(Path.Combine(sceneModPath, texture.Name));
            }

            foreach (var textFile in textFiles)
            {
                textFile.ExtractToFile(Path.Combine(sceneModPath, textFile.Name));
            }

            vmm.InstallLog.Add(sceneModPath);
        }

        public void ExtractPersonAppearance(ZipArchive zip, VmmItem vmm)
        {
            string lookRoot = ConfigurationService.PersonAppearanceFolder;
            string lookModPath = Path.Combine(lookRoot, vmm.ModPath);

            var jsonFile = zip.Entries.Single(e => e.Name.Contains(".json"));
            var textureFiles = zip.Entries.Where(e => e.Name.Contains(".jpg") || e.Name.Contains(".png"));
            var textFiles = zip.Entries.Where(e => e.Name.Contains(".txt"));

            if (!Directory.Exists(lookModPath))
                Directory.CreateDirectory(lookModPath);

            jsonFile.ExtractToFile(Path.Combine(lookModPath, jsonFile.Name));

            foreach (var texture in textureFiles)
            {
                texture.ExtractToFile(Path.Combine(lookModPath, texture.Name));
            }

            foreach (var textFile in textFiles)
            {
                textFile.ExtractToFile(Path.Combine(lookModPath, textFile.Name));
            }

            vmm.InstallLog.Add(lookModPath);

            ExtractMorphs(zip, vmm);
        }

        public void ExtractPersonPose(ZipArchive zip, VmmItem vmm)
        {
            string poseRoot = ConfigurationService.PersonPoseFolder;
            string poseModPath = Path.Combine(poseRoot, vmm.ModPath);

            var jsonFile = zip.Entries.Single(e => e.Name.Contains(".json"));
            var textureFiles = zip.Entries.Where(e => e.Name.Contains(".jpg") || e.Name.Contains(".png"));
            var textFiles = zip.Entries.Where(e => e.Name.Contains(".txt"));

            if (!Directory.Exists(poseModPath))
                Directory.CreateDirectory(poseModPath);

            jsonFile.ExtractToFile(Path.Combine(poseModPath, jsonFile.Name));

            foreach (var texture in textureFiles)
            {
                texture.ExtractToFile(Path.Combine(poseModPath, texture.Name));
            }

            foreach (var textFile in textFiles)
            {
                textFile.ExtractToFile(Path.Combine(poseModPath, textFile.Name));
            }

            vmm.InstallLog.Add(poseModPath);

            ExtractMorphs(zip, vmm);
        }

        public void ExtractPersonAppearanceAndPose(ZipArchive zip, VmmItem vmm)
        {
            //Looks like this might get phased out.  Maybe only used by the game?
        }

        public void ExtractAudio(ZipArchive zip, VmmItem vmm)
        {
            var sounds = zip.Entries.Where(e => e.Name.Contains(".wav") || e.Name.Contains(".mp3") || e.Name.Contains(".ogg"));

            string dir;

            if (sounds.Count() > 0)
            {
                dir = Path.Combine(ConfigurationService.SceneFolder, vmm.ModPath);
                dir = Path.Combine(dir, "audio");

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!vmm.MaintainOriginalDirectoryStructure)
                {
                    foreach (var sound in sounds)
                    {
                        sound.ExtractToFile(Path.Combine(dir, sound.Name));
                    }
                }
                else
                {
                    string targetPath = string.Empty;
                    string targetPathAndFile = string.Empty;
                    foreach (var sound in sounds)
                    {
                        targetPathAndFile = Path.Combine(dir, sound.FullName);
                        targetPath = targetPathAndFile.Replace(sound.Name, string.Empty);

                        if (!Directory.Exists(targetPath))
                            Directory.CreateDirectory(targetPath);

                        sound.ExtractToFile(targetPathAndFile);
                    }
                }

                vmm.InstallLog.Add(dir);
            }
        }

        public void ExtractMorphs(ZipArchive zip, VmmItem vmm)
        {
            var femaleMorphs = zip.Entries.Where(e => (e.Name.Contains(".dsf") || e.Name.Contains(".vmi") || e.Name.Contains(".vmb")) && (e.FullName.Contains(@"female\") || e.FullName.Contains(@"female/")));
            var femaleGenitaliaMorphs = zip.Entries.Where(e => (e.Name.Contains(".dsf") || e.Name.Contains(".vmi") || e.Name.Contains(".vmb")) && (e.FullName.Contains(@"female_genitalia\") || e.FullName.Contains(@"female_genitalia/")));

            string dir;

            if(femaleMorphs.Count() > 0)
            {
                dir = Path.Combine(ConfigurationService.MorphsFemaleFolder, vmm.ModPath);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                foreach (var morph in femaleMorphs)
                {
                    morph.ExtractToFile(Path.Combine(dir, morph.Name));
                }

                vmm.InstallLog.Add(dir);
            }
            
            if(femaleGenitaliaMorphs.Count() > 0)
            {
                dir = Path.Combine(ConfigurationService.MorphsFemaleGenitaliaFolder, vmm.ModPath);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                foreach (var morph in femaleGenitaliaMorphs)
                {
                    morph.ExtractToFile(Path.Combine(dir, morph.Name));
                }

                vmm.InstallLog.Add(dir);
            }
        }

        public void ExtractScripts(ZipArchive zip, VmmItem vmm)
        {
            var scripts = zip.Entries.Where(e => e.Name.Contains(".cs") || e.Name.Contains(".cslist"));

            string dir;

            if (scripts.Count() > 0)
            {
                dir = Path.Combine(ConfigurationService.ScriptsFolder, vmm.ModPath);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!vmm.MaintainOriginalDirectoryStructure)
                {
                    foreach (var script in scripts)
                    {
                        script.ExtractToFile(Path.Combine(dir, script.Name));
                    }
                }
                else
                {
                    string targetPath = string.Empty;
                    string targetPathAndFile = string.Empty;
                    foreach (var script in scripts)
                    {
                        targetPathAndFile = Path.Combine(dir, script.FullName);
                        targetPath = targetPathAndFile.Replace(script.Name, string.Empty);

                        if (!Directory.Exists(targetPath))
                            Directory.CreateDirectory(targetPath);

                        script.ExtractToFile(targetPathAndFile);
                    }
                }

                vmm.InstallLog.Add(dir);
            }
        }

        public void ExtractTextures(ZipArchive zip, VmmItem vmm)
        {
            //png jpg
            var textures = zip.Entries.Where(e => e.Name.Contains(".jpg") || e.Name.Contains(".png"));

            string dir;

            if (textures.Count() > 0)
            {
                dir = Path.Combine(ConfigurationService.TexturesFolder, vmm.ModPath);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!vmm.MaintainOriginalDirectoryStructure)
                {
                    foreach (var texture in textures)
                    {
                        texture.ExtractToFile(Path.Combine(dir, texture.Name));
                    }
                }
                else
                {
                    string targetPath = string.Empty;
                    string targetPathAndFile = string.Empty;
                    foreach (var texture in textures)
                    {
                        targetPathAndFile = Path.Combine(dir, texture.FullName);
                        targetPath = targetPathAndFile.Replace(texture.Name, string.Empty);

                        if (!Directory.Exists(targetPath))
                            Directory.CreateDirectory(targetPath);

                        texture.ExtractToFile(targetPathAndFile);
                    }
                }

                vmm.InstallLog.Add(dir);
            }
        }

        //public void ExtractWardrobe(ZipArchive zip, VmmItem vmm)
        //{
        //    //png jpg
        //    var textures = zip.Entries.Where(e => e.Name.Contains(".jpg") || e.Name.Contains(".png"));

        //    string dir;

        //    if (textures.Count() > 0)
        //    {
        //        dir = Path.Combine(ConfigurationService.WardrobeFolder, vmm.ModPath);

        //        if (!Directory.Exists(dir))
        //            Directory.CreateDirectory(dir);

        //        if (!vmm.MaintainOriginalDirectoryStructure)
        //        {
        //            foreach (var texture in textures)
        //            {
        //                texture.ExtractToFile(Path.Combine(dir, texture.Name));
        //            }
        //        }
        //        else
        //        {
        //            string targetPath = string.Empty;
        //            string targetPathAndFile = string.Empty;
        //            foreach (var texture in textures)
        //            {
        //                targetPathAndFile = Path.Combine(dir, texture.FullName);
        //                targetPath = targetPathAndFile.Replace(texture.Name, string.Empty);

        //                if (!Directory.Exists(targetPath))
        //                    Directory.CreateDirectory(targetPath);

        //                texture.ExtractToFile(targetPathAndFile);
        //            }
        //        }

        //        vmm.InstallLog.Add(dir);
        //    }
        //}

        public void ValidateMorphs(ZipArchive zip)
        {

        }
    }
}
