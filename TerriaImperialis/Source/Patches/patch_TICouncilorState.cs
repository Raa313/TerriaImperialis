using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod;
using PavonisInteractive;

namespace PavonisInteractive.TerraInvicta
{
    public class patch_TICouncilorState : TICouncilorState
    {
        public extern bool orig_ValidDestination(TIGameState candidateDestination, out string reason);
        public bool ValidDestination(TIGameState candidateDestination, out string reason)
        {
            Log.Debug("Before ValidDestination");
            bool result = orig_ValidDestination(candidateDestination, out reason);
            Log.Debug("After ValidDestination");

            return result;

        }

        //[MonoModReplace]
        //public bool ValidDestination(TIGameState candidateDestination, out string reason)
        //{

        //    //IL_0517: Unknown result type (might be due to invalid IL or missing references)
        //    //IL_051d: Invalid comparison between Unknown and I4
        //    //IL_032a: Unknown result type (might be due to invalid IL or missing references)
        //    //IL_032f: Unknown result type (might be due to invalid IL or missing references)
        //    //IL_0331: Unknown result type (might be due to invalid IL or missing references)
        //    //IL_0333: Unknown result type (might be due to invalid IL or missing references)
        //    //IL_0335: Unknown result type (might be due to invalid IL or missing references)
        //    //IL_0338: Unknown result type (might be due to invalid IL or missing references)
        //    //IL_0352: Expected I4, but got Unknown
        //    //IL_0557: Unknown result type (might be due to invalid IL or missing references)
        //    //IL_055d: Invalid comparison between Unknown and I4
        //    if (candidateDestination == (TIGameState)null || candidateDestination.deleted)
        //    {
        //        reason = "UI.Councilor.MoveFail_NoTarget";
        //        return false;
        //    }
        //    if (candidateDestination.isRegionState || candidateDestination.isHabState || candidateDestination.isSpaceShipState)
        //    {
        //        TIRegionState val = (candidateDestination.isRegionState ? candidateDestination.ref_region : null);
        //        TIHabState val2 = (candidateDestination.isHabState ? candidateDestination.ref_hab : null);
        //        TISpaceShipState val3 = (candidateDestination.isSpaceShipState ? candidateDestination.ref_ship : null);
        //        bool flag = false;
        //        if (((TICouncilorState)this).isAlien)
        //        {
        //            if (candidateDestination.isRegionState && !val.AllowedDestinationForAlienCouncilor((TICouncilorState)(object)this))
        //            {
        //                reason = "UI.Councilor.MoveFail_Alien";
        //                return false;
        //            }
        //            if (((TICouncilorState)this).OnEarth && candidateDestination.inSpace)
        //            {
        //                reason = "UI.Councilor.MoveFail_Alien";
        //                return false;
        //            }
        //            if (candidateDestination.isHabState && !((TISpaceAssetState)candidateDestination.ref_hab).IsAlien())
        //            {
        //                reason = "UI.Councilor.MoveFail_Alien";
        //                return false;
        //            }
        //            if (candidateDestination.isSpaceShipState && !candidateDestination.ref_ship.isAlien)
        //            {
        //                reason = "UI.Councilor.MoveFail_Alien";
        //                return false;
        //            }
        //        }
        //        if (candidateDestination.isHabState)
        //        {
        //            if (!((TICouncilorState)this).isAlien && ((TISpaceAssetState)candidateDestination.ref_hab).IsAlien() && !candidateDestination.ref_hab.AllModules().Any((TIHabModuleState x) => x.moduleTemplate.SpecialRules.Contains((HabModuleSpecialRule)1100)))
        //            {
        //                if ((TIGameState)(object)candidateDestination.ref_hab != (TIGameState)(object)candidateDestination.ref_faction.primaryHab)
        //                {
        //                    reason = "UI.Councilor.MoveFail_AlienNotPrimaryHab";
        //                    return false;
        //                }
        //                if (EnumerableExtensions.None<TIObjectiveTemplate>((IEnumerable<TIObjectiveTemplate>)((TICouncilorState)this).faction.GetObjectivesByTypeAndStatus((ObjectiveType)3, (ObjectiveStatus)1), (Func<TIObjectiveTemplate, bool>)((TIObjectiveTemplate x) => (int)x.targetMissionTarget == 14)))
        //                {
        //                    reason = "UI.Councilor.MoveFail_AlienPrimaryHabObjectiveLocked";
        //                    return false;
        //                }
        //                if (candidateDestination.ref_hab.ActiveCombatModules().Count > 0)
        //                {
        //                    reason = "UI.Councilor.MoveFail_AlienPrimaryHabObjectiveHasCombatModules";
        //                    return false;
        //                }
        //                reason = "Valid";
        //                return true;
        //            }
        //            if (candidateDestination.ref_hab.AllModules().Any((TIHabModuleState x) => x.moduleTemplate.SpecialRules.Contains((HabModuleSpecialRule)1100)))
        //            {
        //                if (candidateDestination.ref_hab.ActiveCombatModules().Count > 0)
        //                {
        //                    reason = "UI.Councilor.MoveFail_AlienPrimaryHabObjectiveHasCombatModules";
        //                    return false;
        //                }
        //                reason = "Valid";
        //                return true;
        //            }
        //        }
        //        else if (candidateDestination.isSpaceShipState && !((TICouncilorState)this).isAlien && candidateDestination.ref_ship.isAlien)
        //        {
        //            reason = "UI.Councilor.MoveFail_AlienShip";
        //            return false;
        //        }
        //        foreach (TITraitTemplate trait in ((TICouncilorState)this).traits)
        //        {
        //            if (trait == null)
        //            {
        //                continue;
        //            }
        //            if (candidateDestination.isRegionState)
        //            {
        //                RestrictedLocations restrictedLocations = trait.restrictedLocations;
        //                RestrictedLocations val4 = restrictedLocations;
        //                switch ((RestrictedLocations)((int)val4 - 1))
        //                {
        //                    case RestrictedLocations.HomeNation:
        //                        if ((TIGameState)(object)val.nation == (TIGameState)(object)((TICouncilorState)this).homeNation)
        //                        {
        //                            reason = new StringBuilder("UI.Councilor.MoveFail_").Append(trait.restrictedLocations.ToString()).ToString();
        //                            return false;
        //                        }
        //                        break;
        //                    case RestrictedLocations.HomeNationAndAllies:
        //                        if ((TIGameState)(object)val.nation == (TIGameState)(object)((TICouncilorState)this).homeNation || val.nation.IsAlliedWith(val.nation, false))
        //                        {
        //                            reason = new StringBuilder("UI.Councilor.MoveFail_").Append(trait.restrictedLocations.ToString()).ToString();
        //                            return false;
        //                        }
        //                        break;
        //                    case RestrictedLocations.HomeNationRivals:
        //                        if (base.homeRegion.nation.IsRivalWith(val.nation) || base.homeRegion.nation.IsAtWarWith(val.nation))
        //                        {
        //                            reason = new StringBuilder("UI.Councilor.MoveFail_").Append(trait.restrictedLocations.ToString()).ToString();
        //                            return false;
        //                        }
        //                        break;
        //                    case RestrictedLocations.HomeNationWarOpponents:
        //                        if (base.homeRegion.nation.IsAtWarWith(val.nation))
        //                        {
        //                            reason = new StringBuilder("UI.Councilor.MoveFail_").Append(trait.restrictedLocations.ToString()).ToString();
        //                            return false;
        //                        }
        //                        break;
        //                    case RestrictedLocations.HighUnrestNations:
        //                        if (val.nation.unrest >= TemplateManager.global.HighUnrestDefinition)
        //                        {
        //                            reason = new StringBuilder("UI.Councilor.MoveFail_").Append(trait.restrictedLocations.ToString()).ToString();
        //                            return false;
        //                        }
        //                        break;
        //                }
        //            }
        //            else if ((int)trait.restrictedLocations == 6)
        //            {
        //                reason = new StringBuilder("UI.Councilor.MoveFail_").Append(trait.restrictedLocations.ToString()).ToString();
        //                return false;
        //            }
        //            if ((int)trait.specialTraitRule == 4)
        //            {
        //                flag = true;
        //            }
        //        }
        //        if (!flag)
        //        {
        //            if (candidateDestination.isHabState && val2.tier == 1 && (TIGameState)(object)((TISpaceAssetState)val2).faction != (TIGameState)(object)((TICouncilorState)this).faction)
        //            {
        //                reason = new StringBuilder("UI.Councilor.MoveFail_Undercover").ToString();
        //                return false;
        //            }
        //            if (candidateDestination.isSpaceShipState && (TIGameState)(object)((TISpaceAssetState)val3.fleet).faction != (TIGameState)(object)((TICouncilorState)this).faction)
        //            {
        //                reason = new StringBuilder("UI.Councilor.MoveFail_Undercover").ToString();
        //                return false;
        //            }
        //        }
        //        reason = "Valid";
        //        return true;
        //    }
        //    reason = "Can't figure out where councilor " + ((TIGameState)this).displayName + " is considering as destination to go, passed GS: " + ((object)candidateDestination).ToString();
        //    Log.Error(reason, Array.Empty<object>());
        //    return false;
        //}
    }

}
