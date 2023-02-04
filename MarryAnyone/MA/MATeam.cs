// Decompiled with JetBrains decompiler
// Type: MarryAnyone.MA.MATeam
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using MarryAnyone.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;


 
namespace MarryAnyone.MA
{
  internal class MATeam : IDisposable
  {
    protected Team _team;
    internal List<Tuple<Hero, Agent>> _heroes = new List<Tuple<Hero, Agent>>();
    public bool withMainHero;
    public bool withHeroOfPlayerTeam;
    private int _resolu = -1;

    public MATeam(Team team)
    {
      this._team = team;
      foreach (Agent agent1 in ((IEnumerable<Agent>) team.TeamAgents).Where<Agent>((Func<Agent, bool>) (x => x.IsHero)))
      {
        Agent agent = agent1;
        Hero first = Hero.FindFirst((Func<Hero, bool>) (x => ((MBObjectBase) x).StringId == ((MBObjectBase) agent.Character).StringId));
        if (first != null)
        {
          this._heroes.Add(new Tuple<Hero, Agent>(first, agent));
          if (first == Hero.MainHero)
            this.withMainHero = true;
          if (MARomanceCampaignBehavior.Instance.IsPlayerTeam(first))
            this.withHeroOfPlayerTeam = true;
        }
      }
    }

    public Hero CurrentHero() => this._resolu >= 0 ? this._heroes[this._resolu].Item1 : (Hero) null;

    public int Resolve(string stringID)
    {
      this._resolu = this._heroes.FindIndex((Predicate<Tuple<Hero, Agent>>) (x => ((MBObjectBase) x.Item1).StringId == stringID));
      return this._resolu;
    }

    public override string ToString() => this._team == null ? "NULL" : string.Format("MATeam leader {0} Attacker {1}", this._team.Leader != null ? (object) this._team.Leader.Name : (object) "NULL", (object) this._team.IsAttacker);

    public void Dispose()
    {
      this._team = (Team) null;
      this._heroes.Clear();
    }
  }
}
