// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.Behaviors.EducationCampaignBehaviorPatch
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;



namespace MarryAnyone.Patches.Behaviors
{
    [HarmonyPatch(typeof(EducationCampaignBehavior), "GetHighestThreeAttributes")]
    internal class EducationCampaignBehaviorPatch
    {
        //[HarmonyPrefix]
        private static void Prefix(ref Hero hero)
        {
            if (hero != null)
                return;
            hero = Hero.MainHero;
        }
    }
}
