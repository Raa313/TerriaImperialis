using MonoMod;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PavonisInteractive.TerraInvicta {
    internal class patch_TINationState : TINationState {

        [MonoModReplace]
        public void SetInitialPublicOpinion() {
            FactionIdeology undecided = GameStateManager.UndecidedIdeology().ideology;
            float num = cohesionRestState;
            float num2 = ((10f - democracy) * 3f + num * (float)(6 - numControlPoints + 1)) / 100f;
            float num3 = education * 6.5f + (float)TIUtilities.RandomRange(0, 60) - 3.5f * (10f - democracy);
            Dictionary<FactionIdeology, float> dictionary = new Dictionary<FactionIdeology, float>();
            foreach (TIFactionIdeologyTemplate item in GameStateManager.ActiveHumanIdeologies()) {
                //Log.Debug("Initial Ideology seed: " + item.ideology + " = " + ((item.ideology == FactionIdeology.Undecided) ? 1 : 0));
                dictionary.Add(item.ideology, (item.ideology == FactionIdeology.Undecided) ? 1 : 0);
            }
            List<TIFactionIdeologyTemplate> list = (from x in GameStateManager.ActiveHumanIdeologies()
                                                    where x.initialReactionGroup == 0
                                                    select x).ToList();
            List<TIFactionIdeologyTemplate> list2 = (from x in GameStateManager.ActiveHumanIdeologies()
                                                     where x.initialReactionGroup == 1
                                                     select x).ToList();
            List<TIFactionIdeologyTemplate> list3 = (from x in GameStateManager.ActiveHumanIdeologies()
                                                     where x.initialReactionGroup == 2
                                                     select x).ToList();
            //Log.Debug($"Lists created: {list.Count} in group 0, {list2.Count} in group 1, {list3.Count} in group 2");
            if (num3 <= 25f && list.Count > 0) {
                TIFactionIdeologyTemplate tIFactionIdeologyTemplate = list.SelectRandomItem();
                dictionary[tIFactionIdeologyTemplate.ideology] = num2;
                if (democracy > 3.5f || cohesion < 8f) {
                    list.Remove(tIFactionIdeologyTemplate);
                }
                //Log.Debug("After first assignment: (num3 <= 25f && list.Count > 0)");
            }
            else if (num3 < 65f || democracy <= 2f) {
                //Log.Debug("Second assignment: (num3 < 65f || democracy <= 2f)");
                dictionary[FactionIdeology.Undecided] = num2;
            }
            else if (list2.Count > 0) {
                //Log.Debug("Third assignment: (list2.Count > 0)");
                TIFactionIdeologyTemplate tIFactionIdeologyTemplate2 = list.SelectRandomItem();
                dictionary[tIFactionIdeologyTemplate2.ideology] = num2;
                if (democracy > 3.5f || cohesion < 8f) {
                    list2.Remove(tIFactionIdeologyTemplate2);
                }
            }
            //Log.Debug($"After initial assignments: ");
            float num4 = 1f - num2;
            float num5 = education * 2f + democracy + (float)TIUtilities.RandomRange(0, 80);
            float num6 = Mathf.Min(num2 * 0.75f, TIUtilities.RandomRange(0.15f, 0.3f)) * num4;
            if (num5 <= 30f && list.Count > 0) {
                dictionary[list.SelectRandomItem().ideology] += num6;
            }
            else if (num5 < 60f && list3.Count > 0) {
                dictionary[list3.SelectRandomItem().ideology] += num6;
            }
            else if (list2.Count > 0) {
                dictionary[list2.SelectRandomItem().ideology] += num6;
            }
            num4 -= num6;
            foreach (TIFactionIdeologyTemplate item2 in GameStateManager.ActiveHumanIdeologies().ToList().Shuffle()) {
                if (item2.ideology != FactionIdeology.Undecided) {
                    if (list.Contains(item2)) {
                        dictionary[item2.ideology] += Mathf.Clamp((20f - education - (float)TIUtilities.RandomRange(0, 10)) / 100f, 0f, num4 * ((float)list.Count / 30f));
                    }
                    else if (list3.Contains(item2)) {
                        dictionary[item2.ideology] += Mathf.Clamp(education + (float)TIUtilities.RandomRange(0, 10) / 500f, 0f, num4 * ((float)list3.Count / 60f));
                    }
                    else if (list2.Contains(item2)) {
                        dictionary[item2.ideology] += Mathf.Clamp(2f * education + (float)TIUtilities.RandomRange(0, 10) / 100f, 0f, num4 * ((float)list2.Count / 6.5f));
                    }
                }
            }
            //Log.Debug($"After distribution: ");
            //float num7;
            //for (num7 = dictionary.Sum((KeyValuePair<FactionIdeology, float> x) => x.Value) - dictionary[FactionIdeology.Undecided]; num7 > 1f; num7 = dictionary.Sum((KeyValuePair<FactionIdeology, float> x) => x.Value) - dictionary[FactionIdeology.Undecided]) {
            //    Log.Debug($"Before clamping: ");
            //    TIFactionIdeologyTemplate tIFactionIdeologyTemplate3 = GameStateManager.ActiveHumanIdeologies().Except(new List<TIFactionIdeologyTemplate> { GameStateManager.UndecidedIdeology() }).SelectRandomItem();
            //    dictionary[tIFactionIdeologyTemplate3.ideology] = Mathf.Clamp(dictionary[tIFactionIdeologyTemplate3.ideology] - 0.025f, 0f, 1f);
            //    if (0 == 0) {
            //        Log.Info("Initial Ideology seed hit total of " + num7);
            //    }
            //}

            float num7;
            for (num7 = dictionary.Sum((KeyValuePair<FactionIdeology, float> x) => x.Value) - dictionary[undecided]; num7 > 1f; num7 = dictionary.Sum((KeyValuePair<FactionIdeology, float> x) => x.Value) - dictionary[undecided]) {
                TIFactionIdeologyTemplate tIFactionIdeologyTemplate3 = GameStateManager.ActiveHumanIdeologies().Except(new List<TIFactionIdeologyTemplate> { GameStateManager.UndecidedIdeology() }).SelectRandomItem();
                dictionary[tIFactionIdeologyTemplate3.ideology] = Mathf.Clamp(dictionary[tIFactionIdeologyTemplate3.ideology] - 0.025f, 0f, 1f);
                Log.Info("Initial Ideology seed hit total of " + num7);
            }

            //// 1. Dump every key currently in the dictionary before touching FactionIdeology.Undecided at all.
            //Log.Debug($"Dictionary key count: {dictionary.Count}");
            //foreach (var kvp in dictionary) {
            //    Log.Debug($"  dictionary key: {kvp.Key} (hash {kvp.Key.GetHashCode()}) = {kvp.Value}");
            //}

            //// 2. Check the two things that could be different "Undecided" references.
            //TIFactionIdeologyTemplate undecidedTemplate = GameStateManager.UndecidedIdeology();
            //Log.Debug($"UndecidedIdeology() returned null: {undecidedTemplate == null}");
            //if (undecidedTemplate != null) {
            //    Log.Debug($"UndecidedIdeology().dataName: {undecidedTemplate.dataName}, .ideology: {undecidedTemplate.ideology} (hash {undecidedTemplate.ideology.GetHashCode()})");
            //}
            //Log.Debug($"FactionIdeology.Undecided static value: {FactionIdeology.Undecided} (hash {FactionIdeology.Undecided.GetHashCode()})");

            //// 3. Explicit ContainsKey check before ever indexing with it directly.
            //bool hasUndecidedKey = dictionary.ContainsKey(FactionIdeology.Undecided);
            //Log.Debug($"dictionary.ContainsKey(FactionIdeology.Undecided): {hasUndecidedKey}");

            //if (undecidedTemplate != null) {
            //    bool hasUndecidedTemplateKey = dictionary.ContainsKey(undecidedTemplate.ideology);
            //    Log.Debug($"dictionary.ContainsKey(undecidedTemplate.ideology): {hasUndecidedTemplateKey}");
            //}

            //// 4. Only now attempt the real computation, guarded, so we get a clean log instead of a raw crash.
            //float sumAll = dictionary.Sum((KeyValuePair<FactionIdeology, float> x) => x.Value);
            //Log.Debug($"Sum of all dictionary values: {sumAll}");

            //float num7;
            //if (hasUndecidedKey) {
            //    num7 = sumAll - dictionary[FactionIdeology.Undecided];
            //    Log.Debug($"num7 initial value: {num7}");

            //    for (; num7 > 1f; num7 = dictionary.Sum((KeyValuePair<FactionIdeology, float> x) => x.Value) - dictionary[FactionIdeology.Undecided]) {
            //        Log.Debug($"Before clamping: ");
            //        TIFactionIdeologyTemplate tIFactionIdeologyTemplate3 = GameStateManager.ActiveHumanIdeologies().Except(new List<TIFactionIdeologyTemplate> { GameStateManager.UndecidedIdeology() }).SelectRandomItem();
            //        dictionary[tIFactionIdeologyTemplate3.ideology] = Mathf.Clamp(dictionary[tIFactionIdeologyTemplate3.ideology] - 0.025f, 0f, 1f);
            //        Log.Info("Initial Ideology seed hit total of " + num7);
            //    }
            //}
            //else {
            //    Log.Debug("ABORTING: FactionIdeology.Undecided is not a key in dictionary. See key dump above to see what IS in there.");
            //    num7 = 0f;
            //}


            //Log.Debug($"After clamping: ");
            dictionary[FactionIdeology.Undecided] = 1f - num7;
            foreach (TIFactionIdeologyTemplate item3 in GameStateManager.ActiveHumanIdeologies()) {
                publicOpinion.Add(item3.ideology, dictionary[item3.ideology]);
            }
            GameStateManager.AllFactions().ToList().ForEach(delegate (TIFactionState x) {
                x.SetResourceIncomeDataDirty(FactionResource.Influence);
            });
        }






    }
}
