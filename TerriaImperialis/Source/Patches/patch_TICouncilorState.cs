using MonoMod;
using System;
using System.Text;

namespace PavonisInteractive.TerraInvicta {
    public class patch_TICouncilorState : TICouncilorState {
        //public extern bool orig_ValidDestination(TIGameState candidateDestination, out string reason);
        //public bool ValidDestination(TIGameState candidateDestination, out string reason)
        //{
        //    Log.Debug("Before ValidDestination");
        //    bool result = orig_ValidDestination(candidateDestination, out reason);
        //    Log.Debug("After ValidDestination");

        //    return result;

        //}

        [MonoModReplace]
        public bool ValidDestination(TIGameState candidateDestination, out string reason) {

            if (candidateDestination == (TIGameState)null || candidateDestination.deleted) {
                reason = "UI.Councilor.MoveFail_NoTarget";
                return false;
            }
            if (candidateDestination.isRegionState || candidateDestination.isHabState || candidateDestination.isSpaceShipState) {
                TIRegionState tIRegionState = (candidateDestination.isRegionState ? candidateDestination.ref_region : null);
                TIHabState tIHabState = (candidateDestination.isHabState ? candidateDestination.ref_hab : null);
                TISpaceShipState tISpaceShipState = (candidateDestination.isSpaceShipState ? candidateDestination.ref_ship : null);
                bool undercoverFlag = false;
                if (isAlien) {
                    if (candidateDestination.isRegionState && !tIRegionState.AllowedDestinationForAlienCouncilor(this)) {
                        reason = "UI.Councilor.MoveFail_Alien";
                        return false;
                    }
                    if (OnEarth && candidateDestination.inSpace) {
                        reason = "UI.Councilor.MoveFail_Alien";
                        return false;
                    }
                    if (candidateDestination.isHabState && !candidateDestination.ref_hab.IsAlien()) {
                        reason = "UI.Councilor.MoveFail_Alien";
                        return false;
                    }
                    if (candidateDestination.isSpaceShipState && !candidateDestination.ref_ship.isAlien) {
                        reason = "UI.Councilor.MoveFail_Alien";
                        return false;
                    }
                }
                if (candidateDestination.isHabState) {
                    //if (!(isAlien && candidateDestination.ref_hab.IsAlien() && !candidateDestination.ref_hab.AllModules().Any((TIHabModuleState x) => x.moduleTemplate.SpecialRules.Contains((HabModuleSpecialRule)1100))) {
                    if (!isAlien && candidateDestination.ref_hab.IsAlien()) {
                        if (candidateDestination.ref_hab != candidateDestination.ref_faction.primaryHab) {
                            reason = "UI.Councilor.MoveFail_AlienNotPrimaryHab";
                            return false;
                        }
                        if (faction.GetObjectivesByTypeAndStatus(ObjectiveType.Victory, ObjectiveStatus.Unlocked).None((TIObjectiveTemplate x) => x.targetMissionTarget == ObjectiveMissionTargetType.AlienHQ)) {
                            reason = "UI.Councilor.MoveFail_AlienPrimaryHabObjectiveLocked";
                            return false;
                        }
                        if (candidateDestination.ref_hab.ActiveCombatModules().Count > 0) {
                            reason = "UI.Councilor.MoveFail_AlienPrimaryHabObjectiveHasCombatModules";
                            return false;
                        }
                        reason = "Valid";
                        return true;
                    }
                }
                else if (candidateDestination.isSpaceShipState && !isAlien && candidateDestination.ref_ship.isAlien) {
                    reason = "UI.Councilor.MoveFail_AlienShip";
                    return false;
                }
                foreach (TITraitTemplate trait in traits) {
                    if (trait == null) {
                        continue;
                    }
                    if (candidateDestination.isRegionState) {
                        switch (trait.restrictedLocations) {
                            case RestrictedLocations.HomeNation:
                                if (tIRegionState.nation == homeNation) {
                                    reason = new StringBuilder("UI.Councilor.MoveFail_").Append(trait.restrictedLocations.ToString()).ToString();
                                    return false;
                                }
                                break;
                            case RestrictedLocations.HomeNationAndAllies:
                                if (tIRegionState.nation == homeNation || tIRegionState.nation.IsAlliedWith(tIRegionState.nation)) {
                                    reason = new StringBuilder("UI.Councilor.MoveFail_").Append(trait.restrictedLocations.ToString()).ToString();
                                    return false;
                                }
                                break;
                            case RestrictedLocations.HomeNationRivals:
                                if (homeRegion.nation.IsRivalWith(tIRegionState.nation) || homeRegion.nation.IsAtWarWith(tIRegionState.nation)) {
                                    reason = new StringBuilder("UI.Councilor.MoveFail_").Append(trait.restrictedLocations.ToString()).ToString();
                                    return false;
                                }
                                break;
                            case RestrictedLocations.HomeNationWarOpponents:
                                if (homeRegion.nation.IsAtWarWith(tIRegionState.nation)) {
                                    reason = new StringBuilder("UI.Councilor.MoveFail_").Append(trait.restrictedLocations.ToString()).ToString();
                                    return false;
                                }
                                break;
                            case RestrictedLocations.HighUnrestNations:
                                if (tIRegionState.nation.unrest >= TemplateManager.global.HighUnrestDefinition) {
                                    reason = new StringBuilder("UI.Councilor.MoveFail_").Append(trait.restrictedLocations.ToString()).ToString();
                                    return false;
                                }
                                break;
                        }
                    }
                    else if (trait.restrictedLocations == RestrictedLocations.Space) {
                        reason = new StringBuilder("UI.Councilor.MoveFail_").Append(trait.restrictedLocations.ToString()).ToString();
                        return false;
                    }
                    if (trait.specialTraitRule == SpecialTraitRule.Undercover) {
                        undercoverFlag = true;
                    }
                }
                if (!undercoverFlag) {
                    if (candidateDestination.isHabState && tIHabState.tier == 1 && tIHabState.faction != faction) {
                        reason = new StringBuilder("UI.Councilor.MoveFail_Undercover").ToString();
                        return false;
                    }
                    if (candidateDestination.isSpaceShipState && tISpaceShipState.fleet.faction != faction) {
                        reason = new StringBuilder("UI.Councilor.MoveFail_Undercover").ToString();
                        return false;
                    }
                }
                reason = "Valid";
                return true;
            }
            reason = "Can't figure out where councilor " + ((TIGameState)this).displayName + " is considering as destination to go, passed GS: " + ((object)candidateDestination).ToString();
            Log.Error(reason, Array.Empty<object>());
            return false;
        }
    }

}
