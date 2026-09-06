using MonoMod;
using Newtonsoft.Json.Linq;
using PavonisInteractive.TerraInvicta.Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace PavonisInteractive.TerraInvicta {
    internal class patch_TemplateManager : TemplateManager {


        //[MonoModIgnore]
        //private bool foundGlobal;

        [MonoModIgnore]
        private bool foundGlobal;

        public ref bool ref_foundGlobal() {
            return ref foundGlobal;
        }

        [MonoModIgnore]
        private static JsonController jController = new JsonController();

        [MonoModReplace]
        private static void RegisterFileBasedTemplates(string templatePath, bool replaceDuplicates = false) {
            Debug.Log("RegisterFileBasedTemplates Template Path: " + templatePath);
            if (!Directory.Exists(templatePath)) {
                Log.Error("TemplateManager.InitTemplates -- could not find template path " + templatePath);
                return;
            }
            string[] files = Directory.GetFiles(templatePath, "*.json", SearchOption.AllDirectories);
            IEnumerable<string> second = Enumerable.Empty<string>();
            if (Directory.Exists("DLC_Content")) {
                second = Directory.GetFiles("DLC_Content", "*.json", SearchOption.AllDirectories);
            }
            string[] array = (from x in files.Concat(second).ToArray()
                              orderby x.Contains("TIMetaTemplate.json") descending
                              select x).ToArray();
            for (int num = 0; num < array.Length; num++) {
                RegisterFileBasedTemplate(array[num], replaceDuplicates);
            }
        }


        [MonoModReplace]
        private static void RegisterFileBasedTemplate(string templateFile, bool replaceDuplicates = false) {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(templateFile);
            Type type = FindDataTemplateType(fileNameWithoutExtension);
            if (type == typeof(TIGlobalConfig)) {
                ((patch_TemplateManager)self).ref_foundGlobal() = true;
            }
            if (type == null) {
                Debug.LogError("templateType is null for " + templateFile);
                return;
            }

            Debug.Log("fileNameWithoutExtension: " + fileNameWithoutExtension);
            TIDataTemplate[] array;
            if (TIPlayerProfileManager.useMods) {
                List<JsonMod> modsForTemplate = ModTemplateManager.GetModsForTemplate(fileNameWithoutExtension);
                if (modsForTemplate.Count > 0) {
                    bool dlcFile = templateFile.Contains("DLC_Content");
                    List<JObject> list = jController.LoadJson(templateFile).FileContents;
                    foreach (JsonMod item in modsForTemplate) {
                        MergeArrayHandling mergeArrayMode = MergeArrayHandling.Merge;
                        if (item.TemplatesToConcatArrays != null && item.TemplatesToConcatArrays.Contains(fileNameWithoutExtension + ".json")) {
                            mergeArrayMode = MergeArrayHandling.Concat;
                        }

                        Debug.Log("Merging Mod Template: " + item.ModFilePath);
                        Debug.Log(item.FileContents.ToString());
                        if (item.TemplatesToReplaceArrays != null) {
                            Debug.Log("TemplatesToReplaceArrays: " + string.Join(", ", item.TemplatesToReplaceArrays));
                        }
                        if (item.TemplatesToReplace != null) {
                            Debug.Log("TemplatesToReplace: " + string.Join(", ", item.TemplatesToReplace));
                        }

                        if (item.TemplatesToReplaceArrays != null && item.TemplatesToReplaceArrays.Contains(fileNameWithoutExtension + ".json")) {
                            mergeArrayMode = MergeArrayHandling.Replace;
                            Debug.Log("Replacing array for Mod Template: " + item.ModFilePath);
                        }
                        list = ((item.TemplatesToReplace == null || !item.TemplatesToReplace.Contains(fileNameWithoutExtension + ".json")) ?
                            jController.CombineJson(list, item.FileContents, dlcFile, mergeArrayMode) : item.FileContents);
                        Debug.Log("Successfully Merged Mod Template: " + item.ModFilePath);
                        Debug.Log(" ------ ");
                        item.SetFoundMatch();
                    }
                    array = FSSaveLoad.LoadDataTemplatesFromString(jController.jObjectListToString(list), type.MakeArrayType());
                }
                else {
                    array = FSSaveLoad.LoadDataTemplates(templateFile, type.MakeArrayType());
                }
            }
            else {
                array = FSSaveLoad.LoadDataTemplates(templateFile, type.MakeArrayType());
            }
            TIDataTemplate[] array2 = array;
            foreach (TIDataTemplate tIDataTemplate in array2) {
                if (tIDataTemplate.dataName != null) {
                    Add(tIDataTemplate, type, replaceDuplicates);
                }
                else {
                    Debug.Log("Attempting to add template of type " + type.ToString() + " with null dataName;");
                }
            }
        }


        //Private Reference for monomod
        private static Type FindDataTemplateType(string templateName, Assembly assembly = null) {
            if (assembly == null) {
                assembly = Assembly.GetExecutingAssembly();
            }
            Type type = assembly.GetType(templateName);
            if (type == null) {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++) {
                    type = assemblies[i].GetType(templateName);
                    if (type != null) {
                        return type;
                    }
                }
                if (type == null) {
                    Debug.LogWarning("Did not find Type for: " + templateName + " in extended assembly search");
                }
            }
            if (type == null) {
                Debug.LogWarning("Did not find Type for: " + templateName);
            }
            return type;
        }




    }
}
