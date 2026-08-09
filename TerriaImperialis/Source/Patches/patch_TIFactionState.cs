using MonoMod;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PavonisInteractive.TerraInvicta {
    internal class patch_TIFactionState : TIFactionState {

        [MonoModReplace]
        public void AddAvailableCouncilor(TICouncilorState councilor, bool forced = false) {
            councilor.SetFaction(this);
            TIDateTime tIDateTime = null;
            if (councilor.recruitDate != null) {
                tIDateTime = new TIDateTime(councilor.recruitDate);
            }
            councilor.SetRecruitDate();
            councilors.Add(councilor);
            availableCouncilors.Remove(councilor);
            if (IsAlienFaction) {
                SetIntel(councilor, TemplateManager.global.intelToSeeCouncilorSecrets);
                if (!councilor.OnEarth || TIEffectsState.CheckForAnyEffectInContext(Context.ManyAliensOnEarth, this)) {
                    GrantNewOrgToCouncilor(councilor, TemplateManager.global.alienShockTroopOrgDataName);
                    int num = councilors.Count((TICouncilorState x) => x.OnEarth);
                    if (!councilor.OnEarth) {
                        int b = 18;
                        int importance = Mathf.Min(20 - num, b);
                        FactionGoal_TransportCouncilorsWithFleet prospectiveGoal = new FactionGoal_TransportCouncilorsWithFleet(this, importance, new List<TICouncilorState> { councilor }, AIEvaluators.SelectAlienCrashdownRegion(advance: true));
                        AddGoal(prospectiveGoal, HandleDuplicateGoalRule.Ignore);
                    }
                }
            }
            else {
                SetIntel(councilor, TemplateManager.global.intelToSeeCouncilorMission);
            }
            councilor.SelectVoice();
            if (!forced) {
                councilor.HireRecruitCost(this).PayCost(this, "Hire Councilor");
                switch (councilors.Count) {
                    case 3:
                        CompleteMilestone(CampaignMilestone.TutorialRecruitCouncilor3);
                        break;
                    case 4:
                        CompleteMilestone(CampaignMilestone.TutorialRecruitCouncilor4);
                        break;
                    case 5:
                        CompleteMilestone(CampaignMilestone.TutorialBuildCouncil);
                        break;
                }
            }
            TINotificationQueueState.AddCouncilorMessage(councilor, CouncilorChatType.NewCouncilor, councilor.faction);
            SetResourceIncomeDataDirty(councilorResources);
            councilor.RecordLocation();
            if (councilor.XP == 0 && !forced && tIDateTime == null) {
                if (IsActiveHumanFaction) {
                    councilor.ChangeXP(Mathf.Max(0, TemplateManager.global.initialXPPerYearAge * (councilor.age - TemplateManager.global.minAgeForXPBonus)));
                }
                councilor.ChangeXP((int)TIEffectsState.SumEffectsModifiers(Context.NewCouncilorRecruitXP, this, councilor.XP));
            }
            GameControl.eventManager.TriggerEvent(new CouncilCompositionChanged(this, councilor, councilor.location, joining: true), null);
            GameControl.eventManager.TriggerEvent(new CouncilorPositionUpdated(councilor, councilor.location), null, (from x in new object[6]
                {
                this,
                councilor,
                councilor.location,
                councilor.location.ref_nation,
                councilor.location.ref_fleet,
                councilor.location.ref_spaceBody
                }.Distinct()
                                                                                                                      where x != null
                                                                                                                      select x).ToArray());
            if (isActivePlayer && councilors.Count >= 6) {
                UnlockAchievement("recruitFullCouncil");
                if (turnedCouncilors.Count == 2) {
                    UnlockAchievement("controlFullCouncilTurned");
                }
            }
        }
    }
}
