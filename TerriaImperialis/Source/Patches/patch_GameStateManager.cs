//using MonoMod;
//using System.Collections.Generic;
//using System.Linq;

////Debug
//namespace PavonisInteractive.TerraInvicta {
//    public static class patch_GameStateManager {

//        private static List<TIFactionIdeologyTemplate> activeIdeologies;

//        [MonoModReplace]
//        public static List<TIFactionIdeologyTemplate> ActiveIdeologies() {
//            Log.Debug("=== ActiveIdeologies ===");

//            List<TIFactionIdeologyTemplate> active =
//                GameStateManager.AllFactions()
//                    .Select(x => x.ideology)
//                    .ToList();

//            Log.Debug($"Faction ideology count: {active.Count}");

//            foreach (TIFactionIdeologyTemplate ideology in active) {
//                Log.Debug(
//                    $"  dataName={ideology.dataName}, " +
//                    $"ideology={ideology.ideology}, " +
//                    $"sortOrder={ideology.sortOrder}"
//                );
//            }

//            TIFactionIdeologyTemplate undecided =
//                GameStateManager.UndecidedIdeology();

//            Log.Debug(
//                $"  UNDIDED: dataName={undecided.dataName}, " +
//                $"ideology={undecided.ideology}, " +
//                $"sortOrder={undecided.sortOrder}"
//            );

//            active.Add(undecided);

//            Log.Debug("=== Testing ToDictionary ===");

//            foreach (var group in active.GroupBy(x => x.ideology)) {
//                Log.Debug(
//                    $"Ideology key {group.Key}: {group.Count()} entries"
//                );

//                foreach (var ideology in group) {
//                    Log.Debug($"    {ideology.dataName}");
//                }
//            }

//            active = active.OrderBy(x => x.sortOrder).ToList();

//            return active;
//        }
//    }
//}
