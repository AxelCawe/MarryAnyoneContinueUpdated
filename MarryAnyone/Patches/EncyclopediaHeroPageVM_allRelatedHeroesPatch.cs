// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.EncyclopediaHeroPageVM_allRelatedHeroesPatch
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;

namespace MarryAnyone.Patches
{
  [HarmonyPatch(typeof (EncyclopediaHeroPageVM))]
  internal static class EncyclopediaHeroPageVM_allRelatedHeroesPatch
  {
    private static List<Hero> _heroes;
    private static FieldInfo _accesHero;
    private static Hero _hero;

    public static void Dispose()
    {
      EncyclopediaHeroPageVM_allRelatedHeroesPatch._hero = (Hero) null;
      EncyclopediaHeroPageVM_allRelatedHeroesPatch._heroes = (List<Hero>) null;
      EncyclopediaHeroPageVM_allRelatedHeroesPatch._accesHero = (FieldInfo) null;
    }

    [HarmonyPatch(typeof (EncyclopediaHeroPageVM))]
    [HarmonyPatch("_allRelatedHeroes", MethodType.Getter)]
    [HarmonyPrefix]
    internal static bool _allRelatedHeroesTranspiler(
      EncyclopediaHeroPageVM __instance,
      ref IEnumerable<Hero> __result)
    {
      if (EncyclopediaHeroPageVM_allRelatedHeroesPatch._accesHero == (FieldInfo) null)
      {
        EncyclopediaHeroPageVM_allRelatedHeroesPatch._accesHero = AccessTools.Field(typeof (EncyclopediaHeroPageVM), "_hero");
        if (EncyclopediaHeroPageVM_allRelatedHeroesPatch._accesHero == (FieldInfo) null)
          throw new Exception("Field _hero inaccessible on EncyclopediaHeroPageVM");
      }
      Hero hero = (Hero) EncyclopediaHeroPageVM_allRelatedHeroesPatch._accesHero.GetValue((object) __instance);
      if (EncyclopediaHeroPageVM_allRelatedHeroesPatch._hero != hero || EncyclopediaHeroPageVM_allRelatedHeroesPatch._heroes == null)
      {
        int num = 0;
        EncyclopediaHeroPageVM_allRelatedHeroesPatch._hero = hero;
        EncyclopediaHeroPageVM_allRelatedHeroesPatch._heroes = new List<Hero>();
        EncyclopediaHeroPageVM_allRelatedHeroesPatch._heroes.Add(hero.Father);
        EncyclopediaHeroPageVM_allRelatedHeroesPatch._heroes.Add(hero.Mother);
        EncyclopediaHeroPageVM_allRelatedHeroesPatch._heroes.Add(hero.Spouse);
        foreach (Hero child in EncyclopediaHeroPageVM_allRelatedHeroesPatch._hero.Children)
          EncyclopediaHeroPageVM_allRelatedHeroesPatch._heroes.Add(child);
        foreach (Hero sibling in EncyclopediaHeroPageVM_allRelatedHeroesPatch._hero.Siblings)
        {
          if (EncyclopediaHeroPageVM_allRelatedHeroesPatch._heroes.IndexOf(sibling) < 0)
            EncyclopediaHeroPageVM_allRelatedHeroesPatch._heroes.Add(sibling);
          else
            ++num;
        }
        foreach (Hero exSpouse in EncyclopediaHeroPageVM_allRelatedHeroesPatch._hero.ExSpouses)
        {
          if (EncyclopediaHeroPageVM_allRelatedHeroesPatch._heroes.IndexOf(exSpouse) < 0)
            EncyclopediaHeroPageVM_allRelatedHeroesPatch._heroes.Add(exSpouse);
          else
            ++num;
        }
        Helper.Print(string.Format("_allRelatedHeroesPatch Work on Hero {0} nb Patch applies : {1}", hero == null ? (object) "NULL" : (object) hero.Name.ToString(), (object) num), Helper.PrintHow.PrintToLogAndWrite);
      }
      __result = (IEnumerable<Hero>) EncyclopediaHeroPageVM_allRelatedHeroesPatch._heroes;
      return false;
    }
  }
}
