// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.Behaviors.LordDefectionCampaignBehaviorPatch
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace MarryAnyone.Patches.Behaviors
{
  [HarmonyPatch(typeof (LordDefectionCampaignBehavior))]
  internal class LordDefectionCampaignBehaviorPatch
  {
    [HarmonyPatch(typeof (LordDefectionCampaignBehavior), "conversation_player_is_asking_to_recruit_enemy_on_condition", new Type[] {})]
    [HarmonyPrefix]
    public static bool conversation_player_is_asking_to_recruit_enemy_on_conditionPatch(
      ref bool __result)
    {
      if (Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.Clan != null)
        return true;
      __result = false;
      return false;
    }

    [HarmonyPatch(typeof (LordDefectionCampaignBehavior), "conversation_player_is_asking_to_recruit_neutral_on_condition", new Type[] {})]
    [HarmonyPrefix]
    public static bool conversation_player_is_asking_to_recruit_neutral_on_conditionPatch(
      ref bool __result)
    {
      if (Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.Clan != null)
        return true;
      __result = false;
      return false;
    }

    [HarmonyPatch(typeof (LordDefectionCampaignBehavior), "conversation_suggest_treason_on_condition", new Type[] {})]
    [HarmonyPrefix]
    public static bool conversation_suggest_treason_on_conditionPatch(ref bool __result)
    {
      if (Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.Clan != null)
        return true;
      __result = false;
      return false;
    }
  }
}
