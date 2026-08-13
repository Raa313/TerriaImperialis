//using MonoMod;

////Debug Class

//namespace PavonisInteractive.TerraInvicta {

//    public class patch_TICouncilorState : TICouncilorState {

//        [MonoModReplace]
//        public TIResourcesCost HireRecruitCost(TIFactionState faction) {
//            Log.Debug($"=== HireRecruitCost === faction.ideology null: {faction.ideology == null}, template null: {template == null}");
//            TIResourcesCost tIResourcesCost = new TIResourcesCost();
//            float resourceAmount = 0f;
//            Log.Debug($"faction.ideology.alien: {faction.ideology.alien}, template alien: {template.alien} ");

//            if (!faction.ideology.alien && !template.alien) {
//                Log.Debug("Not Alien");
//                var temp1 = typeTemplate.affinities.Contains(faction.ideology.ideology);
//                Log.Debug($"temp1: {temp1}");
//                var temp2 = TemplateManager.global.affinityCouncilorRecruitCost_influence;
//                Log.Debug($"temp2: {temp2}");
//                var temp3 = typeTemplate.antiAffinities.Contains(faction.ideology.ideology);
//                Log.Debug($"temp3: {temp3}");
//                var temp4 = TemplateManager.global.baseCouncilorRecruitCost_influence;
//                Log.Debug($"temp4: {temp4}");
//                var temp5 = TemplateManager.global.antiAffinityCouncilorRecruitCost_influence;
//                Log.Debug($"temp5: {temp5}");
//                resourceAmount = temp1 ? temp2 : (!temp3 ? temp4 : temp5);
//                //resourceAmount = (typeTemplate.affinities.Contains(faction.ideology.ideology) ? ((float)TemplateManager.global.affinityCouncilorRecruitCost_influence) : ((!typeTemplate.antiAffinities.Contains(faction.ideology.ideology)) ? ((float)TemplateManager.global.baseCouncilorRecruitCost_influence) : ((float)TemplateManager.global.antiAffinityCouncilorRecruitCost_influence)));
//            }
//            tIResourcesCost.AddCost(FactionResource.Influence, resourceAmount);
//            return tIResourcesCost;
//        }
//    }
//}
