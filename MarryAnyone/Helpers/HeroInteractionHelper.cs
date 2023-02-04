// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Helpers.HeroInteractionHelper
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Localization;


namespace MarryAnyone.Helpers
{
  internal static class HeroInteractionHelper
  {
    public static int MAX_COMPATIBLE_BATTLE_TRAIT = 14;
    public static int MAX_COMPATIBLE_BATTLE_TRAIT_ON_3 = 4;
    public static int MAX_COMPATIBLE_BATTLE_TRAIT_ON_7 = 2;

    public static bool CanIntegreSpouseInHeroClan(Hero hero, Hero spouse) => !spouse.IsFactionLeader || spouse.IsMinorFactionHero || hero.Clan.Kingdom == spouse.Clan.Kingdom || hero.Clan.Kingdom?.Leader == hero;

    public static bool HeroCanMeet(Hero hero, Hero otherHero) => hero.CurrentSettlement != null && hero.CurrentSettlement == otherHero.CurrentSettlement || hero.PartyBelongedTo != null && hero.PartyBelongedTo == otherHero.PartyBelongedTo;

    public static bool OkToDoIt(Hero hero, Hero otherHero = null, bool withRelationTest = true)
    {
      if (!hero.IsAlive || (double) hero.Age < (double) Campaign.Current.Models.AgeModel.HeroComesOfAge)
        return false;
      if (otherHero != null)
      {
        if (!otherHero.IsAlive || (double) otherHero.Age < (double) Campaign.Current.Models.AgeModel.HeroComesOfAge || !HeroInteractionHelper.HeroCanMeet(hero, otherHero))
          return false;
        if (withRelationTest && Helper.MASettings.RelationLevelMinForSex >= 0)
          return hero.GetRelation(otherHero) + (Helper.TraitCompatibility(hero, otherHero, DefaultTraits.Calculating) + Helper.TraitCompatibility(hero, otherHero, DefaultTraits.Generosity) * 2 + Helper.TraitCompatibility(hero, otherHero, DefaultTraits.Valor) + Helper.TraitCompatibility(hero, otherHero, DefaultTraits.Honor)) > Helper.MASettings.RelationLevelMinForSex;
      }
      return true;
    }

    public static int PositiveTraits(Hero hero) => hero.GetTraitLevel(DefaultTraits.Honor) + hero.GetTraitLevel(DefaultTraits.Valor) + hero.GetTraitLevel(DefaultTraits.Generosity) + hero.GetTraitLevel(DefaultTraits.Mercy);

    public static int CompatibleBattleTraits(Hero hero, Hero otherHero)
    {
      HeroCompatibleTrait heroCompatibleTrait = new HeroCompatibleTrait(hero, otherHero);
      return heroCompatibleTrait.TraitCompatible(DefaultTraits.Honor, DefaultTraits.Honor) + heroCompatibleTrait.TraitCompatible(DefaultTraits.Mercy, DefaultTraits.Valor, true) + heroCompatibleTrait.TraitCompatible(DefaultTraits.Valor, DefaultTraits.Mercy, true) + heroCompatibleTrait.TraitCompatible(DefaultTraits.Generosity, DefaultTraits.Valor) + heroCompatibleTrait.TraitCompatible(DefaultTraits.Valor, DefaultTraits.Generosity) + heroCompatibleTrait.TraitCompatible(DefaultTraits.Calculating, DefaultTraits.Valor) + heroCompatibleTrait.TraitCompatible(DefaultTraits.Valor, DefaultTraits.Calculating);
    }

    internal static void ChangeHeroRelation(
      Hero hero1,
      Hero hero2,
      int coeff,
      TextObject raison = null,
      int maxCoeff = 5,
      HeroInteractionHelper.ShowWhat showWhat = HeroInteractionHelper.ShowWhat.ShowNotification)
    {
      if (hero1 == null || hero2 == null)
        return;
      bool flag = false;
      if (coeff > maxCoeff)
        coeff = maxCoeff;
      else if (coeff < -maxCoeff)
        coeff = -maxCoeff;
      coeff = coeff <= 0 ? Convert.ToInt32(Math.Round((double) MBRandom.RandomInt(coeff * 10, 0) / 10.0)) : Convert.ToInt32(Math.Round((double) MBRandom.RandomInt(0, coeff * 10) / 10.0));
      if (coeff == 0)
        return;
      if (raison != null)
        showWhat &= ~HeroInteractionHelper.ShowWhat.ShowNotification;
      if (hero1 == Hero.MainHero)
      {
        ChangeRelationAction.ApplyPlayerRelation(hero2, coeff, false, (showWhat & HeroInteractionHelper.ShowWhat.ShowNotification) != 0);
        flag = true;
      }
      if (hero2 == Hero.MainHero)
      {
        ChangeRelationAction.ApplyPlayerRelation(hero1, coeff, false, (showWhat & HeroInteractionHelper.ShowWhat.ShowNotification) != 0);
        flag = true;
      }
      else
        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero1, hero2, coeff, (showWhat & HeroInteractionHelper.ShowWhat.ShowNotification) != 0);
      if (raison == null)
        return;
      TextObject textObject = (showWhat & HeroInteractionHelper.ShowWhat.ShowFinalRelation) == HeroInteractionHelper.ShowWhat.ShowRAS ? (!flag ? (coeff <= 0 ? new TextObject("{=ChangeRelationDownTo} Their relation down by {RELATION}", (Dictionary<string, object>) null) : new TextObject("{=ChangeRelationUpTo} Their relation up by {RELATION}", (Dictionary<string, object>) null)) : (coeff <= 0 ? new TextObject("{=ChangeRelationDownToWithPlayer} Your relation down by {RELATION}", (Dictionary<string, object>) null) : new TextObject("{=ChangeRelationUpToWithPlayer} Your relation up by {RELATION}", (Dictionary<string, object>) null))) : (!flag ? (coeff <= 0 ? new TextObject("{=ChangeRelationDownToFinal} Their relation down by {RELATION} to {FINALRELATION}", (Dictionary<string, object>) null) : new TextObject("{=ChangeRelationUpToFinal} Their relation up by {RELATION} to {FINALRELATION}", (Dictionary<string, object>) null)) : (coeff <= 0 ? new TextObject("{=ChangeRelationDownToFinalWithPlayer} Your relation down by {RELATION} to {FINALRELATION}", (Dictionary<string, object>) null) : new TextObject("{=ChangeRelationUpToFinalWithPlayer} Your relation up by {RELATION} to {FINALRELATION}", (Dictionary<string, object>) null)));
      textObject.SetTextVariable("RELATION", coeff.ToString());
      textObject.SetTextVariable("FINALRELATION", hero1.GetRelation(hero2).ToString());
      Helper.PrintWithColor(((object) raison).ToString() + ((object) textObject).ToString(), coeff > 0 ? 6750105U : 16722716U);
    }

    public enum ShowWhat
    {
      ShowRAS = 0,
      ShowNotification = 1,
      ShowThroughHelper = 2,
      ShowFinalRelation = 4,
    }
  }
}
