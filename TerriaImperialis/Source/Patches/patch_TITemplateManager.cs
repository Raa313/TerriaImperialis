using MonoMod;
using Newtonsoft.Json.Linq;
using PavonisInteractive.TerraInvicta.Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;


namespace PavonisInteractive.TerraInvicta {
    internal class patch_TITemplateManager : TemplateManager {


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
        private static void RegisterFileBasedTemplate(string templateFile, bool replaceDuplicates = false) {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(templateFile);
            Type type = FindDataTemplateType(fileNameWithoutExtension);
            if (type == typeof(TIGlobalConfig)) {
                ((patch_TITemplateManager)self).ref_foundGlobal() = true;
            }
            if (type == null) {
                Debug.LogError("templateType is null for " + templateFile);
                return;
            }
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

                        if (item.TemplatesToReplaceArrays != null && item.TemplatesToReplaceArrays.Contains(fileNameWithoutExtension + ".json")) {
                            mergeArrayMode = MergeArrayHandling.Replace;
                            Debug.Log("Replacing array for Mod Template: " + item.ModFilePath);
                        }
                        list = ((item.TemplatesToReplace == null || !item.TemplatesToReplace.Contains(fileNameWithoutExtension + ".json")) ?
                            jController.CombineJson(list, item.FileContents, dlcFile, mergeArrayMode) : item.FileContents);
                        Debug.Log("TemplatesToReplace: " + (fileNameWithoutExtension) + " - " + (item.TemplatesToReplace == null ? "null" : String.Join(", ", item.TemplatesToReplace)));
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
