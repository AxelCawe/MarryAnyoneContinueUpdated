// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.PrisonerCaptureCampaignBehaviorPatch
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;


namespace MarryAnyone.Patches
{
  [HarmonyPatch(typeof (PrisonerCaptureCampaignBehavior))]
  internal class PrisonerCaptureCampaignBehaviorPatch
  {
    [HarmonyPatch(typeof (PrisonerCaptureCampaignBehavior), "HandleSettlementHeroes", new Type[] {typeof (Settlement)})]
    [HarmonyPrefix]
    public static bool PrisonerCaptureCampaignBehaviorPatch_HandleSettlementHeroes(
      Settlement settlement)
    {
      if (settlement.HeroesWithoutParty.Count > 0)
      {
        foreach (Hero hero in ((IEnumerable<Hero>) settlement.HeroesWithoutParty).Where<Hero>(new Func<Hero, bool>(PrisonerCaptureCampaignBehaviorPatch.SettlementHeroCaptureCommonConditionInterne)).ToList<Hero>())
          TakePrisonerAction.Apply(hero.CurrentSettlement.Party, hero);
      }
      foreach (MobileParty mobileParty in ((IEnumerable<MobileParty>) settlement.Parties).Where<MobileParty>((Func<MobileParty, bool>) (x => x.IsLordParty && (x.Army == null || x.Army != null && x.Army.LeaderParty == x) && x.MapEvent == null && PrisonerCaptureCampaignBehaviorPatch.SettlementHeroCaptureCommonConditionInterne(x.LeaderHero))).ToList<MobileParty>())
      {
        LeaveSettlementAction.ApplyForParty(mobileParty);
        SetPartyAiAction.GetActionForPatrollingAroundSettlement(mobileParty, settlement);
      }
      return false;
    }

    private static bool SettlementHeroCaptureCommonConditionInterne(Hero hero) => hero != null && hero != Hero.MainHero && !hero.IsWanderer && hero.HeroState != Hero.CharacterStates.Prisoner && hero.HeroState != Hero.CharacterStates.Dead && hero.MapFaction != null && hero.CurrentSettlement != null && hero.MapFaction.IsAtWarWith(hero.CurrentSettlement.MapFaction);
  }
}
