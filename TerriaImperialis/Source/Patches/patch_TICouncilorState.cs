using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PavonisInteractive.TerraInvicta
{

    public class patch_TICouncilorState : TICouncilorState
    {


        [SerializeField]
        private bool gameStateSubjectCreated;

        [MonoModIgnore]
        private TICouncilorTypeTemplate _typeTemplate;

        public List<string> traitTemplateNames { get; private set; } //Maybe fix

        public void NewCharacterGeneration(TICouncilorTypeTemplate forcedJob = null, TIRegionState forcedRegion = null, TIFactionState forFaction = null, bool forceMaxStats = false, bool startup = false)
        {
            if (!template.alien) {
                Log.Debug($"NewCharacterGeneration: Generating new character");
                RandomizeBirthday();
                if (!template.randomized) {
                    dateBorn.SetTime(template.yearBorn ?? dateBorn.year, template.monthBorn ?? dateBorn.month, template.dayBorn ?? dateBorn.day);
                    int num = TITimeState.Now().year - 2022;
                    if (template.yearBorn.HasValue)
                    {
                        switch (num)
                        {
                            default:
                                dateBorn.SetTime(dateBorn.year + num, dateBorn.month, dateBorn.day);
                                break;
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                                break;
                        }
                    }
                    Log.Debug($"NewCharacterGeneration: dateBorn: {dateBorn.year}-{dateBorn.month}-{dateBorn.day}");
                }
                if (forcedRegion == null)
                {
                    homeRegion = RandomizeRegionWeightedByPopulation(considerSocialDemographics: true, forFaction);
                    if (!template.randomized)
                    {
                        TIMapRegionTemplate tIMapRegionTemplate = TemplateManager.Find<TIMapRegionTemplate>(template.mapRegionBorn ?? "");
                        TIRegionTemplate tIRegionTemplate = TemplateManager.Find<TIRegionTemplate>(template.regionBorn ?? "");
                        if (tIMapRegionTemplate != null)
                        {
                            homeRegion = GameStateManager.MapRegionLookup(template.mapRegionBorn);
                        }
                        else if (tIRegionTemplate != null)
                        {
                            homeRegion = GameStateManager.FindByTemplate<TIRegionState>(template.regionBorn);
                        }
                        else if (template.regionBorn.Length > 0)
                        {
                            Log.Warn("Councilor " + template.dataName + " has nonexisting home region " + template.regionBorn);
                        }
                    }
                    Log.Debug($"NewCharacterGeneration: homeRegion: {homeRegion?}");
                }
                else
                {
                    homeRegion = forcedRegion;
                }
                ancestry = RandomizeAncestryFromRegion(homeRegion);
                if (!template.randomized)
                {
                    CouncilorAncestry councilorAncestry = (template.strAncestry ?? "").ToEnum(CouncilorAncestry.None);
                    if (councilorAncestry != CouncilorAncestry.None)
                    {
                        ancestry = councilorAncestry;
                    }
                }
                gender = RandomizeGender(homeRegion);
                if (!template.randomized)
                {
                    CouncilorGender councilorGender = (template.strGender ?? "").ToEnum(CouncilorGender.None);
                    if (councilorGender != CouncilorGender.None)
                    {
                        gender = councilorGender;
                    }
                }
                if (!template.randomized)
                {
                    personalName = template.personalName ?? "Missing Personal Name";
                    familyName = template.familyName ?? "Missing Family Name";
                    SetDisplayName();
                }
                else
                {
                    Tuple<string, string> tuple = GenerateNameFromRegionAncestry(homeRegion, ancestry, gender);
                    personalName = tuple.Item1;
                    familyName = tuple.Item2;
                    SetDisplayName();
                }
                if (forcedJob == null)
                {
                    RandomizeJob(forFaction, considerAvailable: true);
                }
                else
                {
                    _typeTemplate = forcedJob;
                    typeTemplateName = forcedJob.dataName;
                }
                if (!template.randomized)
                {
                    if (template.type != "")
                    {
                        typeTemplateName = template.type ?? typeTemplateName;
                    }
                    _typeTemplate = TemplateManager.Find<TICouncilorTypeTemplate>(typeTemplateName);
                    if (_typeTemplate == null)
                    {
                        RandomizeJob(forFaction, considerAvailable: false);
                        Error.Log("Bad job name" + template.type + " passed to character creator in TICouncilorState");
                    }
                }
                RandomizeStats(forFaction, forceMaxStats);
                if (!template.randomized)
                {
                    AssignStatsFromTemplate();
                }
                if (template.randomizeTraits)
                {
                    RandomizeTraits(forFaction);
                    if (!template.randomized && !template.allowRandomOnlyTraits)
                    {
                        traits.RemoveAll((TITraitTemplate x) => x.randomCouncilorsOnly);
                        traitTemplateNames = traits.Select((TITraitTemplate x) => x.dataName).ToList();
                    }
                    if (startup)
                    {
                        traits.RemoveAll((TITraitTemplate x) => x.restrictedLocations != RestrictedLocations.None);
                        traitTemplateNames = traits.Select((TITraitTemplate x) => x.dataName).ToList();
                    }
                }
                else
                {
                    traits.Clear();
                    traitTemplateNames.Clear();
                    if (template.traits != null)
                    {
                        string[] array = template.traits;
                        for (int num2 = 0; num2 < array.Length; num2++)
                        {
                            TITraitTemplate tITraitTemplate = TemplateManager.Find<TITraitTemplate>(array[num2]);
                            if (tITraitTemplate == null)
                            {
                                continue;
                            }
                            bool flag = false;
                            foreach (TITraitTemplate trait in traits)
                            {
                                if (trait.grouping == tITraitTemplate.grouping)
                                {
                                    flag = true;
                                }
                            }
                            if (!flag && !traits.Contains(tITraitTemplate))
                            {
                                AddTrait(tITraitTemplate);
                            }
                        }
                    }
                }
                if (forFaction != null)
                {
                    foreach (TIEffectTemplate item in TIEffectsState.GetFactionEffectsForContext(Context.AllRecruitTraits, forFaction))
                    {
                        if (item.value == 1f)
                        {
                            TITraitTemplate tITraitTemplate2 = TemplateManager.Find<TITraitTemplate>(item.strValue);
                            if (tITraitTemplate2 != null && tITraitTemplate2.CouncilorCanHave(this, forFaction, grantedByEffect: true))
                            {
                                AddTrait(tITraitTemplate2);
                            }
                        }
                    }
                }
                SetLocation(homeRegion);
                foreach (TITraitTemplate trait2 in traits)
                {
                    switch (trait2.restrictedLocations)
                    {
                        case RestrictedLocations.HomeNation:
                        case RestrictedLocations.HomeNationAndAllies:
                            {
                                IEnumerable<TINationState> enumerable2 = from x in GameStateManager.AllExtantHumanNations()
                                                                         where !x.IsAlliedWith(homeNation, includeSelf: true)
                                                                         select x;
                                if (enumerable2.Count() > 0)
                                {
                                    SetLocation(enumerable2.SelectRandomWeightedItem((TINationState x) => x.numControlPoints_unclamped).capital);
                                }
                                break;
                            }
                        case RestrictedLocations.HighUnrestNations:
                            if (homeNation.unrest >= TemplateManager.global.HighUnrestDefinition)
                            {
                                IEnumerable<TINationState> enumerable = from x in GameStateManager.AllExtantHumanNations()
                                                                        where x.unrest < TemplateManager.global.HighUnrestDefinition
                                                                        select x;
                                if (enumerable.Count() > 0)
                                {
                                    SetLocation(enumerable.SelectRandomItem().capital);
                                }
                            }
                            break;
                    }
                    if (location == null)
                    {
                        SetLocation(homeRegion);
                    }
                }
            }
            else {
                Log.Debug($"NewCharacterGeneration: Generating new character for faction (alien): {forFaction?.templateName}, forcedJob: {forcedJob?.dataName}, forcedRegion: {forcedRegion?.templateName}, forceMaxStats: {forceMaxStats}, startup: {startup}");
                TIFactionState tIFactionState = GameStateManager.AlienFaction();
                tIFactionState.councilorsGenerated++;
                RandomizeBirthday();
                gender = CouncilorGender.None;

                //ancestry = CouncilorAncestry.Alien;
                //typeTemplateName = "Alien";
                //RandomizeStats(tIFactionState, forceMaxStats);
                //personalName = Loc.T("TICouncilorTemplate.alienName1");
                //familyName = Loc.T("TICouncilorTemplate.alienName2", tIFactionState.councilorsGenerated.ToString());

                ancestry = CouncilorAncestry.Alien;
                typeTemplateName = "Xenos";
                RandomizeStats(tIFactionState, forceMaxStats);
                personalName = Loc.T("TICouncilorTemplate.alienName1");
                familyName = Loc.T("TICouncilorTemplate.alienName2", tIFactionState.councilorsGenerated.ToString());


                SetDisplayName();
                if (TIEffectsState.CheckForAnyEffectInContext(Context.ManyAliensOnEarth, tIFactionState))
                {
                    if (GameStateManager.AlienNation().extant)
                    {
                        SetLocation(GameStateManager.AlienNation().capital);
                    }
                    else
                    {
                        IEnumerable<TIRegionState> enumerable3 = from x in GameStateManager.AllRegions()
                                                                 where x.alienFacility.Extant()
                                                                 select x;
                        if (enumerable3.Count() > 0)
                        {
                            SetLocation(enumerable3.SelectRandomItem().ref_region);
                        }
                        else
                        {
                            IEnumerable<TIRegionState> enumerable4 = from x in GameStateManager.AllRegions()
                                                                     where x.alienLanding.Extant()
                                                                     select x;
                            if (enumerable4.Count() > 0)
                            {
                                SetLocation(enumerable4.SelectRandomItem().ref_region);
                            }
                            else
                            {
                                SetLocation(tIFactionState.primaryHab);
                            }
                        }
                    }
                }
                else
                {
                    SetLocation(tIFactionState.primaryHab);
                }
                if (template.traits != null)
                {
                    string[] array = template.traits;
                    for (int num2 = 0; num2 < array.Length; num2++)
                    {
                        TITraitTemplate tITraitTemplate3 = TemplateManager.Find<TITraitTemplate>(array[num2]);
                        if (tITraitTemplate3 == null)
                        {
                            continue;
                        }
                        bool flag2 = false;
                        foreach (TITraitTemplate trait3 in traits)
                        {
                            if (trait3.grouping == tITraitTemplate3.grouping)
                            {
                                flag2 = true;
                            }
                        }
                        if (!flag2 && !traits.Contains(tITraitTemplate3))
                        {
                            AddTrait(tITraitTemplate3);
                        }
                    }
                }
            }
            if (!template.randomized)
            {
                TIFactionState[] array2 = GameStateManager.AllFactions();
                foreach (TIFactionState tIFactionState2 in array2)
                {
                    if (tIFactionState2.templateName == template.debugStartingCouncil && tIFactionState2.councilors.Count < 6)
                    {
                        tIFactionState2.AddAvailableCouncilor(this, forced: true);
                        tIFactionState2.SetIntel(this, TemplateManager.global.intelToSeeCouncilorMission);
                        break;
                    }
                }
                foreach (TINationState item2 in GameStateManager.AllExtantNations())
                {
                    if (item2.templateName == template.debugStartingNation)
                    {
                        SetLocation(item2.capital);
                        break;
                    }
                }
            }
            appearanceTemplateName = SelectAppearance();
            TIGlobalValuesState.GlobalValues.councilorAppearanceTemplatesInUse.Add(appearanceTemplateName);
            gameStateSubjectCreated = true;
        }


        private extern void orig_RandomizeBirthday();

        private void RandomizeBirthday()
        {
            orig_RandomizeBirthday();
        }

        private void RandomizeJob(TIFactionState faction, bool considerAvailable) {
            Log.Debug($"RandomizeJob: faction: {faction?.templateName}, considerAvailable: {considerAvailable}");
            Dictionary<TICouncilorTypeTemplate, float> weightedList = TemplateManager.IterateByClass<TICouncilorTypeTemplate>(allowChild: false).ToDictionary((TICouncilorTypeTemplate jobTemplate) => jobTemplate, (TICouncilorTypeTemplate jobTemplate) => (!jobTemplate.unlocked) ? 0f : (jobTemplate.weight / (considerAvailable ? Mathf.Pow(Mathf.Max(1, (faction?.councilors.Count((TICouncilorState x) => x.typeTemplate == jobTemplate) + faction?.availableCouncilors.Count((TICouncilorState x) => x.typeTemplate == jobTemplate) + 1) ?? 1), 2f) : 1f)));
            _typeTemplate = weightedList.SelectRandomWeightedItem((KeyValuePair<TICouncilorTypeTemplate, float> k) => k.Value).Key;
            Log.Debug($"RandomizeJob: selected job template: {_typeTemplate?.dataName}");
            typeTemplateName = _typeTemplate.dataName;
        }

        private void RandomizeStats(TIFactionState forFaction, bool forceBestStats)
        {
            Log.Debug($"RandomizeStats: forFaction: {forFaction?.templateName}, forceBestStats: {forceBestStats}");
            TICouncilorTypeTemplate typeTemplate = this._typeTemplate;
            CouncilorAttribute[] array;
            if (typeTemplate == null)
            {
                Log.Debug("RandomizeStats: typeTemplate is null");
                array = null;
            }
            else
            {
                Log.Debug("RandomizeStats: typeTemplate is not null");
                CouncilorAttribute[] keyStat = typeTemplate.keyStat;
                if (keyStat == null)
                {
                    array = null;
                }
                else
                {
                    array = keyStat.Where<CouncilorAttribute>((CouncilorAttribute x) => x > CouncilorAttribute.None).Distinct<CouncilorAttribute>().ToArray<CouncilorAttribute>();
                }
            }
            CouncilorAttribute[] array2 = array;
            if (forceBestStats && !this.isAlien)
            {
                Log.Debug("RandomizeStats: forceBestStats is true and isAlien is false");
                if (array2.Length > 1)
                {
                    this.attributes[array2[0]] = 7;
                    this.attributes[array2[1]] = 6;
                }
                else if (array2.Length == 1)
                {
                    this.attributes[array2[0]] = 8;
                }
            }
            foreach (CouncilorAttribute councilorAttribute in Enums.CouncilorAttributes) {
                Log.Debug($"RandomizeStats: councilorAttribute: {councilorAttribute}, forceBestStats: {forceBestStats}, array2: {array2?.Length}");
                if (!forceBestStats || array2 == null || !array2.Contains(councilorAttribute)) {
                    Log.Debug($"RandomizeStats: councilorAttribute: {councilorAttribute} is not in key stats or forceBestStats is false");
                    switch (councilorAttribute) {
                        case CouncilorAttribute.Persuasion:
                            Log.Debug($"RandomizeStats: basePersuasion: {this.typeTemplate.basePersuasion}");
                            Log.Debug($"RandomizeStats: randPersuasion: {this.typeTemplate.randPersuasion}");
                            this.attributes[CouncilorAttribute.Persuasion] = this.typeTemplate.basePersuasion + TIUtilities.RandomRange(0, this.typeTemplate.randPersuasion + 1) + (int)TIEffectsState.SumEffectsModifiers(Context.AllRecruitStats, forFaction, (float)this.typeTemplate.basePersuasion, councilorAttribute.ToString());
                            break;
                        case CouncilorAttribute.Investigation:
                            this.attributes[CouncilorAttribute.Investigation] = this.typeTemplate.baseInvestigation + TIUtilities.RandomRange(0, this.typeTemplate.randInvestigation + 1) + (int)TIEffectsState.SumEffectsModifiers(Context.AllRecruitStats, forFaction, (float)this.typeTemplate.baseInvestigation, councilorAttribute.ToString());
                            break;
                        case CouncilorAttribute.Espionage:
                            this.attributes[CouncilorAttribute.Espionage] = this.typeTemplate.baseEspionage + TIUtilities.RandomRange(0, this.typeTemplate.randEspionage + 1) + (int)TIEffectsState.SumEffectsModifiers(Context.AllRecruitStats, forFaction, (float)this.typeTemplate.baseEspionage, councilorAttribute.ToString());
                            break;
                        case CouncilorAttribute.Command:
                            this.attributes[CouncilorAttribute.Command] = this.typeTemplate.baseCommand + TIUtilities.RandomRange(0, this.typeTemplate.randCommand + 1) + (int)TIEffectsState.SumEffectsModifiers(Context.AllRecruitStats, forFaction, (float)this.typeTemplate.baseCommand, councilorAttribute.ToString());
                            break;
                        case CouncilorAttribute.Administration:
                            this.attributes[CouncilorAttribute.Administration] = this.typeTemplate.baseAdministration + TIUtilities.RandomRange(0, this.typeTemplate.randAdministration + 1) + (int)TIEffectsState.SumEffectsModifiers(Context.AllRecruitStats, forFaction, (float)this.typeTemplate.baseAdministration, councilorAttribute.ToString());
                            break;
                        case CouncilorAttribute.Science:
                            this.attributes[CouncilorAttribute.Science] = this.typeTemplate.baseScience + TIUtilities.RandomRange(0, this.typeTemplate.randScience + 1) + (int)TIEffectsState.SumEffectsModifiers(Context.AllRecruitStats, forFaction, (float)this.typeTemplate.baseScience, councilorAttribute.ToString());
                            break;
                        case CouncilorAttribute.Security:
                            this.attributes[CouncilorAttribute.Security] = this.typeTemplate.baseSecurity + TIUtilities.RandomRange(0, this.typeTemplate.randSecurity + 1) + (int)TIEffectsState.SumEffectsModifiers(Context.AllRecruitStats, forFaction, (float)this.typeTemplate.baseSecurity, councilorAttribute.ToString());
                            break;
                        case CouncilorAttribute.Loyalty:
                            this.attributes[CouncilorAttribute.Loyalty] = this.typeTemplate.baseLoyalty + TIUtilities.RandomRange(0, this.typeTemplate.randLoyalty + 1) + (int)TIEffectsState.SumEffectsModifiers(Context.AllRecruitStats, forFaction, (float)this.typeTemplate.baseLoyalty, councilorAttribute.ToString());
                            break;
                    }
                }
            }
            Log.Debug("RandomizeStats: Finished randomizing stats");
            this.attributes[CouncilorAttribute.ApparentLoyalty] = this.attributes[CouncilorAttribute.Loyalty] - 2 + TIUtilities.RandomRange(0, 4);
            foreach (KeyValuePair<CouncilorAttribute, int> keyValuePair in this.attributes.ToList<KeyValuePair<CouncilorAttribute, int>>())
            {
                Log.Debug($"RandomizeStats: councilorAttribute: {keyValuePair.Key}, value: {keyValuePair.Value}");
                if (keyValuePair.Value < 0)
                {
                    this.attributes[keyValuePair.Key] = 0;
                }
            }
        }

        public TICouncilorTypeTemplate typeTemplate
        {
            get
            {
                if (this._typeTemplate == null)
                {
                    Debug.Log("typeTemplate is null, finding typeTemplate for: " + this.typeTemplateName);
                    this._typeTemplate = TemplateManager.Find<TICouncilorTypeTemplate>(this.typeTemplateName, true);
                }
                return this._typeTemplate;
            }
        }

        private void AssignStatsFromTemplate()
        {
            attributes[CouncilorAttribute.Persuasion] = Mathf.Clamp(template.persuasion ?? attributes[CouncilorAttribute.Persuasion], 0, TemplateManager.global.maxCouncilorAttribute);
            attributes[CouncilorAttribute.Espionage] = Mathf.Clamp(template.espionage ?? attributes[CouncilorAttribute.Espionage], 0, TemplateManager.global.maxCouncilorAttribute);
            attributes[CouncilorAttribute.Command] = Mathf.Clamp(template.command ?? attributes[CouncilorAttribute.Command], 0, TemplateManager.global.maxCouncilorAttribute);
            attributes[CouncilorAttribute.Investigation] = Mathf.Clamp(template.investigation ?? attributes[CouncilorAttribute.Investigation], 0, TemplateManager.global.maxCouncilorAttribute);
            attributes[CouncilorAttribute.Science] = Mathf.Clamp(template.science ?? attributes[CouncilorAttribute.Science], 0, TemplateManager.global.maxCouncilorAttribute);
            attributes[CouncilorAttribute.Administration] = Mathf.Clamp(template.administration ?? attributes[CouncilorAttribute.Administration], 0, TemplateManager.global.maxCouncilorAttribute);
            attributes[CouncilorAttribute.Security] = Mathf.Clamp(template.security ?? attributes[CouncilorAttribute.Security], 0, TemplateManager.global.maxCouncilorAttribute);
            attributes[CouncilorAttribute.Loyalty] = Mathf.Clamp(template.loyalty ?? attributes[CouncilorAttribute.Loyalty], 0, TemplateManager.global.maxCouncilorAttribute);
            attributes[CouncilorAttribute.ApparentLoyalty] = attributes[CouncilorAttribute.Loyalty];
        }

        private void RandomizeTraits(TIFactionState faction)
        {
            traits.Clear();
            traitTemplateNames.Clear();
            int num = 0;
            List<int> list = new List<int>();
            foreach (TITraitTemplate item2 in TemplateManager.IterateByClass<TITraitTemplate>(allowChild: false))
            {
                float individualTraitChance = GetIndividualTraitChance(item2, faction);
                if (!item2.grouping.HasValue || individualTraitChance >= 100f)
                {
                    if ((float)Mathd.d100() <= individualTraitChance)
                    {
                        AddTrait(item2);
                        if (item2.grouping.HasValue)
                        {
                            list.Add(item2.grouping.Value);
                        }
                    }
                }
                else if (item2.grouping.HasValue && item2.grouping > num)
                {
                    num = item2.grouping.Value;
                }
            }
            Dictionary<string, float> dictionary = new Dictionary<string, float>();
            for (int i = 1; i <= num; i++)
            {
                if (list.Contains(i))
                {
                    continue;
                }
                float num2 = 0f;
                dictionary.Clear();
                foreach (TITraitTemplate item3 in TemplateManager.IterateByClass<TITraitTemplate>(allowChild: false))
                {
                    if (item3.grouping == i)
                    {
                        dictionary.Add(item3.dataName, GetIndividualTraitChance(item3, faction));
                        num2 += GetIndividualTraitChance(item3, faction);
                    }
                }
                if (dictionary.Count > 0)
                {
                    if (num2 < 100f)
                    {
                        dictionary.Add("", Math.Max(100f - num2, 0f));
                    }
                    string key = dictionary.SelectRandomWeightedItem((KeyValuePair<string, float> j) => j.Value).Key;
                    //TITraitTemplate item = TemplateManager.Find(key, typeof(TITraitTemplate));
                    TITraitTemplate item = TemplateManager.Find<TITraitTemplate>(key);
                    if (key != "" && item != null && !traits.Contains(item)) { 
                        if (key != "" && !traits.Contains(item))
                        {
                            AddTrait(item);
                        }
                    }   
                }
            }
            foreach (TITraitTemplate item4 in traits.ToList())
            {
                item4.RerollTrait(this, faction);
            }
        }

        private string SelectAppearance()
        {
            if (!string.IsNullOrEmpty(template.appearanceTemplateName))
            {
                TICouncilorAppearanceTemplate tICouncilorAppearanceTemplate = TemplateManager.Find<TICouncilorAppearanceTemplate>(template.appearanceTemplateName);
                if (tICouncilorAppearanceTemplate != null && tICouncilorAppearanceTemplate.enable)
                {
                    return tICouncilorAppearanceTemplate.dataName;
                }
            }
            int year = GameStateManager.Time().template.year - 50;
            List<TICouncilorAppearanceTemplate> list = (from aTemplate in TemplateManager.IterateByClass<TICouncilorAppearanceTemplate>()
                                                        where aTemplate.ValidForCharacter(this, year, requireJobAlignment: true, requireAncestryAlignment: true, requireNotDuplicated: true)
                                                        select aTemplate).ToList();
            if (list.Count == 0)
            {
                list = (from aTemplate in TemplateManager.IterateByClass<TICouncilorAppearanceTemplate>()
                        where aTemplate.ValidForCharacter(this, year, requireJobAlignment: false, requireAncestryAlignment: true, requireNotDuplicated: true)
                        select aTemplate).ToList();
                if (list.Count == 0)
                {
                    list = (from aTemplate in TemplateManager.IterateByClass<TICouncilorAppearanceTemplate>()
                            where aTemplate.ValidForCharacter(this, year, requireJobAlignment: false, requireAncestryAlignment: false, requireNotDuplicated: true)
                            select aTemplate).ToList();
                    if (list.Count == 0)
                    {
                        list = (from aTemplate in TemplateManager.IterateByClass<TICouncilorAppearanceTemplate>()
                                where aTemplate.ValidForCharacter(this, year, requireJobAlignment: false, requireAncestryAlignment: false, requireNotDuplicated: false)
                                select aTemplate).ToList();
                    }
                }
            }
            if (list.Count > 0)
            {
                return list.SelectRandomItem().dataName;
            }
            if (gender != CouncilorGender.Female)
            {
                return "CharImage6";
            }
            return "CharImage2";
        }



        //        //        [MonoModReplace]
        //        //        public TIResourcesCost HireRecruitCost(TIFactionState faction) {
        //        //            Log.Debug($"=== HireRecruitCost === faction.ideology null: {faction.ideology == null}, template null: {template == null}");
        //        //            TIResourcesCost tIResourcesCost = new TIResourcesCost();
        //        //            float resourceAmount = 0f;
        //        //            Log.Debug($"faction.ideology.alien: {faction.ideology.alien}, template alien: {template.alien} ");

        //        //            if (!faction.ideology.alien && !template.alien) {
        //        //                Log.Debug("Not Alien");
        //        //                var temp1 = typeTemplate.affinities.Contains(faction.ideology.ideology);
        //        //                Log.Debug($"temp1: {temp1}");
        //        //                var temp2 = TemplateManager.global.affinityCouncilorRecruitCost_influence;
        //        //                Log.Debug($"temp2: {temp2}");
        //        //                var temp3 = typeTemplate.antiAffinities.Contains(faction.ideology.ideology);
        //        //                Log.Debug($"temp3: {temp3}");
        //        //                var temp4 = TemplateManager.global.baseCouncilorRecruitCost_influence;
        //        //                Log.Debug($"temp4: {temp4}");
        //        //                var temp5 = TemplateManager.global.antiAffinityCouncilorRecruitCost_influence;
        //        //                Log.Debug($"temp5: {temp5}");
        //        //                resourceAmount = temp1 ? temp2 : (!temp3 ? temp4 : temp5);
        //        //                //resourceAmount = (typeTemplate.affinities.Contains(faction.ideology.ideology) ? ((float)TemplateManager.global.affinityCouncilorRecruitCost_influence) : ((!typeTemplate.antiAffinities.Contains(faction.ideology.ideology)) ? ((float)TemplateManager.global.baseCouncilorRecruitCost_influence) : ((float)TemplateManager.global.antiAffinityCouncilorRecruitCost_influence)));
        //        //            }
        //        //            tIResourcesCost.AddCost(FactionResource.Influence, resourceAmount);
        //        //            return tIResourcesCost;
        //        //        }
    }
}
