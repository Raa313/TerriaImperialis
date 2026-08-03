using MonoMod;
using System.Linq;

namespace PavonisInteractive.TerraInvicta.Modding {
    internal class patch_ModManager : ModManager {

        [MonoModIgnore] private JsonController jsonController;

        [MonoModIgnore] private bool hitFailure;

        public extern void orig_LoadJsonMods();

        public void LoadJsonMods() {

            jsonMods.Clear();
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
                hitFailure = true;
                break;
            }
        }


    }
}
