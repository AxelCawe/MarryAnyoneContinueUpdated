// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.Behaviors.SandBoxSourceCampaignComponentsCompanionRolesCampaignBehaviorPatch
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using SandBox.CampaignBehaviors;
using System;
using TaleWorlds.CampaignSystem;


namespace MarryAnyone.Patches.Behaviors
{
  [HarmonyPatch(typeof (CompanionRolesCampaignBehavior))]
  internal static class SandBoxSourceCampaignComponentsCompanionRolesCampaignBehaviorPatch
  {
    private static bool IsChild(Hero child, Hero parent)
    {
      if (child.Father == parent || child.Mother == parent || parent.Spouse != null && (child.Father == parent.Spouse || child.Mother == parent.Spouse))
        return true;
      foreach (Hero exSpouse in parent.ExSpouses)
      {
        if (child.Father == exSpouse || child.Mother == exSpouse)
          return true;
      }
      return false;
    }

    [HarmonyPatch(typeof (CompanionRolesCampaignBehavior), "turn_companion_to_lord_on_condition")]
    [HarmonyPrefix]
    public static bool turn_companion_to_lord_on_conditionPatch(ref bool __result)
    {
      if (Hero.OneToOneConversationHero == null || !Hero.MainHero.MapFaction.IsKingdomFaction || !Hero.MainHero.IsFactionLeader || Hero.OneToOneConversationHero.Clan != Hero.MainHero.Clan || Hero.OneToOneConversationHero.Occupation !=Occupation.Lord)
        return true;
      __result = false;
      if (SandBoxSourceCampaignComponentsCompanionRolesCampaignBehaviorPatch.IsChild(Hero.OneToOneConversationHero, Hero.MainHero))
        __result = true;
      else if (Hero.MainHero.Father != null && SandBoxSourceCampaignComponentsCompanionRolesCampaignBehaviorPatch.IsChild(Hero.OneToOneConversationHero, Hero.MainHero.Father))
        __result = true;
      else if (Hero.MainHero.Mother != null && SandBoxSourceCampaignComponentsCompanionRolesCampaignBehaviorPatch.IsChild(Hero.OneToOneConversationHero, Hero.MainHero.Mother))
        __result = true;
      return false;
    }

    [HarmonyPatch(typeof (CompanionRolesCampaignBehavior), "ClanNameSelectionIsDone", new Type[] {typeof (string)})]
    [HarmonyPostfix]
    public static void ClanNameSelectionIsDonePatch()
    {
      if (Hero.OneToOneConversationHero == null || Hero.OneToOneConversationHero.Clan == Hero.MainHero.Clan)
        return;
      Helper.FamilyJoinClan(Hero.OneToOneConversationHero, Hero.MainHero.Clan, Hero.OneToOneConversationHero.Clan);
    }
  }
}
