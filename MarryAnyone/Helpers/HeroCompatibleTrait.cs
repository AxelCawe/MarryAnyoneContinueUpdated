// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Helpers.HeroCompatibleTrait
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;


namespace MarryAnyone.Helpers
{
  public class HeroCompatibleTrait
  {
    private Hero _hero;
    private Hero _otherHero;

    public HeroCompatibleTrait(Hero hero, Hero otherHero)
    {
      this._hero = hero;
      this._otherHero = otherHero;
    }

    public int TraitCompatible(TraitObject trait, TraitObject otherTrait, bool signeDifferent = false)
    {
      int traitLevel1 = this._hero.GetTraitLevel(trait);
      int traitLevel2 = this._otherHero.GetTraitLevel(otherTrait);
      if (signeDifferent)
        traitLevel1 *= -1;
      return traitLevel1 <= 0 && traitLevel2 >= 0 ? 0 : Math.Abs(Math.Min(traitLevel1, traitLevel2));
    }
  }
}
