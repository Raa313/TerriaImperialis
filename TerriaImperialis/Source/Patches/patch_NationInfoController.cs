//using MonoMod;

//namespace PavonisInteractive.TerraInvicta {
//    internal class patch_NationInfoController : NationInfoController {


//        [MonoModReplace]
//        public static string BuildPublicOpinionLine(TINationState nation, FactionIdeology ideology, bool region = false) {

//            float publicOpinionOfFaction = nation.GetPublicOpinionOfFaction(ideology);
//            nation.historyPublicOpinion[31].TryGetValue(ideology, out var value);
//            //if (ideology == FactionIdeology.Undecided) {
//            //    return Loc.T(region ? "UI.Region.PublicOpinionLineNoFaction" : "UI.Nation.PublicOpinionLineNoFaction", GameStateManager.UndecidedIdeology().ideologyStrPublicOpinion, nation.GetPublicOpinionOfFaction(FactionIdeology.Undecided).ToPercent("P0"), numberToArrow(publicOpinionOfFaction - value, WhatIsGood.downIsGood));
//            //}
//            FactionIdeology undecided = GameStateManager.UndecidedIdeology().ideology;
//            if (ideology == undecided) {
//                return Loc.T(region ? "UI.Region.PublicOpinionLineNoFaction" : "UI.Nation.PublicOpinionLineNoFaction", GameStateManager.UndecidedIdeology().ideologyStrPublicOpinion, nation.GetPublicOpinionOfFaction(undecided).ToPercent("P0"), numberToArrow(publicOpinionOfFaction - value, WhatIsGood.downIsGood));
//            }

//            TIFactionState factionByIdeology = TIFactionIdeologyTemplate.GetFactionByIdeology(ideology);
//            return Loc.T(region ? "UI.Region.PublicOpinionLineFaction" : "UI.Nation.PublicOpinionLineFaction", factionByIdeology.ideology.ideologyStrPublicOpinion, factionByIdeology.template.inlineColorString, factionByIdeology.displayName, publicOpinionOfFaction.ToPercent("P0"), numberToArrow(publicOpinionOfFaction - value, (!(factionByIdeology == GameControl.control.activePlayer)) ? WhatIsGood.downIsGood : WhatIsGood.upIsGood));
//        }

//    }
//}
