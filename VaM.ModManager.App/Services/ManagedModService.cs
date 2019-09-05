using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using VaM.ModManager.App.Enums;
using VaM.ModManager.App.Models;

namespace VaM.ModManager.App.Services
{
    public class ManagedModService
    {
        public void CreateMod(VmmItem vmm)
        {
            vmm.ModType = ModTypeEnum.InDevelopment;

            var paths = ListModFolders(vmm);
            CreateModFolders(paths);

            //save the inital vmm file to the Vam Mod Manager Folder
            new ModService().SaveVmmFile(vmm);

            //copy the default scene to use as a starting point
            var defaultScenePath = Path.Combine(ConfigurationService.SceneFolderMeshedVR, @"default.json");
            var modScenePath = Path.Combine(ConfigurationService.SceneFolder, vmm.ModPath);
            modScenePath = Path.Combine(modScenePath, $"{vmm.ModName}.json");

            File.Copy(defaultScenePath, modScenePath);
        }

        public void DeleteMod(VmmItem vmm)
        {
            var paths = ListModFolders(vmm);
            DeleteModFolders(paths);
        }

        public void InstallMod(string vacPath)
        {
            if (File.Exists(vacPath))
            {
                ZipFile.ExtractToDirectory(vacPath, ConfigurationService.Instance.VamLocation);
            }
        }

        public void FinalizeMod(VmmItem vmm, List<ModFolderItem> excludedModFolders)
        {
            if (vmm.ModType == ModTypeEnum.InDevelopment)
                throw new Exception("You can not finalize a mod when the type is set to In Development.");

            //create vac file
            string finalVacPath = ConfigurationService.Instance.ModLocation;
            string finalVacPathAndFilename = Path.Combine(finalVacPath, vmm.VacFilename);

            //create empty temp directory to target zipfile at
            var finalizePath = ConfigurationService.FinalizationFolder;
            Directory.CreateDirectory(finalizePath);

            if(!File.Exists(finalVacPathAndFilename))
            {
                ZipFile.CreateFromDirectory(finalizePath, finalVacPathAndFilename);
            }

            using (var zip = ZipFile.Open(finalVacPathAndFilename, ZipArchiveMode.Update))
            {
                //NOTE: This is a bit of a hack.  Since the path is pulled from the Vmm item itself, we need to trick it into thinking it is still in development
                //so we can grab all the files from the dev folders...
                //Should find a better solution later.
                var cacheType = vmm.ModType;
                vmm.ModType = ModTypeEnum.InDevelopment;

                //look in mod dev folders then set the type back
                var paths = ListModFolders(vmm, false);

                vmm.ModType = cacheType;

                //if not empty copy path and contents to vac
                foreach (var p in paths)
                {
                    //if author choose to exclude this folder, skip it and continue
                    string absPath = Path.GetFullPath(p.FolderPath);
                    if (excludedModFolders.Any(f => absPath.Contains(f.FolderPath)))
                        continue;

                    if (Directory.Exists(p.FolderPath))
                    {
                        var files = Directory.GetFiles(p.FolderPath, "*", SearchOption.AllDirectories);

                        if (files.Length > 0)
                        {
                            foreach (var file in files)
                            {
                                if (file.Contains(".json"))
                                {
                                    FinalizeJson(file, zip, vmm);
                                    continue;
                                }

                                if(file.Contains(".vmm"))
                                {
                                    FinalizeVmm(zip, vmm);
                                    continue;
                                }

                                string newFile = file.Replace($"{ConfigurationService.Instance.VamLocation + @"\"}", string.Empty);
                                newFile = newFile.Replace(@"\dev\", $"{@"\" + vmm.ModVersion.ToString("0.0#") + @"\"}");
                                
                                zip.CreateEntryFromFile(file, newFile, CompressionLevel.Fastest);
                            }
                        }
                    }
                }
            }

            Directory.Delete(finalizePath, true);

            //finally, increment the version number for dev.
            vmm.ModType = ModTypeEnum.InDevelopment;
            vmm.ModVersion += 0.1f;
            new ModService().SaveVmmFile(vmm);
        }

        private void FinalizeJson(string jsonPath, ZipArchive zip, VmmItem vmm)
        {
            string json = string.Empty;
            using (var reader = new StreamReader(jsonPath))
            {
                json = reader.ReadToEnd();
            }

            json = json.Replace(@"\dev\", $"\\{vmm.ModVersion.ToString("0.0#")}\\");

            string tempPath = Path.Combine(ConfigurationService.FinalizationFolder, Path.GetFileName(jsonPath));

            using (var writer = new StreamWriter(tempPath, false))
            {
                writer.Write(json);
                writer.Flush();
            }

            string tempFile = jsonPath.Replace($"{ConfigurationService.Instance.VamLocation + @"\"}", string.Empty);
            tempFile = tempFile.Replace(@"\dev\", $"{@"\" + vmm.ModVersion.ToString("0.0#") + @"\"}");

            zip.CreateEntryFromFile(tempPath, tempFile, CompressionLevel.Fastest);
        }

        private void FinalizeVmm(ZipArchive zip, VmmItem vmm)
        {
            new ModService().SaveVmmFile(vmm, true);
            string vmmPath = Path.Combine(ConfigurationService.FinalizationFolder, vmm.ModPathAndFilename);
            string zipPath = ConfigurationService.VamModManagerFolder.Replace($"{ConfigurationService.Instance.VamLocation + @"\"}", string.Empty);
            zipPath = Path.Combine(zipPath, vmm.ModPathAndFilename);
            zip.CreateEntryFromFile(vmmPath, zipPath, CompressionLevel.Fastest);
        }

        public List<ModFolderItem> ListBaseVamFolders()
        {
            List<ModFolderItem> paths = new List<ModFolderItem>
            {
                new ModFolderItem{ DisplayName="Female Clothing", FolderPath=ConfigurationService.ClothingFemaleFolder },
                new ModFolderItem{ DisplayName="Female Morphs", FolderPath=ConfigurationService.MorphsFemaleFolder },
                new ModFolderItem{ DisplayName="Female Genitalia Morphs", FolderPath=ConfigurationService.MorphsFemaleGenitaliaFolder },
                new ModFolderItem{ DisplayName="Look", FolderPath=ConfigurationService.PersonAppearanceFolder },
                new ModFolderItem{ DisplayName="Male Clothing", FolderPath=ConfigurationService.ClothingMaleFolder },
                new ModFolderItem{ DisplayName="Male Morphs", FolderPath=ConfigurationService.MorphsMaleFolder },
                new ModFolderItem{ DisplayName="Male Genitalia Morphs", FolderPath=ConfigurationService.MorphsMaleGenitaliaFolder },
                new ModFolderItem{ DisplayName="Pose", FolderPath=ConfigurationService.PersonPoseFolder },
                new ModFolderItem{ DisplayName="Scene", FolderPath=ConfigurationService.SceneFolder },
                new ModFolderItem{ DisplayName="Scripts", FolderPath=ConfigurationService.ScriptsFolder },
                new ModFolderItem{ DisplayName="Textures", FolderPath=ConfigurationService.TexturesFolder },
                new ModFolderItem{ DisplayName="VaM Mod Manager", FolderPath=ConfigurationService.VamModManagerFolder },
            };

            return paths;
        }

        public List<ModFolderItem> ListModFolders(VmmItem vmm, bool isFinalization = false)
        {
            var paths = ListBaseVamFolders();

            for (int i = 0; i < paths.Count; i++)
            {
                paths[i].FolderPath = Path.Combine(paths[i].FolderPath, vmm.ModPath);

                if (isFinalization)
                {
                    paths[i].FolderPath = paths[i].FolderPath.Replace(ConfigurationService.Instance.VamLocation,
                        ConfigurationService.FinalizationFolder);
                }
            }

            return paths;
        }

        public List<ModFolderItem> ListEmptyModFolders(VmmItem vmm)
        {
            var paths = ListModFolders(vmm);
            
            for (int i = 0; i < paths.Count; i++)
            {
                bool hasContent = Directory.GetFiles(paths[i].FolderPath).Any() || Directory.GetDirectories(paths[i].FolderPath).Any();
                paths[i].IsExcludedFromFinalization = !hasContent;
            }

            return paths;
        }

        public void CreateModFolders(List<ModFolderItem> paths)
        {
            foreach (var path in paths)
            {
                Directory.CreateDirectory(path.FolderPath);
            }
        }

        public void DeleteModFolders(List<ModFolderItem> paths)
        {
            foreach (var path in paths)
            {
                if(Directory.Exists(path.FolderPath))
                {
                    try
                    {
                        //delete version folder
                        Directory.Delete(path.FolderPath, true);

                        //if there are no other versions of this mod, delete the mod folder
                        if(DeleteParentFolder(path.FolderPath))
                        {
                            var parent = Directory.GetParent(path.FolderPath).FullName;
                            DeleteParentFolder(parent);
                        }
                    }
                    catch(DirectoryNotFoundException)
                    {
                        //log exception
                    }
                    catch(UnauthorizedAccessException)
                    {
                        //log exception
                    }
                }
            }
        }

        private bool DeleteParentFolder(string path)
        {
            var parent = Directory.GetParent(path);
            if (parent.GetFiles().Count() == 0 && parent.GetDirectories().Count() == 0)
            {
                Directory.Delete(parent.FullName);
                return true;
            }

            return false;
        }
    }
}
