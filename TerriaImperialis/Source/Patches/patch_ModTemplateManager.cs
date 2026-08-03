using MonoMod;
using PavonisInteractive.TerraInvicta.Modding;
using System.Linq;

namespace PavonisInteractive.TerraInvicta {
    internal class patch_ModTemplateManager : ModTemplateManager {

        [MonoModIgnore] private static JsonController jsonController;

        public static void LoadJsonMods() {
            Log.Debug("Loading Json Mods ModTemplateManager");
            jsonMods.Clear();
            Log.Debug("Setting Up Directories");
            foreach (string item in (from s in GetEnabledModFiles()
                                     where s.Contains(".json") && !s.Contains("ModInfo.json")
                                     select s).ToList()) {
                if (jsonController == null) {
                    jsonController = new JsonController();
                }
                JsonMod jsonMod = jsonController.LoadJson(item);
                if (jsonMod != null) {
                    jsonMods.Add(jsonMod);
                    Log.Debug("Mod Template Found: " + jsonMod.ModFilePath);
                    continue;
                }
                Log.Debug("Mod Manager Failed on: " + item);
                break;
            }
        }



    }
}
