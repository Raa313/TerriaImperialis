using MonoMod;
using PavonisInteractive.TerraInvicta.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace PavonisInteractive.TerraInvicta {
    public class patch_TIFactionState : TIFactionState {
        //extern void orig_NewCampaign();
        //public void NewCampaign()
        //{
        //    System.IO.File.AppendAllText(
        //        @"C:\Users\Kaise\Desktop\ti_debug.txt",
        //        $"{System.DateTime.Now:O} - Entered NewCampaign\n");

        //    Log.Debug("Entered patch_TIFactionState.NewCampaign()");
        //    BepInEx.Logging.Logger.CreateLogSource("TerriaImperialis").LogDebug("Before NewCampaign");
        //    orig_NewCampaign();
        //    Log.Debug("Finished patch_TIFactionState.NewCampaign()");
        //}

        [MonoModIgnore][SerializeField] private bool gameStateSubjectCreated;
        [MonoModReplace]
        public void NewCampaign() {
            //IL_0775: Unknown result type (might be due to invalid IL or missing references)
            //IL_0781: Expected O, but got Unknown
            //IL_075e: Unknown result type (might be due to invalid IL or missing references)
            //IL_076a: Expected O, but got Unknown
            //IL_05d0: Unknown result type (might be due to invalid IL or missing references)
            //IL_05e2: Expected O, but got Unknown
            //IL_06dd: Unknown result type (might be due to invalid IL or missing references)
            //IL_06ea: Expected O, but got Unknown
            if (gameStateSubjectCreated) {
                return;
            }
            bool flag = GameControl.control.skirmishMode || TemplateManager.global.debug_advancedFactionStart || this.IsAlienFaction;
            List<string> list = (from y in GameStateManager.IterateByClass<TISpaceFleetState>(false).SelectMany((TISpaceFleetState x) => x.ships)
                                 select y.templateName).Distinct().ToList();
            foreach (TISpaceShipTemplate item in TemplateManager.IterateByClass<TISpaceShipTemplate>(true)) {
                if (!(item.designingFaction == this)) {
                    continue;
                }
                if (!flag) {
                    List<string> startingShipDesigns = GameStateManager.Time().template.startingShipDesigns;
                    if ((startingShipDesigns == null || !startingShipDesigns.Contains(item.dataName)) && !list.Contains(item.dataName)) {
                        continue;
                    }
                }
                item.SetClassDisplayName(false);
                base.shipDesigns.Add(item);
            }
            if (IsActiveHumanFaction) {
                RecruitInitialCouncilors();
                if (!GameControl.control.skirmishMode) {
                    GenerateRecruitableCouncilors(campaignStart: true);
                }
                if (player.isAI) {
                    Dictionary<TICouncilorState, Dictionary<FactionResource, float>> councilorIncomes =
                        councilors.ToDictionary((TICouncilorState x) => x,
                            (TICouncilorState y) => councilorResources.ToDictionary((FactionResource z) => z,
                            (FactionResource z) => y.GetMonthlyIncome(z))
                        );
                    //var councilorIncomes = new Dictionary<TICouncilorState, Dictionary<FactionResource, float>>();
                    AIDailyFactionPlanner.RecruitCouncilors(
                        this,
                        new List<TIMissionTemplate>(),
                        new List<TIMissionTemplate>(),
                        ref councilorIncomes,
                        chasingHydra: false,
                        0,
                        controllingNeutralPowers: true
                    );
                }
            }
            else {
                Dictionary<TIHabSiteState, float> bestNoblesOptions = (from x in (from x in GameStateManager.KuiperBeltObjects(false)
                                                                                  where ((TISpaceObjectState)x).semiMajorAxis_AU > 38.0 && (int)((TISpaceObjectState)x).objectType == 3
                                                                                  select x).SelectMany((TISpaceBodyState x) => x.habSites)
                                                                       where x.water_day > 0f && x.volatiles_day > 0f && x.metals_day > 0f && x.fissiles_day > 0f
                                                                       select x).ToDictionary((TIHabSiteState x) => x, (TIHabSiteState y) => y.GetDailyProduction((FactionResource)11));
                float maxNobles = bestNoblesOptions.Max((KeyValuePair<TIHabSiteState, float> x) => x.Value);
                TIHabSiteState tIHabSiteState = bestNoblesOptions.Keys.Where(x => bestNoblesOptions[x] >= maxNobles * 0.75f).MaxBy((TIHabSiteState x) => AIEvaluators.EvaluateHabSite(this, x));
                primaryHab = GameStateManager.FindByTemplate<TIHabState>("AlienHQ");
                primaryHab.habSite = tIHabSiteState;
                primaryHab.habSite.hab = primaryHab;
                primaryHab.barycenter = tIHabSiteState.parentBody;
                TIHabState tiHabState = GameStateManager.FindByTemplate<TIHabState>("AlienHQStation");
                if (tiHabState != null) {
                    TIOrbitState orbitState = (primaryHab.barycenter.ref_spaceBody.interfaceOrbits.Count <= 0) ? GameStateManager.FindByTemplate<TIOrbitState>("LowNeptuneOrbit") : primaryHab.barycenter.ref_spaceBody.interfaceOrbits[0];
                    tiHabState.SetRandomizedOrbitFromState(orbitState);
                }
                List<TIRegionState> listTiRegionState = new List<TIRegionState>();
                TIRegionState tIRegionState = GameStateManager.Time().template.InitialCrashdownRegion() ?? AIEvaluators.SelectAlienCrashdownRegion(advance: true);
                listTiRegionState.Add(tIRegionState);
                SetIntialPlanetaryConquestGoals(tIHabSiteState);
                tIRegionState.alienCrashdown.SetAsInitialCrashdownRegion();
                int aliensPreferredCouncilorCount = AIEvaluators.GetAliensPreferredCouncilorCount();
                for (int num = 0; num < aliensPreferredCouncilorCount; num++) {
                    TICouncilorState tICouncilorState = GameStateManager.CreateNewGameState<TICouncilorState>();
                    tICouncilorState.InitWithTemplate(TemplateManager.Find<TICouncilorTemplate>("randomizedAlienCouncilor1"));
                    tICouncilorState.NewCharacterGeneration(null, null, this, forceMaxStats: false, startup: true);
                    tICouncilorState.location.ref_hab.DepartCouncilor(tICouncilorState);
                    tICouncilorState.SetFaction(this);
                    tICouncilorState.SetRecruitDate();
                    base.councilors.Add(tICouncilorState);
                    TISpaceShipState tISpaceShipState = null;
                    switch (num) {
                        case 0:
                            tICouncilorState.SetLocation(tIRegionState);
                            break;
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            if (GameStateManager.Time().template.startingAlienCouncilorFleets.Where((string x) => !string.IsNullOrEmpty(x)).Count() >= num) {
                                TISpaceFleetState tISpaceFleetState = GameStateManager.FindByTemplate<TISpaceFleetState>(GameStateManager.Time().template.startingAlienCouncilorFleets[num - 1]);
                                if (tISpaceFleetState != null && tISpaceFleetState.ships.Count > 0 && tISpaceFleetState.ships.Any((TISpaceShipState x) => x.HasSpecialModuleRule(SpecialModuleRule.Crashdown))) {
                                    tISpaceShipState = tISpaceFleetState.ships.First((TISpaceShipState x) => x.HasSpecialModuleRule(SpecialModuleRule.Crashdown));
                                }
                            }
                            if (tISpaceShipState != null) {
                                tICouncilorState.SetLocation(tISpaceShipState);
                                TIRegionState tIRegionState2 = AIEvaluators.SelectAlienCrashdownRegion(advance: true);
                                AddGoal(new FactionGoal_TransportCouncilorsWithFleet(this, 15, new List<TICouncilorState> { tICouncilorState }, tIRegionState2), HandleDuplicateGoalRule.Ignore, tISpaceShipState.ref_fleet);
                                listTiRegionState.Add(tIRegionState2);
                            }
                            else {
                                tICouncilorState.SetLocation(primaryHab);
                                FactionGoal_TransportCouncilorsWithFleet prospectiveGoal = new FactionGoal_TransportCouncilorsWithFleet(this, 17, new List<TICouncilorState> { tICouncilorState }, AIEvaluators.SelectAlienCrashdownRegion(advance: true));
                                AddGoal(prospectiveGoal, HandleDuplicateGoalRule.Ignore);
                            }
                            break;
                        default:
                            tICouncilorState.SetLocation(primaryHab);
                            break;
                    }
                    if (num == 5) {
                        GrantNewOrgToCouncilor(tICouncilorState, TemplateManager.global.alienShockTroopOrgDataName);
                    }
                    //if (tISpaceShipState != null) {
                    //    tICouncilorState.SetLocation(tISpaceShipState);
                    //    TIRegionState tIRegionState2 = AIEvaluators.SelectAlienCrashdownRegion(true);
                    //    AddGoal(new FactionGoal_TransportCouncilorsWithFleet(this, 15, new List<TICouncilorState> { tICouncilorState }, tIRegionState2), HandleDuplicateGoalRule.Ignore, tISpaceShipState.ref_fleet);
                    //    listTiRegionState.Add(tIRegionState2);
                    //}
                    if (tICouncilorState.location == null) {
                        tICouncilorState.SetLocation(primaryHab);
                    }
                    SetIntel(tICouncilorState, TemplateManager.global.intelToSeeCouncilorSecrets);
                    if (string.IsNullOrEmpty(tICouncilorState.iconResource)) {
                        Log.Error("NO ICON FOR THIS GUY " + tICouncilorState.appearanceTemplateName);
                    }
                }
                //if (base.fleets.Any((TISpaceFleetState x) => x.HasSpecialModuleCapability((SpecialModuleRule)23))) {
                //    foreach (TISpaceFleetState item2 in base.fleets.Where((TISpaceFleetState x) => x.HasSpecialModuleCapability((SpecialModuleRule)23))) {
                //        ((TIFactionState)this).AddGoal((TIFactionGoalState)new FactionGoal_InvadeEarth((TIFactionState)(object)this, 19), (HandleDuplicateGoalRule)2, item2);
                //    }
                //}
                foreach (TISpaceFleetState item2 in fleets.Where((TISpaceFleetState x) => x.HasSpecialModuleCapability(SpecialModuleRule.LandArmy))) {
                    AddGoal(new FactionGoal_InvadeEarth(this, 19), HandleDuplicateGoalRule.ResetImportanceIfHigher, item2);
                }
                if (!GoalsOfType(GoalType.InvadeEarth).Any() && GameStateManager.Time().template.alienQuietDuration_years > 0f) {
                    AddGoal(new FactionGoal_InvadeEarth(this, 19), HandleDuplicateGoalRule.ResetImportanceIfHigher);
                }
                foreach (TIHabState hab in habs) {
                    if (hab.IsBase) {
                        AddGoal(new FactionGoal_BuildFullBase(this, 19, hab), HandleDuplicateGoalRule.Ignore);
                    }
                    else {
                        AddGoal(new FactionGoal_BuildFullStation(this, 18, hab), HandleDuplicateGoalRule.Ignore);
                    }
                }
            }
            //if (base.player.isAI && !((TIGameState)this).templateName.Contains("Vault_")) {
            //    foreach (TIHabState hab in ((TIFactionState)this).habs) {
            //        if (hab.IsBase) {
            //            ((TIFactionState)this).AddGoal((TIFactionGoalState)new FactionGoal_BuildFullBase((TIFactionState)(object)this, 10, hab), (HandleDuplicateGoalRule)0, (TISpaceFleetState)null);
            //        }
            //        else {
            //            ((TIFactionState)this).AddGoal((TIFactionGoalState)new FactionGoal_BuildFullStation((TIFactionState)(object)this, 10, hab), (HandleDuplicateGoalRule)0, (TISpaceFleetState)null);
            //        }
            //    }
            //}
            foreach (TISpaceShipState item3 in base.fleets.SelectMany((TISpaceFleetState x) => x.ships)) {
                if (item3.template.designingFaction == this && !base.shipDesigns.Contains(item3.template) && !templateName.Contains("Vault_")) {
                    SaveShipDesign(item3.template);
                }
            }
        }
    }
}
