using MonoMod;
using PavonisInteractive.TerraInvicta.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;



namespace PavonisInteractive.TerraInvicta
{
    public class patch_TIFactionState : TIFactionState
    {
        extern void orig_NewCampaign();
        public void NewCampaign()
        {
            System.IO.File.AppendAllText(
                @"C:\Users\Kaise\Desktop\ti_debug.txt",
                $"{System.DateTime.Now:O} - Entered NewCampaign\n");

            Log.Debug("Entered patch_TIFactionState.NewCampaign()");
            BepInEx.Logging.Logger.CreateLogSource("TerriaImperialis").LogDebug("Before NewCampaign");
            orig_NewCampaign();
            Log.Debug("Finished patch_TIFactionState.NewCampaign()");
        }
    }
}

        //[MonoModIgnore]
        //[SerializeField]
        //private bool gameStateSubjectCreated;
        //[MonoModReplace]
        //public void NewCampaign()
        //{
        //    UnityEngine.Debug.Log("Entered patch_TIFactionState.NewCampaign()");
        //    Debug.Log("NewCampaign() called for faction: " + ((TIGameState)this).templateName);
        //    Log.Debug("NewCampaign() called for faction: " + ((TIGameState)this).templateName, Array.Empty<object>());
        //    //IL_0775: Unknown result type (might be due to invalid IL or missing references)
        //    //IL_0781: Expected O, but got Unknown
        //    //IL_075e: Unknown result type (might be due to invalid IL or missing references)
        //    //IL_076a: Expected O, but got Unknown
        //    //IL_05d0: Unknown result type (might be due to invalid IL or missing references)
        //    //IL_05e2: Expected O, but got Unknown
        //    //IL_06dd: Unknown result type (might be due to invalid IL or missing references)
        //    //IL_06ea: Expected O, but got Unknown
        //    if (gameStateSubjectCreated)
        //    {
        //        return;
        //    }
        //    bool flag = GameControl.control.skirmishMode || TemplateManager.global.debug_advancedFactionStart || ((TIFactionState)this).IsAlienFaction;
        //    List<string> list = (from y in GameStateManager.IterateByClass<TISpaceFleetState>(false).SelectMany((TISpaceFleetState x) => x.ships)
        //                         select ((TIGameState)y).templateName).Distinct().ToList();
        //    foreach (TISpaceShipTemplate item in TemplateManager.IterateByClass<TISpaceShipTemplate>(true))
        //    {
        //        if (!((TIGameState)(object)item.designingFaction == (TIGameState)(object)this))
        //        {
        //            continue;
        //        }
        //        if (!flag)
        //        {
        //            List<string> startingShipDesigns = GameStateManager.Time().template.startingShipDesigns;
        //            if ((startingShipDesigns == null || !startingShipDesigns.Contains(((TIDataTemplate)item).dataName)) && !list.Contains(((TIDataTemplate)item).dataName))
        //            {
        //                continue;
        //            }
        //        }
        //        item.SetClassDisplayName(false);
        //        base.shipDesigns.Add(item);
        //    }
        //    if (((TIFactionState)this).IsActiveHumanFaction && !((TIGameState)this).templateName.Contains("Vault_"))
        //    {
        //        ((TIFactionState)this).RecruitInitialCouncilors();
        //        if (!GameControl.control.skirmishMode)
        //        {
        //            ((TIFactionState)this).GenerateRecruitableCouncilors(true);
        //        }
        //        if (base.player.isAI)
        //        {
        //            var councilorIncomes = new Dictionary<TICouncilorState, Dictionary<FactionResource, float>>();
        //            AIDailyFactionPlanner.RecruitCouncilors(
        //                (TIFactionState)(object)this,
        //                new List<TIMissionTemplate>(),
        //                new List<TIMissionTemplate>(),
        //                ref councilorIncomes,
        //                false,
        //                0,
        //                false
        //            );
        //        }
        //    }
        //    else if (!((TIGameState)this).templateName.Contains("Vault_"))
        //    {
        //        Dictionary<TIHabSiteState, float> bestNoblesOptions = (from x in (from x in GameStateManager.KuiperBeltObjects(false)
        //                                                                          where ((TISpaceObjectState)x).semiMajorAxis_AU > 38.0 && (int)((TISpaceObjectState)x).objectType == 3
        //                                                                          select x).SelectMany((TISpaceBodyState x) => x.habSites)
        //                                                               where x.water_day > 0f && x.volatiles_day > 0f && x.metals_day > 0f && x.fissiles_day > 0f
        //                                                               select x).ToDictionary((TIHabSiteState x) => x, (TIHabSiteState y) => y.GetDailyProduction((FactionResource)11));
        //        float maxNobles = bestNoblesOptions.Max((KeyValuePair<TIHabSiteState, float> x) => x.Value);
        //        TIHabSiteState val = EnumerableExtensions.MaxBy<TIHabSiteState, float>(bestNoblesOptions.Keys.Where((TIHabSiteState x) => bestNoblesOptions[x] >= maxNobles * 0.75f), (Func<TIHabSiteState, float>)((TIHabSiteState x) => AIEvaluators.EvaluateHabSite((TIFactionState)(object)this, x, false, false, true)));
        //        base.primaryHab = GameStateManager.FindByTemplate<TIHabState>("AlienHQ", false);
        //        base.primaryHab.habSite = val;
        //        base.primaryHab.habSite.hab = base.primaryHab;
        //        ((TISpaceGameState)base.primaryHab).barycenter = (TINaturalSpaceObjectState)(object)val.parentBody;
        //        TIHabState val2 = GameStateManager.FindByTemplate<TIHabState>("AlienHQStation", false);
        //        if ((TIGameState)(object)val2 != (TIGameState)null)
        //        {
        //            TIOrbitState val3 = ((((TIGameState)((TISpaceGameState)base.primaryHab).barycenter).ref_spaceBody.interfaceOrbits.Count <= 0) ? GameStateManager.FindByTemplate<TIOrbitState>("LowNeptuneOrbit", false) : ((TIGameState)((TISpaceGameState)base.primaryHab).barycenter).ref_spaceBody.interfaceOrbits[0]);
        //            ((TISpaceAssetState)val2).SetRandomizedOrbitFromState(val3, true);
        //        }
        //        List<TIRegionState> list2 = new List<TIRegionState>();
        //        TIRegionState val4 = GameStateManager.Time().template.InitialCrashdownRegion() ?? AIEvaluators.SelectAlienCrashdownRegion(true);
        //        list2.Add(val4);
        //        ((TIFactionState)this).SetIntialPlanetaryConquestGoals(val);
        //        val4.alienCrashdown.SetAsInitialCrashdownRegion();
        //        for (int num = 0; num < 6; num++)
        //        {
        //            TICouncilorState val5 = GameStateManager.CreateNewGameState<TICouncilorState>();
        //            ((TIGameState)val5).InitWithTemplate((TIDataTemplate)(object)TemplateManager.Find<TICouncilorTemplate>("randomizedAlienCouncilor1", false));
        //            val5.NewCharacterGeneration((TICouncilorTypeTemplate)null, (TIRegionState)null, (TIFactionState)null, false, true);
        //            val5.location.ref_hab.DepartCouncilor(val5);
        //            val5.SetFaction((TIFactionState)(object)this);
        //            val5.SetRecruitDate();
        //            base.councilors.Add(val5);
        //            TISpaceShipState val6 = null;
        //            switch (num)
        //            {
        //                case 0:
        //                    val5.SetLocation((TIGameState)(object)val4);
        //                    break;
        //                case 1:
        //                    {
        //                        TISpaceFleetState val8 = GameStateManager.FindByTemplate<TISpaceFleetState>("alienFleet12020", false);
        //                        val6 = (((TIGameState)(object)val8 != (TIGameState)null) ? val8.ships[0] : null);
        //                        break;
        //                    }
        //                case 2:
        //                    {
        //                        TISpaceFleetState val10 = GameStateManager.FindByTemplate<TISpaceFleetState>("alienFleet22020", false);
        //                        val6 = (((TIGameState)(object)val10 != (TIGameState)null) ? val10.ships[0] : null);
        //                        break;
        //                    }
        //                case 3:
        //                    {
        //                        TISpaceFleetState val9 = GameStateManager.FindByTemplate<TISpaceFleetState>("alienFleet32020", false);
        //                        val6 = (((TIGameState)(object)val9 != (TIGameState)null) ? val9.ships[0] : null);
        //                        break;
        //                    }
        //                case 4:
        //                    {
        //                        TISpaceFleetState val11 = GameStateManager.FindByTemplate<TISpaceFleetState>("alienFleet42020", false);
        //                        val6 = (((TIGameState)(object)val11 != (TIGameState)null) ? val11.ships[0] : null);
        //                        break;
        //                    }
        //                case 5:
        //                    {
        //                        TISpaceFleetState val7 = GameStateManager.FindByTemplate<TISpaceFleetState>("alienFleet52020", false);
        //                        val6 = (((TIGameState)(object)val7 != (TIGameState)null) ? val7.ships[0] : null);
        //                        ((TIFactionState)this).GrantNewOrgToCouncilor(val5, TemplateManager.global.alienShockTroopOrgDataName);
        //                        break;
        //                    }
        //                default:
        //                    val5.SetLocation((TIGameState)(object)base.primaryHab);
        //                    break;
        //            }
        //            if ((TIGameState)(object)val6 != (TIGameState)null)
        //            {
        //                val5.SetLocation((TIGameState)(object)val6);
        //                TIRegionState val12 = AIEvaluators.SelectAlienCrashdownRegion(true);
        //                ((TIFactionState)this).AddGoal((TIFactionGoalState)new FactionGoal_TransportCouncilorsWithFleet((TIFactionState)(object)this, 15, new List<TICouncilorState> { val5 }, (TIGameState)(object)val12), (HandleDuplicateGoalRule)0, ((TIGameState)val6).ref_fleet);
        //                list2.Add(val12);
        //            }
        //            if (val5.location == (TIGameState)null)
        //            {
        //                val5.SetLocation((TIGameState)(object)base.primaryHab);
        //            }
        //            ((TIFactionState)this).SetIntel((TIGameState)(object)val5, TemplateManager.global.intelToSeeCouncilorSecrets, (TIGameState)null);
        //            if (string.IsNullOrEmpty(val5.iconResource))
        //            {
        //                Log.Error("NO ICON FOR THIS GUY " + val5.appearanceTemplateName, Array.Empty<object>());
        //            }
        //        }
        //        if (base.fleets.Any((TISpaceFleetState x) => x.HasSpecialModuleCapability((SpecialModuleRule)23)))
        //        {
        //            foreach (TISpaceFleetState item2 in base.fleets.Where((TISpaceFleetState x) => x.HasSpecialModuleCapability((SpecialModuleRule)23)))
        //            {
        //                ((TIFactionState)this).AddGoal((TIFactionGoalState)new FactionGoal_InvadeEarth((TIFactionState)(object)this, 19), (HandleDuplicateGoalRule)2, item2);
        //            }
        //        }
        //    }
        //    if (base.player.isAI && !((TIGameState)this).templateName.Contains("Vault_"))
        //    {
        //        foreach (TIHabState hab in ((TIFactionState)this).habs)
        //        {
        //            if (hab.IsBase)
        //            {
        //                ((TIFactionState)this).AddGoal((TIFactionGoalState)new FactionGoal_BuildFullBase((TIFactionState)(object)this, 10, hab), (HandleDuplicateGoalRule)0, (TISpaceFleetState)null);
        //            }
        //            else
        //            {
        //                ((TIFactionState)this).AddGoal((TIFactionGoalState)new FactionGoal_BuildFullStation((TIFactionState)(object)this, 10, hab), (HandleDuplicateGoalRule)0, (TISpaceFleetState)null);
        //            }
        //        }
        //    }
        //    foreach (TISpaceShipState item3 in base.fleets.SelectMany((TISpaceFleetState x) => x.ships))
        //    {
        //        if ((TIGameState)(object)item3.template.designingFaction == (TIGameState)(object)this && !base.shipDesigns.Contains(item3.template) && !((TIGameState)this).templateName.Contains("Vault_"))
        //        {
        //            ((TIFactionState)this).SaveShipDesign(item3.template);
        //        }
        //    }
        //}
//    }
//}
