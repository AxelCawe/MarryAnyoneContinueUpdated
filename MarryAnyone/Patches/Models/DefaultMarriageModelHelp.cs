// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.Models.DefaultMarriageModelHelp
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using System.Collections.Generic;
using TaleWorlds.CampaignSystem;


namespace MarryAnyone.Patches.Models
{
  internal static class DefaultMarriageModelHelp
  {
    public static IEnumerable<Hero> DiscoverAncestors(Hero hero, int n)
    {

      if (hero != null)
      {
        yield return hero;
        if (n > 0)
        {
          foreach (Hero discoverAncestor in DefaultMarriageModelHelp.DiscoverAncestors(hero.Mother, n - 1))
            yield return discoverAncestor;
          foreach (Hero discoverAncestor in DefaultMarriageModelHelp.DiscoverAncestors(hero.Father, n - 1))
            yield return discoverAncestor;
        }
      }
    }
  }
}
