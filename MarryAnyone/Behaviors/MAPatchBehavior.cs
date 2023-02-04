// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Behaviors.MAPatchBehavior
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;


namespace MarryAnyone.Behaviors
{
  internal class MAPatchBehavior : CampaignBehaviorBase
  {
    private int _maxWanderer;
    private Random _random;

    private bool FiltreBanditClan(Hero hero) => hero.Clan != null && hero.Clan.IsBanditFaction;

    private Hero ResolveHero(
      List<Hero> heroes,
      Settlement settlement,
      MAPatchBehavior.FiltreOk filtreOk)
    {
      int count = heroes.Count;
      int age = (this._maxWanderer - count) % 10 * 10 + 18;
      bool female = (this._maxWanderer - count) % 2 == 1;
      bool otherCulture = (this._maxWanderer - count) % 4 == 3;
      Hero hero = ((IEnumerable<Hero>) heroes).Where<Hero>((Func<Hero, bool>) (x => (double) x.Age >= (double) age && (double) x.Age <= (double) (age + 10) && x.IsFemale == female && (x.Culture == settlement.Culture && !otherCulture || x.Culture != settlement.Culture & otherCulture) && !x.IsFriend(Hero.MainHero))).Random<Hero>(this._random);
      if (hero == null && filtreOk != null)
        hero = ((IEnumerable<Hero>) heroes).Where<Hero>((Func<Hero, bool>) (x => filtreOk(x) && (double) x.Age >= (double) age && (double) x.Age <= (double) (age + 10) && x.IsFemale == female && !x.IsFriend(Hero.MainHero))).Random<Hero>(this._random);
      if (hero == null && filtreOk != null)
        hero = ((IEnumerable<Hero>) heroes).Where<Hero>((Func<Hero, bool>) (x => filtreOk(x) && x.IsFemale == female && !x.IsFriend(Hero.MainHero))).Random<Hero>(this._random);
      if (hero == null && filtreOk != null)
        hero = ((IEnumerable<Hero>) heroes).Where<Hero>((Func<Hero, bool>) (x => filtreOk(x) && !x.IsFriend(Hero.MainHero))).Random<Hero>(this._random);
      if (hero == null)
        hero = ((IEnumerable<Hero>) heroes).Where<Hero>((Func<Hero, bool>) (x => x.Clan == null && (double) x.Age >= (double) age && (double) x.Age <= (double) (age + 10) && x.IsFemale == female && !x.IsFriend(Hero.MainHero))).Random<Hero>(this._random);
      if (hero == null)
        hero = ((IEnumerable<Hero>) heroes).Where<Hero>((Func<Hero, bool>) (x => x.Clan == null && x.IsFemale == female && !x.IsFriend(Hero.MainHero))).Random<Hero>(this._random);
      if (hero == null)
        hero = ((IEnumerable<Hero>) heroes).Where<Hero>((Func<Hero, bool>) (x => x.Clan == null && !x.IsFriend(Hero.MainHero))).Random<Hero>(this._random);
      return hero;
    }

    private void OnSessionLaunched(CampaignGameStarter cgs)
    {
      Helper.MASettingsClean();
      Helper.MAEtape = Helper.Etape.EtapeLoadPas2;
      foreach (Kingdom kingdom in Kingdom.All)
      {
        if (!kingdom.IsEliminated && kingdom.Leader != null && kingdom.Leader.Clan.Kingdom != kingdom)
        {
          Helper.Print(string.Format("PATCH Kingdom will destroy the kingdom {0}", (object) kingdom.Name), Helper.PrintHow.PrintToLogAndWriteAndForceDisplay);
          foreach (Clan clan in (IEnumerable<Clan>) kingdom.Clans)
            Helper.Print(string.Format("with the clan {0}", (object) clan.Name), Helper.PrintHow.PrintToLogAndWrite);
          DestroyKingdomAction.Apply(kingdom);
          kingdom.MainHeroCrimeRating = 0.0f;
        }
      }
      this._maxWanderer = Helper.MASettings.PatchMaxWanderer;
      Helper.Print(string.Format("PatchMaxWanderer Start maxWanderer ?= {0}", (object) this._maxWanderer), Helper.PrintHow.PrintToLogAndWriteAndInit);
      if (this._maxWanderer <= 0)
        return;
      this._random = new Random();
      foreach (Kingdom kingdom in Kingdom.All)
      {
        foreach (Settlement settlement in ((IEnumerable<Settlement>) kingdom.Settlements).Where<Settlement>((Func<Settlement, bool>) (x => x.IsTown)))
        {
          int count = settlement.HeroesWithoutParty.Count;
          Helper.Print(string.Format("PatchMaxWanderer {0} nbHeroWithoutParty ?= {1}", (object) settlement.Name, (object) count), Helper.PrintHow.PrintToLogAndWriteAndInit);
          if (count > this._maxWanderer)
          {
            List<Hero> list = ((IEnumerable<Hero>) settlement.HeroesWithoutParty).ToList<Hero>();
            for (; count > this._maxWanderer; --count)
            {
              Hero hero = this.ResolveHero(list, settlement, new MAPatchBehavior.FiltreOk(this.FiltreBanditClan));
              if (hero != null)
              {
                list.Remove(hero);
                KillCharacterAction.ApplyByRemove(hero, false, true);
              }
              else
                break;
            }
          }
        }
      }
    }

    public override void RegisterEvents() => CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object) this, new Action<CampaignGameStarter>(this.OnSessionLaunched));

    public override void SyncData(IDataStore dataStore)
    {
    }

    private delegate bool FiltreOk(Hero hero);
  }
}
