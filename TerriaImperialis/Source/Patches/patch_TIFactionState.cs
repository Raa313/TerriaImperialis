using MonoMod;
using PavonisInteractive.TerraInvicta.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Debug Class

namespace PavonisInteractive.TerraInvicta {
    internal class patch_TIFactionState : TIFactionState {

        [MonoModIgnore]
        [SerializeField]
        private bool gameStateSubjectCreated;


        [MonoModReplace]
        public void NewCampaign() {
            Log.Debug($"=== NewCampaign ===");
            if (gameStateSubjectCreated) {
                Log.Debug("gameStateSubjectCreated is true, returning");
                return;
            }
            bool flag = GameControl.control.skirmishMode || TemplateManager.global.debug_advancedFactionStart;
            List<string> list = (from y in GameStateManager.IterateByClass<TISpaceFleetState>().SelectMany((TISpaceFleetState x) => x.ships)
                                 select y.templateName).Distinct().ToList();
            foreach (TISpaceShipTemplate item in TemplateManager.IterateByClass<TISpaceShipTemplate>()) {
                if (!(item.designingFaction == this)) {
                    continue;
                }
                if (!flag) {
                    List<string> startingShipDesigns = GameStateManager.Time().template.startingShipDesigns;
                    if ((startingShipDesigns == null || !startingShipDesigns.Contains(item.dataName)) && !list.Contains(item.dataName)) {
                        continue;
                    }
                }
                item.SetClassDisplayName();
                shipDesigns.Add(item);
            }
            Log.Debug("TISpaceShipTemplate designs loaded");
            if (IsActiveHumanFaction) {
                Log.Debug("IsActiveHumanFaction is true, recruiting initial councilors");
                RecruitInitialCouncilors();
                if (!GameControl.control.skirmishMode) {
                    GenerateRecruitableCouncilors(campaignStart: true);
                }
                if (player.isAI) {
                    Dictionary<TICouncilorState, Dictionary<FactionResource, float>> councilorIncomes = councilors.ToDictionary((TICouncilorState x) => x, (TICouncilorState y) => councilorResources.ToDictionary((FactionResource z) => z, (FactionResource z) => y.GetMonthlyIncome(z)));
                    AIDailyFactionPlanner.RecruitCouncilors(this, new List<TIMissionTemplate>(), new List<TIMissionTemplate>(), ref councilorIncomes, chasingHydra: false, 0, controllingNeutralPowers: true);
                }
                Log.Debug("RecruitInitialCouncilors complete if (IsActiveHumanFaction");
            }
            else {
                Log.Debug("IsActiveHumanFaction is false, initializing alien faction");
                Dictionary<TIHabSiteState, float> bestNoblesOptions = (from x in (from x in GameStateManager.KuiperBeltObjects(includeSatellites: false)
                                                                                  where x.semiMajorAxis_AU > 38.0 && x.apoapsis_AU < 55.0 && x.objectType == SpaceObjectType.DwarfPlanet
                                                                                  select x).SelectMany((TISpaceBodyState x) => x.habSites)
                                                                       where x.water_day > 0f && x.volatiles_day > 0f && x.metals_day > 0f && x.fissiles_day > 0f
                                                                       select x).ToDictionary((TIHabSiteState x) => x, (TIHabSiteState y) => y.GetDailyProduction(FactionResource.NobleMetals));
                float maxNobles = bestNoblesOptions.Max((KeyValuePair<TIHabSiteState, float> x) => x.Value);
                TIHabSiteState tIHabSiteState = bestNoblesOptions.Keys.Where((TIHabSiteState x) => bestNoblesOptions[x] >= maxNobles * 0.75f).MaxBy((TIHabSiteState x) => AIEvaluators.EvaluateHabSite(this, x));
                //primaryHab = GameStateManager.FindByTemplate<TIHabState>("AlienHQ");
                primaryHab = GameStateManager.FindByTemplate<TIHabState>("XenosHQ");
                Log.Debug($"Found hab site for alien faction: {primaryHab}");
                primaryHab.habSite = tIHabSiteState;
                primaryHab.habSite.hab = primaryHab;
                primaryHab.barycenter = tIHabSiteState.parentBody;
                //TIHabState tIHabState = GameStateManager.FindByTemplate<TIHabState>("AlienHQStation");
                TIHabState tIHabState = GameStateManager.FindByTemplate<TIHabState>("XenosHQStation");
                Log.Debug($"Found hab state for alien faction: {tIHabState}");
                if (tIHabState != null) {
                    TIOrbitState orbitState = ((primaryHab.barycenter.ref_spaceBody.interfaceOrbits.Count <= 0) ? GameStateManager.FindByTemplate<TIOrbitState>("LowNeptuneOrbit") : primaryHab.barycenter.ref_spaceBody.interfaceOrbits[0]);
                    tIHabState.SetRandomizedOrbitFromState(orbitState);
                }
                List<TIRegionState> list2 = new List<TIRegionState>();
                TIRegionState tIRegionState = GameStateManager.Time().template.InitialCrashdownRegion() ?? AIEvaluators.SelectAlienCrashdownRegion(advance: true);
                list2.Add(tIRegionState);
                SetIntialPlanetaryConquestGoals(tIHabSiteState);
                tIRegionState.alienCrashdown.SetAsInitialCrashdownRegion();
                int aliensPreferredCouncilorCount = AIEvaluators.GetAliensPreferredCouncilorCount();
                for (int num = 0; num < aliensPreferredCouncilorCount; num++) {
                    TICouncilorState tICouncilorState = GameStateManager.CreateNewGameState<TICouncilorState>();
                    //tICouncilorState.InitWithTemplate(TemplateManager.Find<TICouncilorTemplate>("randomizedAlienCouncilor1"));
                    tICouncilorState.InitWithTemplate(TemplateManager.Find<TICouncilorTemplate>("randomizedAlienCouncilor3"));
                    tICouncilorState.NewCharacterGeneration(null, null, this, forceMaxStats: false, startup: true);
                    tICouncilorState.location.ref_hab.DepartCouncilor(tICouncilorState);
                    tICouncilorState.SetFaction(this);
                    tICouncilorState.SetRecruitDate();
                    councilors.Add(tICouncilorState);
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
                                list2.Add(tIRegionState2);
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
                    if (tICouncilorState.location == null) {
                        tICouncilorState.SetLocation(primaryHab);
                    }
                    SetIntel(tICouncilorState, TemplateManager.global.intelToSeeCouncilorSecrets);
                }
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
                Log.Debug("Alien faction initialization complete if (IsActiveHumanFaction) is false");
            }
            foreach (TISpaceShipState item3 in fleets.SelectMany((TISpaceFleetState x) => x.ships)) {
                if (item3.template.designingFaction == this && !shipDesigns.Contains(item3.template)) {
                    SaveShipDesign(item3.template);
                }
            }
            Log.Debug("Ship designs saved");
        }

        public override void PostVisualizerCreationInit_7() {
            if (!gameStateSubjectCreated) {
                Log.Debug("PostVisualizerCreationInit_7: gameStateSubjectCreated is false, initializing faction state");
                TIFactionState[] array = GameStateManager.AllFactions();
                foreach (TIFactionState enemyCouncil in array) {
                    GainFactionHate(enemyCouncil, 0f, cantConflagrate: false, "Initialization");
                }
                TIRegionUFOCrashdownState tIRegionUFOCrashdownState = null;
                foreach (TIRegionUFOCrashdownState item in GameStateManager.IterateByClass<TIRegionUFOCrashdownState>()) {
                    if (item.crashdownPresent) {
                        tIRegionUFOCrashdownState = item;
                        break;
                    }
                }
                Log.Debug("PostVisualizerCreationInit_7: Logging campaign start and unlocked objectives");
                if (!GameControl.control.skirmishMode) {
                    if (tIRegionUFOCrashdownState != null) {
                        TINotificationQueueState.LogCampaignStart(this, tIRegionUFOCrashdownState.region);
                    }
                    foreach (TIObjectiveTemplate item2 in GetObjectivesByStatus(ObjectiveStatus.Unlocked)) {
                        if (item2.objectiveType != ObjectiveType.General && !item2.isChildObjective) {
                            TINotificationQueueState.LogObjectiveUnlocked(this, item2);
                        }
                    }
                    if (IsActiveHumanFaction) {
                        TIUtilities.InitRandom();
                        GenerateOrgsForAcquisition(campaignStart: true);
                        AddToCurrentResource(GameStateManager.Time().template.bonusMoney, FactionResource.Money);
                        AddToCurrentResource(GameStateManager.Time().template.bonusInfluence, FactionResource.Influence);
                        AddToCurrentResource(GameStateManager.Time().template.bonusOps, FactionResource.Operations);
                        AddToCurrentResource(GameStateManager.Time().template.bonusBoost, FactionResource.Boost);
                        AddToCurrentResource(GameStateManager.Time().template.bonusWater, FactionResource.Water);
                        AddToCurrentResource(GameStateManager.Time().template.bonusVolatiles, FactionResource.Volatiles);
                        AddToCurrentResource(GameStateManager.Time().template.bonusMetals, FactionResource.Metals);
                        AddToCurrentResource(GameStateManager.Time().template.bonusNobles, FactionResource.NobleMetals);
                        AddToCurrentResource(GameStateManager.Time().template.bonusFissiles, FactionResource.Fissiles);
                        AddToCurrentResource(GameStateManager.Time().template.bonusAntimatter, FactionResource.Antimatter);
                        AddToCurrentResource(GameStateManager.Time().template.bonusExotics, FactionResource.Exotics);
                        foreach (TIProjectTemplate item3 in TemplateManager.IterateByClass<TIProjectTemplate>()) {
                            if (item3.FactionPrereqsSatisfied(this) && item3.GetResearchCost(this) <= 0f) {
                                OnProjectComplete(item3, -1, suppressLogging: true);
                            }
                        }
                        Debug.Log("PostVisualizerCreationInit_7: Checking for completed projects");
                        foreach (string item4 in GameStateManager.Time().template.projectsCompleted.Where((string x) => !string.IsNullOrEmpty(x))) {
                            TIProjectTemplate tIProjectTemplate = TemplateManager.Find<TIProjectTemplate>(item4);
                            if (tIProjectTemplate.FactionPrereqsSatisfied(this)) {
                                OnProjectComplete(tIProjectTemplate, -1, suppressLogging: true);
                            }
                        }
                        Log.Debug("PostVisualizerCreationInit_7: Checking for available projects");
                        if (!IsAlienFaction) {
                            foreach (TIProjectTemplate item5 in TemplateManager.IterateByClass<TIProjectTemplate>()) {
                                List<string> prereqs = item5.prereqs;
                                if (prereqs != null && prereqs.Count == 0 && !ProjectAlreadyTriggered(item5) && item5.FactionPrereqsSatisfied(this)) {
                                    RollToAddProjectTrigger(item5);
                                }
                            }
                        }
                    }
                    if (player.isAI && updateShipDesignsFlag) {
                        AIDailyFactionPlanner.DesignShips(this);
                    }
                }
            }
            foreach (TISpaceShipTemplate shipDesign in shipDesigns) {
                shipDesign.CacheTemplateValues();
            }
            unassignedOrgs = unassignedOrgs.Distinct().ToList();
            CheckForOrgProjectStatusChange();
            CheckforHabProjectUnlock();
            CachePriorityBonuses_Day();
            CheckForMissedProjectProject();
            gameStateSubjectCreated = true;
            if (!GameControl.control.skirmishMode && !IsAlienFaction) {
                string text = displayName + " Missing Projects: ";
                foreach (TIProjectTemplate allProject in TIGlobalResearchState.GetAllProjects()) {
                    if (!ProjectAlreadyTriggered(allProject) && allProject.factionAvailableChance >= 100f && allProject.PrereqsSatisfied(TIGlobalResearchState.FinishedTechs(), completedProjects, this)) {
                        text = text + allProject.dataName + "\n";
                        AddAvailableProject(allProject);
                    }
                }
                if (text.Contains("\n")) {
                    Log.Error(text);
                }
                foreach (KeyValuePair<TIObjectiveTemplate, ObjectiveStatus> item6 in objectives.Where((KeyValuePair<TIObjectiveTemplate, ObjectiveStatus> x) => x.Value == ObjectiveStatus.Unlocked).ToList()) {
                    if (completedProjectsDistinct.Contains(item6.Key.targetProjectTemplate)) {
                        CheckForObjectivesCompleteViaProject(item6.Key.targetProjectTemplate);
                        Log.Error("REPAIR: Objective " + item6.Key.displayName(this) + " not completed despite project " + item6.Key.targetProjectTemplate.displayName + " completed by " + displayName);
                    }
                }
                if (isActivePlayer) {
                    Log.Time("<color=#00cc00>LoadTime:</color> CacheAllTechTooltipStrings", delegate {
                        CacheAllTechTooltipStrings();
                    });
                }
                if (!isActivePlayer) {
                    HumanHabPlanner.ManageMineNetwork(this);
                }
            }
            if (GameControl.control.skirmishMode) {
                return;
            }
            foreach (TIHabModuleState item7 in nShipyardQueues.Keys.ToList()) {
                if (item7.active && nShipyardQueues[item7].Count > 0 && nShipyardQueues[item7][0].costPaid && nShipyardQueues[item7][0].daysToCompletion > 0f) {
                    GameControl.eventManager.TriggerEvent(new ShipConstructionUpdated(this, item7, nShipyardQueues[item7][0]), null, this, item7);
                }
            }
        }



        //[MonoModReplace]
        //public void AddAvailableCouncilor(TICouncilorState councilor, bool forced = false) {
        //    councilor.SetFaction(this);
        //    TIDateTime tIDateTime = null;
        //    if (councilor.recruitDate != null) {
        //        tIDateTime = new TIDateTime(councilor.recruitDate);
        //    }
        //    councilor.SetRecruitDate();
        //    councilors.Add(councilor);
        //    availableCouncilors.Remove(councilor);
        //    if (IsAlienFaction) {
        //        SetIntel(councilor, TemplateManager.global.intelToSeeCouncilorSecrets);
        //        if (!councilor.OnEarth || TIEffectsState.CheckForAnyEffectInContext(Context.ManyAliensOnEarth, this)) {
        //            GrantNewOrgToCouncilor(councilor, TemplateManager.global.alienShockTroopOrgDataName);
        //            int num = councilors.Count((TICouncilorState x) => x.OnEarth);
        //            if (!councilor.OnEarth) {
        //                int b = 18;
        //                int importance = Mathf.Min(20 - num, b);
        //                FactionGoal_TransportCouncilorsWithFleet prospectiveGoal = new FactionGoal_TransportCouncilorsWithFleet(this, importance, new List<TICouncilorState> { councilor }, AIEvaluators.SelectAlienCrashdownRegion(advance: true));
        //                AddGoal(prospectiveGoal, HandleDuplicateGoalRule.Ignore);
        //            }
        //        }
        //    }
        //    else {
        //        SetIntel(councilor, TemplateManager.global.intelToSeeCouncilorMission);
        //    }
        //    councilor.SelectVoice();
        //    if (!forced) {
        //        Log.Debug($"Paying cost to hire councilor {councilor} for faction {this.displayName}");
        //        councilor.HireRecruitCost(this).PayCost(this, "Hire Councilor");
        //        switch (councilors.Count) {
        //            case 3:
        //                CompleteMilestone(CampaignMilestone.TutorialRecruitCouncilor3);
        //                break;
        //            case 4:
        //                CompleteMilestone(CampaignMilestone.TutorialRecruitCouncilor4);
        //                break;
        //            case 5:
        //                CompleteMilestone(CampaignMilestone.TutorialBuildCouncil);
        //                break;
        //        }
        //    }
        //    TINotificationQueueState.AddCouncilorMessage(councilor, CouncilorChatType.NewCouncilor, councilor.faction);
        //    SetResourceIncomeDataDirty(councilorResources);
        //    councilor.RecordLocation();
        //    if (councilor.XP == 0 && !forced && tIDateTime == null) {
        //        if (IsActiveHumanFaction) {
        //            councilor.ChangeXP(Mathf.Max(0, TemplateManager.global.initialXPPerYearAge * (councilor.age - TemplateManager.global.minAgeForXPBonus)));
        //        }
        //        councilor.ChangeXP((int)TIEffectsState.SumEffectsModifiers(Context.NewCouncilorRecruitXP, this, councilor.XP));
        //    }
        //    GameControl.eventManager.TriggerEvent(new CouncilCompositionChanged(this, councilor, councilor.location, joining: true), null);
        //    GameControl.eventManager.TriggerEvent(new CouncilorPositionUpdated(councilor, councilor.location), null, (from x in new object[6]
        //        {
        //        this,
        //        councilor,
        //        councilor.location,
        //        councilor.location.ref_nation,
        //        councilor.location.ref_fleet,
        //        councilor.location.ref_spaceBody
        //        }.Distinct()
        //                                                                                                              where x != null
        //                                                                                                              select x).ToArray());
        //    if (isActivePlayer && councilors.Count >= 6) {
        //        UnlockAchievement("recruitFullCouncil");
        //        if (turnedCouncilors.Count == 2) {
        //            UnlockAchievement("controlFullCouncilTurned");
        //        }
        //    }
        //}

        //[MonoModReplace]
        //public bool GenerateRecruitableCouncilors(bool campaignStart = false) {
        //    bool result = false;
        //    if (availableCouncilors.Count > 1 && !campaignStart && IsActiveHumanFaction) {
        //        for (int num = availableCouncilors.Count - 1; num >= 0; num--) {
        //            if (TIUtilities.RandomFloatValue() * 100f < (float)availableCouncilors[num].age) {
        //                TICouncilorState tICouncilorState = availableCouncilors[num];
        //                availableCouncilors.Remove(tICouncilorState);
        //                if (tICouncilorState.template.randomized) {
        //                    TIGlobalValuesState.GlobalValues.councilorAppearanceTemplatesInUse.Remove(tICouncilorState.appearanceTemplateName);
        //                    tICouncilorState.ArchiveState();
        //                    GameStateManager.RemoveGameState<TICouncilorState>(tICouncilorState.ID);
        //                }
        //            }
        //        }
        //    }
        //    if (TemplateManager.global.maxFactionCouncilorCandidatePool > 0) {
        //        int num2 = (IsActiveHumanFaction ? TIUtilities.RandomRange(-TemplateManager.global.maxFactionCouncilorCandidatePoolVariance, TemplateManager.global.maxFactionCouncilorCandidatePoolVariance) : 0);
        //        for (int i = availableCouncilors.Count; i <= TemplateManager.global.maxFactionCouncilorCandidatePool + num2; i++) {
        //            List<TICouncilorState> list = new List<TICouncilorState>();
        //            foreach (TICouncilorState item in GameStateManager.IterateByClass<TICouncilorState>()) {
        //                if (!item.everBeenAvailable && !item.template.debugOnly && string.IsNullOrEmpty(item.template.debugStartingCouncil) && !item.template.randomized && item.age >= 18 && item.age <= 85 && item.template.allowedIdeologies.Contains(ideology.ideology)) {
        //                    list.Add(item);
        //                }
        //            }
        //            if (TIUtilities.RandomFloatValue() > TemplateManager.global.chanceCouncilorTemplate || list.Count == 0) {
        //                TICouncilorState tICouncilorState2 = GameStateManager.CreateNewGameState<TICouncilorState>();
        //                if (IsAlienFaction) {
        //                    tICouncilorState2.InitWithTemplate(TemplateManager.Find<TICouncilorTemplate>("randomizedAlienCouncilor3"));
        //                }
        //                else {
        //                    tICouncilorState2.InitWithTemplate(TemplateManager.Find<TICouncilorTemplate>("randomizedCouncilor2"));
        //                }
        //                Log.Debug($"tICouncilorState2 {tICouncilorState2.templateName} for faction {this.displayName}");
        //                Log.Debug($"randomizedCouncilor2 found: {TemplateManager.Find<TICouncilorTemplate>("randomizedCouncilor2") != null}");
        //                if (availableCouncilors.None((TICouncilorState x) => x.HireRecruitCost(this).CanAfford(this)) && availableCouncilors.Count((TICouncilorState x) => x.typeTemplate.affinities.Contains(ideology.ideology)) < 2) {
        //                    IEnumerable<TICouncilorTypeTemplate> enumerable = from x in TemplateManager.IterateByClass<TICouncilorTypeTemplate>()
        //                                                                      where x.affinities.Contains(ideology.ideology)
        //                                                                      select x;
        //                    if (enumerable.Count() > 0) {
        //                        tICouncilorState2.NewCharacterGeneration(enumerable.SelectRandomItem(), null, IsAlienFaction ? null : this);
        //                    }
        //                }
        //                else {
        //                    tICouncilorState2.NewCharacterGeneration(null, null, IsAlienFaction ? null : this);
        //                }
        //                availableCouncilors.Insert(0, tICouncilorState2);
        //                result = true;
        //            }
        //            else {
        //                int index = TIUtilities.RandomRange(0, list.Count);
        //                availableCouncilors.Insert(0, list[index]);
        //                result = true;
        //                list[index].everBeenAvailable = true;
        //            }
        //        }
        //    }
        //    return result;
        //}
    }
}
