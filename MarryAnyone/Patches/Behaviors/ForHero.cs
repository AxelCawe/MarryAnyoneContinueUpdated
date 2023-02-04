// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.Behaviors.ForHero
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using MarryAnyone.Behaviors;
using TaleWorlds.CampaignSystem;


 
namespace MarryAnyone.Patches.Behaviors
{
  internal class ForHero
  {
    public Hero _hero;
    public Hero _spouse;
    public Hero _sauveHeroSpouse;
    public Hero _sauveSpouseSpouse;
    public bool _wasPregnant;
    public bool _wasSpousePregnant;
    public bool _canKeep;
    public bool Swap;

    public ForHero(Hero hero)
    {
      this._hero = hero;
      this._wasPregnant = hero.IsPregnant;
      if (hero.Spouse == null)
        return;
      this._spouse = hero.Spouse;
      this._wasSpousePregnant = hero.Spouse.IsPregnant;
    }

    public void SwapSpouse(Hero spouse)
    {
      if (spouse == this._hero.Spouse)
        return;
      this.Swap = true;
      this._sauveHeroSpouse = this._hero.Spouse;
      this._sauveSpouseSpouse = (Hero) null;
      this._spouse = spouse;
      if (this._spouse != null)
      {
        if (this._hero == Hero.MainHero && MARomanceCampaignBehavior.Instance != null)
          this._canKeep = MARomanceCampaignBehavior.Instance.SpouseOfPlayer(this._spouse);
        else if (this._spouse == Hero.MainHero && MARomanceCampaignBehavior.Instance != null)
          this._canKeep = MARomanceCampaignBehavior.Instance.SpouseOfPlayer(this._hero);
        this._sauveSpouseSpouse = this._spouse.Spouse;
        if (this._canKeep)
        {
          if (this._hero == Hero.MainHero)
            this._hero.Spouse = this._spouse;
          else
            this._spouse.Spouse = this._hero;
          Helper.RemoveExSpouses(this._hero);
          Helper.RemoveExSpouses(this._spouse);
        }
        else
          Helper.SetSpouse(this._spouse, this._hero, Helper.enuSetSpouse.JustSet);
        this._wasSpousePregnant = this._spouse.IsPregnant;
      }
      else
      {
        this._sauveSpouseSpouse = (Hero) null;
        this._wasSpousePregnant = false;
      }
      if (this._canKeep)
        return;
      Helper.SetSpouse(this._hero, this._spouse, Helper.enuSetSpouse.JustSet);
    }

    public void UnSwap()
    {
      if (!this.Swap || this._canKeep)
        return;
      if (this._spouse != null)
        Helper.SetSpouse(this._spouse, this._sauveSpouseSpouse, Helper.enuSetSpouse.JustSet);
      Helper.SetSpouse(this._hero, this._sauveHeroSpouse, Helper.enuSetSpouse.JustSet);
      this.Swap = false;
    }
  }
}
