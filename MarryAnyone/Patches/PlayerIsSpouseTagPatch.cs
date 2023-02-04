// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.PlayerIsSpouseTagPatch
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using MarryAnyone.Behaviors;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation.Tags;
using TaleWorlds.Core;


 
namespace MarryAnyone.Patches
{
  [HarmonyPatch(typeof (PlayerIsSpouseTag), "IsApplicableTo")]
  internal class PlayerIsSpouseTagPatch
  {
    [HarmonyPatch(typeof (PlayerIsSpouseTag), "IsApplicableTo", new Type[] {typeof (CharacterObject)})]
    [HarmonyPostfix]
    private static void PlayerIsSpouseTagIsApplicableTo(
      ref bool __result,
      CharacterObject character)
    {
      if (__result)
        return;
      if (MARomanceCampaignBehavior.Instance != null && ((BasicCharacterObject) character).IsHero)
        __result = MARomanceCampaignBehavior.Instance.SpouseOfPlayer(character.HeroObject);
      else
        __result = false;
    }
  }
}
