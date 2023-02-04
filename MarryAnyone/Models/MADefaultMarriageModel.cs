// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Models.MADefaultMarriageModel
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using MarryAnyone.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;


 
namespace MarryAnyone.Models
{
  public class MADefaultMarriageModel : DefaultMarriageModel
  {
    public static bool IsCoupleSuitableForMarriageStatic(
      Hero firstHero,
      Hero secondHero,
      bool canCheat)
    {
      ISettingsProvider maSettings = (ISettingsProvider) Helper.MASettings;
      bool flag1 = firstHero == Hero.MainHero || secondHero == Hero.MainHero;
      bool flag2 = maSettings.SexualOrientation == "Homosexual" & flag1;
      bool flag3 = maSettings.SexualOrientation == "Bisexual" & flag1;
      bool flag4 = maSettings.Incest & flag1;
      bool flag5 = MADefaultMarriageModel.DiscoverAncestors(firstHero, 3).Intersect<Hero>(MADefaultMarriageModel.DiscoverAncestors(secondHero, 3)).Any<Hero>();
      if (!flag1 && (firstHero.Clan == null || secondHero.Clan == null || firstHero.Spouse != null && !firstHero.Spouse.IsDead || secondHero.Spouse != null && !secondHero.Spouse.IsDead) || firstHero.Clan?.Leader == firstHero && !flag1 && secondHero.Clan?.Leader == secondHero)
        return false;
      if (!flag4 && flag5)
      {
        MADefaultMarriageModel.DiscoverAncestors(firstHero, 3).Intersect<Hero>(MADefaultMarriageModel.DiscoverAncestors(secondHero, 3)).ToList<Hero>();
        return false;
      }
      return flag2 ? firstHero.IsFemale == secondHero.IsFemale && MADefaultMarriageModel.IsSuitableForMarriageStatic(firstHero, canCheat) && MADefaultMarriageModel.IsSuitableForMarriageStatic(secondHero, canCheat) : (flag3 ? MADefaultMarriageModel.IsSuitableForMarriageStatic(firstHero, canCheat) && MADefaultMarriageModel.IsSuitableForMarriageStatic(secondHero, canCheat) : firstHero.IsFemale != secondHero.IsFemale && MADefaultMarriageModel.IsSuitableForMarriageStatic(firstHero, canCheat) && MADefaultMarriageModel.IsSuitableForMarriageStatic(secondHero, canCheat));
    }

    public override bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero) => MADefaultMarriageModel.IsCoupleSuitableForMarriageStatic(firstHero, secondHero, false);

    public static IEnumerable<Hero> DiscoverAncestors(Hero hero, int n)
    {
      Hero hero1 = hero;
      if (hero1 != null)
      {
        yield return hero1;
        int num = n;
        if (num > 0)
        {
          foreach (Hero discoverAncestor in MADefaultMarriageModel.DiscoverAncestors(hero1.Mother, num - 1))
            yield return discoverAncestor;
          foreach (Hero discoverAncestor in MADefaultMarriageModel.DiscoverAncestors(hero1.Father, num - 1))
            yield return discoverAncestor;
        }
      }
    }

    public static bool IsSuitableForCheatingStatic(Hero maidenOrSuitor)
    {
      if (!Helper.IsSuitableForMarriagePathMA(maidenOrSuitor) || !Helper.MASettings.Cheating || maidenOrSuitor.Spouse == null && (maidenOrSuitor.ExSpouses == null || !((IEnumerable<Hero>) maidenOrSuitor.ExSpouses).Any<Hero>((Func<Hero, bool>) (exSpouse => exSpouse.IsAlive))))
        return false;
      return maidenOrSuitor.IsFemale ? (double) ((BasicCharacterObject) maidenOrSuitor.CharacterObject).Age >= (double) Campaign.Current.Models.MarriageModel.MinimumMarriageAgeFemale : (double) ((BasicCharacterObject) maidenOrSuitor.CharacterObject).Age >= (double) Campaign.Current.Models.MarriageModel.MinimumMarriageAgeMale;
    }

    public static bool IsSuitableForMarriageStatic(Hero maidenOrSuitor, bool canCheat = false)
    {
      if (!Helper.IsSuitableForMarriagePathMA(maidenOrSuitor))
        return false;
      int num;
      bool flag1 = (num = 0) != 0;
      bool flag2 = num != 0;
      bool flag3 = num != 0;
      if (maidenOrSuitor == Hero.MainHero)
      {
        flag2 = Helper.MASettings.Cheating;
        flag1 = Helper.MASettings.Polygamy;
      }
      else if (canCheat)
        flag2 = Helper.MASettings.Cheating;
      if (!(flag1 | flag2) && (maidenOrSuitor.Spouse != null || ((IEnumerable<Hero>) maidenOrSuitor.ExSpouses).Any<Hero>((Func<Hero, bool>) (exSpouse => exSpouse.IsAlive))))
        return false;
      return maidenOrSuitor.IsFemale ? (double) ((BasicCharacterObject) maidenOrSuitor.CharacterObject).Age >= (double) Campaign.Current.Models.MarriageModel.MinimumMarriageAgeFemale : (double) ((BasicCharacterObject) maidenOrSuitor.CharacterObject).Age >= (double) Campaign.Current.Models.MarriageModel.MinimumMarriageAgeMale;
    }

    public override bool IsSuitableForMarriage(Hero maidenOrSuitor) => MADefaultMarriageModel.IsSuitableForMarriageStatic(maidenOrSuitor);
  }
}
