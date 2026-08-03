using MonoMod;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Systems.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;

//Debug Patch
internal class patch_StartMenuController : StartMenuController {

    [MonoModIgnore] private List<TINationTemplate> nationsInScenario;

    [MonoModIgnore] private List<String> defaultCompletedProjectsInScenario;

    [MonoModIgnore] private Dictionary<string, string> currentStartOptions = new Dictionary<string, string>();

    [MonoModIgnore] private TIMetaTemplate selectedMetaTemplateScenario;

    [MonoModIgnore] private List<TIFactionTemplate> currentAllowedFactions;

    [MonoModIgnore] private List<TIFactionTemplate> factionsInScenario;

    [MonoModIgnore] private List<string> defaultFactionsInScenario;

    [MonoModIgnore] private IScenario selectedScenario;

    [MonoModIgnore] private int defaultMiningProductivity;

    [MonoModIgnore] private string selectedFactionDataName;

    [MonoModIgnore] private TIFactionTemplate selectedFaction => TemplateManager.Find<TIFactionTemplate>(selectedFactionDataName);


    // Ref wrappers for editing private fields
    public ref List<TINationTemplate> ref_nationsInScenario() {
        return ref nationsInScenario;
    }

    public ref List<string> ref_defaultCompletedProjectsInScenario() {
        return ref defaultCompletedProjectsInScenario;
    }

    public ref Dictionary<string, string> ref_currentStartOptions() {
        return ref currentStartOptions;
    }

    public ref TIMetaTemplate ref_selectedMetaTemplateScenario() {
        return ref selectedMetaTemplateScenario;
    }

    public ref List<TIFactionTemplate> ref_currentAllowedFactions() {
        return ref currentAllowedFactions;
    }

    public ref List<TIFactionTemplate> ref_factionsInScenario() {
        return ref factionsInScenario;
    }

    public ref List<string> ref_defaultFactionsInScenario() {
        return ref defaultFactionsInScenario;
    }

    public ref IScenario ref_selectedScenario() {
        return ref selectedScenario;
    }

    public ref int ref_defaultMiningProductivity() {
        return ref defaultMiningProductivity;
    }

    public void UpdateStartOptions(string category, TIMetaTemplate template) {
        Log.Debug($"[UpdateStartOptions] Updating start options for category {category} with template {template.dataName}");
        currentStartOptions[category] = template.dataName;
        StringBuilder stringBuilder = new StringBuilder();
        foreach (string value in currentStartOptions.Values) {
            stringBuilder.AppendLine(Loc.T(new StringBuilder("TIMetaTemplate.description.").Append(value).ToString()));
        }
        newGameSummaryText.SetText(stringBuilder.ToString());
        if (template.newCampaignOptionCategory == "Scenario") {
            Log.Debug($"[UpdateStartOptions] Updating scenario options for template {template.dataName}");
            selectedMetaTemplateScenario = template;
            nationsInScenario = TIMetaTemplate.GetTemplatesOfTypeFromMeta(template.dataName, typeof(TINationTemplate)).ConvertAll((TIDataTemplate x) => (TINationTemplate)x).ToList();
            defaultCompletedProjectsInScenario = (TIMetaTemplate.GetTemplatesOfTypeFromMeta(template.dataName, typeof(TIStartTimeTemplate)).First() as TIStartTimeTemplate).projectsCompleted;
            UpdateAllowedFactions();
            UpdateTutorialOptions();
            SetStarterNationOptions();
            UpdateMapOptions(template.dataName);
        }
        if (template.newCampaignOptionCategory == "FactionCouncils") {
            Log.Debug($"[UpdateStartOptions] Updating faction council options for template {template.dataName}");
            currentAllowedFactions = (from x in TIMetaTemplate.GetTemplatesOfTypeFromMeta(template.dataName, typeof(TIFactionTemplate)).ConvertAll((TIDataTemplate x) => (TIFactionTemplate)x)
                                      where x.activePlayerAllowed
                                      select x).ToList();
            factionsInScenario = TIMetaTemplate.GetTemplatesOfTypeFromMeta(template.dataName, typeof(TIFactionTemplate)).ConvertAll((TIDataTemplate x) => (TIFactionTemplate)x).ToList();
            defaultFactionsInScenario = new List<string>(factionsInScenario.Select((TIFactionTemplate x) => x.dataName).ToList());
            ResetFactionOptions();
            UpdateAllowedFactions();
            UpdateFactionOptions();
        }
        if (template.newCampaignOptionCategory == "SolarSystem") {
            Log.Debug($"[UpdateStartOptions] Updating solar system options for template {template.dataName}");
            int count = TIMetaTemplate.GetTemplatesOfTypeFromMeta(template.dataName, typeof(TISpaceBodyTemplate)).Count;
            if (count <= TemplateManager.global.defaultSpaceBodyCapForMiningProductivityBonus) {
                defaultMiningProductivity = (int)((float)TemplateManager.global.defaultMiningProductivity * ((float)TemplateManager.global.defaultSpaceBodyCapForMiningProductivityBonus / (float)count));
            }
            else {
                defaultMiningProductivity = TemplateManager.global.defaultMiningProductivity;
            }
            miningProductivityMultiplierSlider.SetValueWithoutNotify(defaultMiningProductivity);
            UpdateMiningProductivySpeedSlider(updateDifficulty: false);
        }
    }

    private void UpdateAllowedFactions() {
        int num = 0;
        foreach (object item in factionToggleListManager) {
            ((FactionToggleListItemController)(dynamic)item).UpdateForDefaultFactions(factionsInScenario);
            num++;
        }
    }

    private void UpdateMapOptions(string templateName) {
        mapSeedInputGO.SetActive(value: false);
        mapRandomizeToggleGO.SetActive(value: false);
    }

    private void ResetFactionOptions() {
        factionToggleListManager.SetListSize<FactionToggleListItemController>(factionsInScenario.Count);
        int num = 0;
        foreach (object item in factionToggleListManager) {
            ((FactionToggleListItemController)(dynamic)item).Init(factionsInScenario[num], selectedScenario.activePlayerFaction, this);
            num++;
        }
        UpdateAllowedFactions();
        UpdateBaseCPForDefaultFactions();
    }

    private void UpdateBaseCPForDefaultFactions() {
        controlPointFreebieBonusSlider.SetValueWithoutNotify(baseFreebiesCount() / TemplateManager.global.pointsPerCPSliderTick);
        UpdateControlPointFreebieSlider(updateDifficulty: false);
    }

    private int baseFreebiesCount() {
        int num = TemplateManager.global.controlPointMaintenanceFreebies;
        if (factionsInScenario.Where((TIFactionTemplate o) => !o.isAlien).Count() < 7) {
            num += (7 - factionsInScenario.Where((TIFactionTemplate o) => !o.isAlien).Count()) * TemplateManager.global.controlPointBonusMaintenanceFreebiesPerRemovedFaction;
        }
        return num;
    }

    private void SetStarterNationOptions() {
        Log.Debug("[SetStarterNationOptions] Setting up starter nation options");
        customStartingNationGroupDropdown.ClearOptions();
        customStartingNationGroupDropdown.options.Add(new TMP_Dropdown.OptionData
        {
            text = Loc.T(new StringBuilder("UI.StartScreen.CustomizeCampaign.NationGroupNone").ToString())
        });
        int num = nationsInScenario.Max((TINationTemplate x) => x.group);
        int i;
        for (i = 1; i < num + 1; i++) {
            Log.Debug($"[SetStarterNationOptions] Processing nation group {i}");
            IEnumerable<TINationTemplate> enumerable = nationsInScenario.Where((TINationTemplate o) => o.group == i);
            TINationTemplate tINationTemplate = enumerable.MaxBy((TINationTemplate x) => x.StartingClaims(nationsInScenario, defaultCompletedProjectsInScenario, countLockedClaims: true));
            TINationTemplate tINationTemplate2 = enumerable.MaxBy((TINationTemplate x) => x.initialGDP.GetValueOrDefault());
            string text = (tINationTemplate.IsStartingUnion(nationsInScenario, defaultCompletedProjectsInScenario) ? tINationTemplate.startUpUnionDisplayName() : tINationTemplate.startUpDisplayName());
            string text2 = (tINationTemplate2.IsStartingUnion(nationsInScenario, defaultCompletedProjectsInScenario) ? tINationTemplate2.startUpUnionDisplayName() : tINationTemplate2.startUpDisplayName());
            StringBuilder stringBuilder = new StringBuilder();
            int num2 = 1;
            if (tINationTemplate != tINationTemplate2) {
                stringBuilder.Append(Loc.T("UI.Global.2IC", text, text2));
                num2 = 2;
            }
            else {
                stringBuilder.Append(text);
            }
            if (enumerable.Count() > num2) {
                stringBuilder.Append("+");
            }
            customStartingNationGroupDropdown.options.Add(new TMP_Dropdown.OptionData
            {
                text = stringBuilder.ToString()
            });
        }
        customStartingNationGroupDropdown.value = 0;
        customStartingNationGroupDropdown.RefreshShownValue();
    }

}
