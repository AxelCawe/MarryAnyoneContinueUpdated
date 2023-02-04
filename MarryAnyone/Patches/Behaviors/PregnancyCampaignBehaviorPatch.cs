// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.Behaviors.PregnancyCampaignBehaviorPatch
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using Helpers;
using MarryAnyone.Behaviors;
using MarryAnyone.Helpers;
using MarryAnyone.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MarryAnyone.Patches.Behaviors
{
  [HarmonyPatch(typeof (PregnancyCampaignBehavior))]
  internal static class PregnancyCampaignBehaviorPatch
  {
    private static readonly ShortLifeObject _shortLifeObject = new ShortLifeObject(100);
    private static List<Hero> _spouses;
    private static Hero _sideFemaleHero;
    private static bool _playerRelation = false;
    private static ForHero _forHero = (ForHero) null;

    //[HarmonyPatch(typeof (PregnancyCampaignBehavior), "HeroPregnancyCheckCondition", new Type[] {typeof (Hero)})]
    //[HarmonyPrefix]
    //private static bool HeroPregnancyCheckConditionPatch(
    //  Hero hero,
    //  PregnancyCampaignBehavior __instance,
    //  ref bool __result)
    //{
    //  __result = hero.IsFemale && hero.IsAlive && (double) hero.Age > (double) Campaign.Current.Models.AgeModel.HeroComesOfAge && !CampaignOptions.IsLifeDeathCycleDisabled;
    //  return false;
    //}

    [HarmonyPatch(typeof (PregnancyCampaignBehavior), "DailyTickHero", new Type[] {typeof (Hero)})]
    [HarmonyPrefix]
    private static void DailyTickHeroPrefix(Hero hero)
    {
      if (!PregnancyCampaignBehaviorPatch._shortLifeObject.Swap((object) hero))
        return;
      if (PregnancyCampaignBehaviorPatch._forHero != null && PregnancyCampaignBehaviorPatch._forHero.Swap)
        PregnancyCampaignBehaviorPatch._forHero.UnSwap();
      PregnancyCampaignBehaviorPatch._forHero = new ForHero(hero);
      PregnancyCampaignBehaviorPatch._spouses = (List<Hero>) null;
      PregnancyCampaignBehaviorPatch._sideFemaleHero = (Hero) null;
      PregnancyCampaignBehaviorPatch._playerRelation = false;
      if (hero.IsFemale && HeroInteractionHelper.OkToDoIt(hero))
      {
        bool flag1 = MARomanceCampaignBehavior.Instance != null;
        bool flag2 = flag1 && MARomanceCampaignBehavior.Instance.PartnerOfPlayer(hero);
        if (hero.Spouse != null || flag2 || hero.ExSpouses != null && hero.ExSpouses.IsEmpty())
        {
          if (hero == Hero.MainHero | flag2 || flag1 && MARomanceCampaignBehavior.Instance.SpouseOfPlayer(hero))
          {
            PregnancyCampaignBehaviorPatch._playerRelation = true;
            MASettings maSettings = Helper.MASettings;
            if (PregnancyCampaignBehaviorPatch._spouses == null)
              PregnancyCampaignBehaviorPatch._spouses = new List<Hero>();
            if ((flag2 || Hero.MainHero.ExSpouses.Contains(hero)) && HeroInteractionHelper.OkToDoIt(hero, Hero.MainHero))
              PregnancyCampaignBehaviorPatch._spouses.Add(Hero.MainHero);
            if (hero.Spouse != null && HeroInteractionHelper.OkToDoIt(hero, hero.Spouse, Helper.MASettings.ImproveBattleRelation) && PregnancyCampaignBehaviorPatch._spouses.IndexOf(hero.Spouse) < 0)
              PregnancyCampaignBehaviorPatch._spouses.Add(hero.Spouse);
            if (maSettings.Polyamory)
            {
              if (MARomanceCampaignBehavior.Instance.Partners != null)
              {
                foreach (Hero partner in MARomanceCampaignBehavior.Instance.Partners)
                {
                  if (partner != hero && HeroInteractionHelper.OkToDoIt(hero, partner) && PregnancyCampaignBehaviorPatch._spouses.IndexOf(partner) < 0)
                    PregnancyCampaignBehaviorPatch._spouses.Add(partner);
                }
              }
              if (Hero.MainHero.ExSpouses != null)
              {
                foreach (Hero exSpouse in Hero.MainHero.ExSpouses)
                {
                  if (exSpouse.IsAlive && exSpouse != hero && HeroInteractionHelper.OkToDoIt(hero, exSpouse) && PregnancyCampaignBehaviorPatch._spouses.IndexOf(exSpouse) < 0)
                    PregnancyCampaignBehaviorPatch._spouses.Add(exSpouse);
                }
              }
            }
          }
          else
          {
            if (PregnancyCampaignBehaviorPatch._spouses == null)
              PregnancyCampaignBehaviorPatch._spouses = new List<Hero>();
            if (hero.Spouse != null && HeroInteractionHelper.OkToDoIt(hero, hero.Spouse, false) && PregnancyCampaignBehaviorPatch._spouses.IndexOf(hero.Spouse) < 0)
              PregnancyCampaignBehaviorPatch._spouses.Add(hero.Spouse);
            if (hero.ExSpouses != null)
            {
              foreach (Hero exSpouse in hero.ExSpouses)
              {
                if (exSpouse.IsAlive && exSpouse != hero && HeroInteractionHelper.OkToDoIt(hero, exSpouse) && PregnancyCampaignBehaviorPatch._spouses.IndexOf(exSpouse) < 0)
                  PregnancyCampaignBehaviorPatch._spouses.Add(exSpouse);
              }
            }
          }
        }
        int index = -1;
        if (PregnancyCampaignBehaviorPatch._spouses != null && PregnancyCampaignBehaviorPatch._spouses.Count > 1)
        {
          List<int> intList = new List<int>();
          int num1 = 0;
          foreach (Hero spouse in PregnancyCampaignBehaviorPatch._spouses)
          {
            int attractionValuePercentage = Campaign.Current.Models.RomanceModel.GetAttractionValuePercentage(hero, spouse);
            num1 += attractionValuePercentage * (spouse.IsFemale ? 1 : 3);
            intList.Add(num1);
          }
          int num2 = MBRandom.RandomInt(num1);
          Helper.Print("Random: " + num2.ToString(), Helper.PrintHow.PrintDisplay);
          index = 0;
          while (index < PregnancyCampaignBehaviorPatch._spouses.Count && num2 > intList[index])
            ++index;
        }
        else if (PregnancyCampaignBehaviorPatch._spouses != null && ((IEnumerable<Hero>) PregnancyCampaignBehaviorPatch._spouses).Count<Hero>() == 1)
          index = 0;
        if (index >= 0)
        {
          if (index >= PregnancyCampaignBehaviorPatch._spouses.Count)
            index = PregnancyCampaignBehaviorPatch._spouses.Count - 1;
          PregnancyCampaignBehaviorPatch._forHero.SwapSpouse(PregnancyCampaignBehaviorPatch._spouses[index]);
        }
        else
        {
          if (Helper.MASettings.Debug)
          {
            if (hero == Hero.MainHero)
              Helper.Print("No spouse or cheating partner allowed for your sex time", Helper.PrintHow.PrintDisplay);
            else if (MARomanceCampaignBehavior.Instance != null && (MARomanceCampaignBehavior.Instance.SpouseOfPlayer(hero) || MARomanceCampaignBehavior.Instance.PartnerOfPlayer(hero)))
              Helper.Print(string.Format("No spouse or cheating partner allowed for {0} sex time", (object) hero.Name), Helper.PrintHow.PrintDisplay);
          }
          hero.Spouse = (Hero) null;
        }
      }
      if (hero.Spouse == null || hero.IsFemale != hero.Spouse.IsFemale)
        return;
      PregnancyCampaignBehaviorPatch._sideFemaleHero = hero.Spouse;
      hero.Spouse.Spouse = (Hero) null;
      hero.Spouse = (Hero) null;
    }

    [HarmonyPatch(typeof (PregnancyCampaignBehavior), "DailyTickHero", new Type[] {typeof (Hero)})]
    [HarmonyPostfix]
    private static void DailyTickHeroPostfix(Hero hero)
    {
      if (hero.Spouse == null && PregnancyCampaignBehaviorPatch._sideFemaleHero != null)
        hero.Spouse = PregnancyCampaignBehaviorPatch._sideFemaleHero;
      bool flag1 = PregnancyCampaignBehaviorPatch._forHero != null && PregnancyCampaignBehaviorPatch._forHero._hero == hero && PregnancyCampaignBehaviorPatch._forHero._spouse == hero.Spouse;
      Hero hero1 = (Hero) null;
      if (hero == Hero.MainHero)
        hero1 = hero.Spouse;
      else if (hero.Spouse == Hero.MainHero)
        hero1 = hero;
      if (flag1 && Helper.MASettings.ImproveRelation && hero != null && hero.Spouse != null)
      {
        bool flag2 = PregnancyCampaignBehaviorPatch._playerRelation;
        if (flag2 && !Helper.MASettings.NotifyRelationImprovementWithinFamily)
          flag2 = hero1 != null;
        bool flag3 = hero.IsPregnant & flag1 && !PregnancyCampaignBehaviorPatch._forHero._wasPregnant;
        int num1 = 0;
        float randomFloat = MBRandom.RandomFloat;
        int relation = hero.GetRelation(hero.Spouse);
        int num2 = (Helper.TraitCompatibility(hero, hero.Spouse, DefaultTraits.Calculating) + Helper.TraitCompatibility(hero, hero.Spouse, DefaultTraits.Generosity) * 3 + Helper.TraitCompatibility(hero, hero.Spouse, DefaultTraits.Valor)) / 2;
        if (flag3)
          num1 = (int) ((double) randomFloat * 9.0) + 1 + (num2 > 0 ? num2 * 2 : 0);
        int num3 = relation >= 0 ? (relation >= 25 ? (relation >= 50 ? (int) ((double) randomFloat * 5.0) + (num2 <= -3 ? -2 : num2) : (int) ((double) randomFloat * 4.0) - 1 + (num2 <= -2 ? -1 : num2)) : (int) ((double) randomFloat * 5.0) - 2 + (num2 <= -2 ? -1 : num2)) : (int) ((double) randomFloat * 6.0) - 3 + (num2 <= -2 ? -1 : num2);
        if (num3 != 0 && flag2)
        {
          ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, hero.Spouse, num3, false);
          StringHelpers.SetCharacterProperties("HEROONE", hero.CharacterObject, (TextObject) null, false);
          StringHelpers.SetCharacterProperties("HEROTOW", hero.Spouse.CharacterObject, (TextObject) null, false);
          MBTextManager.SetTextVariable("INCREMENT", num3);
          MBTextManager.SetTextVariable("FINALRELATION", hero.GetRelation(hero.Spouse));
          Color color = num3 < 0 ? Color.FromUint(16722716U) : (flag3 ? Helper.yellowCollor : Color.FromUint(6750105U));
          if (flag3)
            Helper.PrintWithColor(((object) new TextObject("{=TheTwoOfThemHaveAGoodTime}{HEROONE.NAME} and {HEROTOW.NAME} have a good time together, their relationship up from {INCREMENT} points to {FINALRELATION}", (Dictionary<string, object>) null)).ToString(), Helper.yellowCollor);
          else
            Helper.PrintWithColor((num3 <= 0 ? (object) new TextObject("{=TheTwoOfThemSpendTimeDown}{HEROONE.NAME} and {HEROTOW.NAME} don't have a good time together, their relationship down from {INCREMENT} points to {FINALRELATION}", (Dictionary<string, object>) null) : (object) new TextObject("{=TheTwoOfThemSpendTime}{HEROONE.NAME} and {HEROTOW.NAME} spend some time together, their relationship up from {INCREMENT} points to {FINALRELATION}", (Dictionary<string, object>) null)).ToString(), color);
        }
      }
      if (PregnancyCampaignBehaviorPatch._forHero != null)
        PregnancyCampaignBehaviorPatch._forHero.UnSwap();
      PregnancyCampaignBehaviorPatch._forHero = (ForHero) null;
    }

    public static void Done() => PregnancyCampaignBehaviorPatch._shortLifeObject.Done();
  }
}
