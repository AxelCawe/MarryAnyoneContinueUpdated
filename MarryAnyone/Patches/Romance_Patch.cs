// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.Romance_Patch
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using MarryAnyone.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;


 
namespace MarryAnyone.Patches
{
  [HarmonyPatch(typeof (Romance))]
  internal static class Romance_Patch
  {
    [HarmonyPatch(typeof (Romance), "GetCourtedHeroInOtherClan", new Type[] {typeof (Hero), typeof (Hero)})]
    [HarmonyPrefix]
    public static bool GetCourtedHeroInOtherClanPrefix(Hero person1, Hero person2, Hero __result)
    {
      __result = (Hero) null;
      if (person2.Clan != null)
      {
        foreach (Hero hero in ((IEnumerable<Hero>) person2.Clan.Lords).Where<Hero>((Func<Hero, bool>) (x => x != person2)))
        {
          if (Romance.GetRomanticLevel(person1, hero) >= Romance.RomanceLevelEnum.MatchMadeByFamily)
          {
            __result = hero;
            return false;
          }
        }
      }
      return false;
    }

    [HarmonyPatch(typeof (Romance), "EndAllCourtships", new Type[] {typeof (Hero)})]
    [HarmonyPrefix]
    private static bool EndAllCourtshipsPrefix(Hero forHero)
    {
      foreach (Romance.RomanticState romanticState in ((IEnumerable<Romance.RomanticState>) Romance.RomanticStateList).ToList<Romance.RomanticState>())
      {
        if ((romanticState.Person1 == forHero || romanticState.Person2 == forHero) && ((int)romanticState.Level == 7 || (int)romanticState.Level == 6 || (int)romanticState.Level == 5 || (int)romanticState.Level == 4) && ((int)romanticState.Level != 7 || !MARomanceCampaignBehavior.Instance.SpouseOrNot(romanticState.Person1, romanticState.Person2) || !Helper.MASettings.Polygamy))
        {
          romanticState.Level = Romance.RomanceLevelEnum.Ended;
          if (romanticState.Level == Romance.RomanceLevelEnum.Marriage)
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(romanticState.Person1, romanticState.Person2, -30, true);
          else if (romanticState.Level == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage)
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(romanticState.Person1, romanticState.Person2, -20, true);
          else if (romanticState.Level == Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible)
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(romanticState.Person1, romanticState.Person2, -10, true);
          else if (romanticState.Level == Romance.RomanceLevelEnum.CourtshipStarted)
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(romanticState.Person1, romanticState.Person2, -4, true);
        }
      }
      return false;
    }
  }
}
